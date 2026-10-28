/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHC = BH.oM.Civils.Elements;
using BHG = BH.oM.Geometry;

using ACG = Autodesk.AutoCAD.Geometry;

using ADC = Autodesk.Civil.DatabaseServices;


namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        ///
        /// From Civil3D FeatureLine to BHoM FeatureLine, including handling of elevation points
        /// 
        public static BHC.FeatureLine FromCivil3D(this ADC.FeatureLine civFeatureLine)
        {
            var piPoints = new List<BHG.Point>();
            var bulges = new List<double>();
            var elevationPoints = new List<BHG.Point>();

            // Get PI Points with their bulges
            var civPIPoints = civFeatureLine.GetPoints(Autodesk.Civil.FeatureLinePointType.PIPoint);
            for (int i = 0; i < civPIPoints.Count; i++)
            {
                var pt = civPIPoints[i];
                piPoints.Add(new BHG.Point { X = pt.X, Y = pt.Y, Z = pt.Z });
                
                // Add bulge if not the last point
                if (i < civPIPoints.Count - 1)
                {
                    bulges.Add(civFeatureLine.GetBulge(i));
                }
            }

            // Create PolyCurve from PI Points and bulges (same logic as polyline conversion)
            var segments = new List<BHG.ICurve>();
            for (int i = 0; i < piPoints.Count - 1; i++)
            {
                var start = piPoints[i].ToACGPoint3d();
                var end = piPoints[i + 1].ToACGPoint3d();
                double bulge = bulges[i];

                if (Math.Abs(bulge) < 1e-12)
                {
                    // Straight segment
                    segments.Add(new BHG.Line
                    {
                        Start = piPoints[i],
                        End = piPoints[i + 1]
                    });
                }
                else
                {
                    // Arc segment via bulge conversion
                    BHG.Arc arc = BH.Civil3D.Engine.Core.Convert.BulgeToArc(start, end, bulge);
                    segments.Add(arc);
                }
            }
            var polyCurve = new BHG.PolyCurve { Curves = segments };

            // Get Elevation Points and apply to segments
            var civElevPoints = civFeatureLine.GetPoints(Autodesk.Civil.FeatureLinePointType.ElevationPoint);
            foreach (var pt in civElevPoints)
            {
                var point3d = (ACG.Point3d)pt;
                elevationPoints.Add(new BHG.Point { X = point3d.X, Y = point3d.Y, Z = point3d.Z });
            }

            // Apply elevation data: split segments at elevation points and update Z values
            if (elevationPoints.Count > 0)
            {
                polyCurve = ApplyElevationPoints(polyCurve, elevationPoints);
            }

            return new BHC.FeatureLine
            {
                Description = civFeatureLine.Description,
                Curve = polyCurve
            };
        }


        /***************************************************/
        /**** Helper Methods  ****/
        /***************************************************/
     
        private static BHG.PolyCurve ApplyElevationPoints(BHG.PolyCurve polyCurve, List<BHG.Point> elevationPoints)
        {
            // For each elevation point, find the closest location on the polycurve
            // and split segments at that location, updating Z values
            
            var newSegments = new List<BHG.ICurve>();
            var startPt = (polyCurve.Curves.Count > 0 && polyCurve.Curves[0] is BHG.Line line) ? line.Start : new BHG.Point { X = 0, Y = 0, Z = 0 };
            var sortedElevPoints = elevationPoints.OrderBy(p => Math.Sqrt((p.X - startPt.X) * (p.X - startPt.X) + (p.Y - startPt.Y) * (p.Y - startPt.Y))).ToList();
            
            foreach (var segment in polyCurve.Curves)
            {
                var newSegs = SplitSegmentAtElevationPoints(segment, sortedElevPoints);
                newSegments.AddRange(newSegs);
            }

            return new BHG.PolyCurve { Curves = newSegments };
        }

        private static List<BHG.ICurve> SplitSegmentAtElevationPoints(BHG.ICurve segment, List<BHG.Point> elevationPoints)
        {
            var result = new List<BHG.ICurve> { segment };

            // For each elevation point, check if it projects onto this segment
            foreach (var elevPt in elevationPoints)
            {
                var updated = new List<BHG.ICurve>();
                
                foreach (var seg in result)
                {
                    // Check if elevation point is on or near this segment
                    var projPt = ProjectPointOnSegment(elevPt, seg);
                    
                    if (projPt != null && IsPointOnSegment(projPt, seg))
                    {
                        // Split segment, updating Z from elevation point
                        var split = SplitSegment(seg, projPt, elevPt.Z);
                        updated.AddRange(split);
                    }
                    else
                    {
                        updated.Add(seg);
                    }
                }
                
                result = updated;
            }

            return result;
        }

        private static BHG.Point ProjectPointOnSegment(BHG.Point point, BHG.ICurve segment)
        {
            if (segment is BHG.Line line)
            {
                var start = line.Start;
                var end = line.End;
                var v = end - start;
                var w = new BHG.Vector { X = point.X - start.X, Y = point.Y - start.Y, Z = 0 };
                double t = BH.Engine.Geometry.Query.DotProduct(w, v) / BH.Engine.Geometry.Query.DotProduct(v, v);
                t = Math.Max(0, Math.Min(1, t));
                return start + t * v;
            }
            return null;
        }

        private static bool IsPointOnSegment(BHG.Point projPoint, BHG.ICurve segment)
        {
            if (segment is BHG.Line line)
            {
                var midPt = new BHG.Point 
                { 
                    X = (line.Start.X + line.End.X) / 2, 
                    Y = (line.Start.Y + line.End.Y) / 2,
                    Z = (line.Start.Z + line.End.Z) / 2
                };
                double dx = projPoint.X - midPt.X;
                double dy = projPoint.Y - midPt.Y;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                return dist < 0.001; // tolerance
            }
            return false;
        }

        private static List<BHG.ICurve> SplitSegment(BHG.ICurve segment, BHG.Point splitPt, double elevZ)
        {
            var result = new List<BHG.ICurve>();

            if (segment is BHG.Line line)
            {
                var updateSplitPt = new BHG.Point { X = splitPt.X, Y = splitPt.Y, Z = elevZ };
                result.Add(new BHG.Line { Start = line.Start, End = updateSplitPt });
                result.Add(new BHG.Line { Start = updateSplitPt, End = line.End });
            }
            else
            {
                result.Add(segment); // For arcs, just return as is for now
            }

            return result;
        }

        /***************************************************/
    }
}


