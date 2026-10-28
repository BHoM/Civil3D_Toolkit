/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

using BH.Engine.Geometry;
using BHC = BH.oM.Adapters.Civil3D.Elements;
using BH.oM.Base.Attributes;
using BHG = BH.oM.Geometry;

using ACG = Autodesk.AutoCAD.Geometry;
using ADC = Autodesk.Civil.DatabaseServices;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Converts a Civil 3D FeatureLine to BH.oM.Adapters.Civil3D.Elements.FeatureLine. Straight segments are split at the feature line's elevation points.")]
        [Input("civFeatureLine", "A Civil 3D FeatureLine to convert.")]
        [Output("featureLine", "The converted BH.oM.Adapters.Civil3D.Elements.FeatureLine.")]
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
                var start = piPoints[i].ToCivil3D();
                var end = piPoints[i + 1].ToCivil3D();
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
                    BHG.Arc arc = start.BulgeToArc(end, bulge);
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
        /**** Private Methods                           ****/
        /***************************************************/

        private static BHG.PolyCurve ApplyElevationPoints(BHG.PolyCurve polyCurve, List<BHG.Point> elevationPoints)
        {
            List<BHG.ICurve> segments = new List<BHG.ICurve>();
            foreach (BHG.ICurve segment in polyCurve.Curves)
                segments.AddRange(SplitSegmentAtElevationPoints(segment, elevationPoints));

            return new BHG.PolyCurve { Curves = segments };
        }

        /***************************************************/

        //Splits a straight segment at every elevation point that lies on it in plan, in order along the segment, taking each
        //split point's Z from its elevation point. Arcs are returned unchanged.
        private static List<BHG.ICurve> SplitSegmentAtElevationPoints(BHG.ICurve segment, List<BHG.Point> elevationPoints)
        {
            BHG.Line line = segment as BHG.Line;
            if (line == null)
                return new List<BHG.ICurve> { segment };

            List<(double Parameter, BHG.Point Point)> splits = new List<(double, BHG.Point)>();
            foreach (BHG.Point elevationPoint in elevationPoints)
            {
                double parameter;
                if (TryParameterInPlan(line, elevationPoint, out parameter))
                    splits.Add((parameter, elevationPoint));
            }

            if (splits.Count == 0)
                return new List<BHG.ICurve> { segment };

            List<BHG.ICurve> result = new List<BHG.ICurve>();
            BHG.Point start = line.Start;
            foreach ((double Parameter, BHG.Point Point) split in splits.OrderBy(x => x.Parameter))
            {
                BHG.Point onLine = line.Start + (line.End - line.Start) * split.Parameter;
                BHG.Point end = new BHG.Point { X = onLine.X, Y = onLine.Y, Z = split.Point.Z };
                result.Add(new BHG.Line { Start = start, End = end });
                start = end;
            }

            result.Add(new BHG.Line { Start = start, End = line.End });
            return result;
        }

        /***************************************************/

        //Gets where a point falls along a line in plan, as a parameter between 0 and 1. Fails if the point is off the line in plan,
        //or at one of its ends (which are already vertices).
        private static bool TryParameterInPlan(BHG.Line line, BHG.Point point, out double parameter)
        {
            parameter = 0.0;
            const double tolerance = 1e-3;

            BHG.Vector direction = new BHG.Vector { X = line.End.X - line.Start.X, Y = line.End.Y - line.Start.Y };
            double length = direction.Length();
            if (length < tolerance)
                return false;

            BHG.Vector toPoint = new BHG.Vector { X = point.X - line.Start.X, Y = point.Y - line.Start.Y };
            double along = toPoint.DotProduct(direction) / length;
            if (along <= tolerance || along >= length - tolerance)
                return false;

            double offset = System.Math.Abs(toPoint.X * direction.Y - toPoint.Y * direction.X) / length;
            if (offset > tolerance)
                return false;

            parameter = along / length;
            return true;
        }

        /***************************************************/
    }
}
