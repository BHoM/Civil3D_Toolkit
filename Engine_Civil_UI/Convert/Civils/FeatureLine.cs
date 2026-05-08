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


namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Converts a BHoM FeatureLine to a Civil3D FeatureLine.
        /// In Civil3D, FeatureLine objects are typically created through a Surface or directly created
        /// within a transaction context. This method extracts the necessary PI points and bulges.
        /// </summary>
        /// <param name="bhomFeatureLine">The BHoM FeatureLine to convert</param>
        /// <returns>A Civil3D FeatureLine object, or null if conversion fails</returns>
        public static ADC.FeatureLine ToCivil3D(this BHC.FeatureLine bhomFeatureLine)
        {
            if (bhomFeatureLine?.Curve == null)
            {
                BH.Engine.Base.Compute.RecordError("FeatureLine or its curve is null");
                return null;
            }

            // Extract PI points and bulges from the BHoM curve
            if (!ToCivil3D(bhomFeatureLine, out var civPIPoints, out var bulges))
            {
                BH.Engine.Base.Compute.RecordError("Failed to extract PI points and bulges from FeatureLine");
                return null;
            }

            try
            {
                // Attempt to create a FeatureLine with the extracted PI points and bulges
                // Note: FeatureLine requires a specific constructor signature in Civil3D
                // The actual creation depends on the Civil3D API version being used
                
                // Try creating with the List<Point3d> and List<double> pattern
                var featureLine = (ADC.FeatureLine)Activator.CreateInstance(
                    typeof(ADC.FeatureLine),
                    new object[] { civPIPoints, bulges }
                );

                // Set description if provided
                if (!string.IsNullOrEmpty(bhomFeatureLine.Description))
                {
                    featureLine.Description = bhomFeatureLine.Description;
                }

                return featureLine;
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordError(
                    $"Failed to create Civil3D FeatureLine: {ex.Message}. " +
                    $"FeatureLine creation requires a transaction context or surface reference. " +
                    $"Use the extracted PI points and bulges from ToCivil3D(BHC.FeatureLine, out civPIPoints, out bulges) instead.");
                return null;
            }
        }

        /// <summary>
        /// Converts a BHoM curve to Civil3D FeatureLine PI points and bulges.
        /// Note: Creating the actual ADC.FeatureLine requires database transaction context.
        /// This method provides the points and bulges needed for FeatureLine creation.
        /// </summary>
        /// <param name="curve">The BHoM curve to convert (Line, Arc, Polyline, or PolyCurve)</param>
        /// <param name="civPIPoints">Output: List of Civil3D Point3d PI points</param>
        /// <param name="bulges">Output: List of bulge values for segments</param>
        /// <param name="description">Optional description for the FeatureLine</param>
        /// <returns>True if conversion was successful, false otherwise</returns>
        public static bool ToCivil3D(
            this BHG.ICurve curve,
            out List<ACG.Point3d> civPIPoints,
            out List<double> bulges,
            string description = "")
        {
            civPIPoints = new List<ACG.Point3d>();
            bulges = new List<double>();

            if (curve == null)
            {
                BH.Engine.Base.Compute.RecordError("Curve is null");
                return false;
            }

            try
            {
                // Extract PI points and bulges from the BHoM curve
                if (!ExtractPIPointsAndBulges(curve, out var piPoints, out var extractedBulges))
                {
                    BH.Engine.Base.Compute.RecordError("Failed to extract PI points and bulges from curve");
                    return false;
                }

                // Convert BHoM points to Civil3D Point3d format
                foreach (var pt in piPoints)
                {
                    civPIPoints.Add(pt.ToACGPoint3d());
                }

                bulges = new List<double>(extractedBulges);
                return true;
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordError($"Failed to convert BHoM curve to Civil3D FeatureLine data: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Converts a BHoM FeatureLine to Civil3D FeatureLine PI points and bulges.
        /// Note: Creating the actual ADC.FeatureLine requires database transaction context.
        /// </summary>
        /// <param name="bhFeatureLine">The BHoM FeatureLine to convert</param>
        /// <param name="civPIPoints">Output: List of Civil3D Point3d PI points</param>
        /// <param name="bulges">Output: List of bulge values for segments</param>
        /// <returns>True if conversion was successful, false otherwise</returns>
        public static bool ToCivil3D(this BHC.FeatureLine bhFeatureLine,out List<ACG.Point3d> civPIPoints,out List<double> bulges)
        {
            civPIPoints = new List<ACG.Point3d>();
            bulges = new List<double>();

            if (bhFeatureLine?.Curve == null)
            {
                BH.Engine.Base.Compute.RecordError("FeatureLine or its curve is null");
                return false;
            }

            return ToCivil3D(bhFeatureLine.Curve, out civPIPoints, out bulges, bhFeatureLine.Description);
        }


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
                    BHG.Arc arc = BH.UI.Civil.Engine.Convert.BulgeToArc(start, end, bulge);
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
        /**** Methods to Extract PI Points and Bulges  ****/
        /***************************************************/

        /// <summary>
        /// Extracts PI points and bulges from a BHoM curve (Line, Arc, Polyline, or PolyCurve).
        /// </summary>
        /// <param name="curve">The BHoM curve to extract from</param>
        /// <param name="piPoints">Output: List of PI (Point of Intersection) points</param>
        /// <param name="bulges">Output: List of bulge values corresponding to segments</param>
        /// <returns>True if extraction was successful, false otherwise</returns>
        public static bool ExtractPIPointsAndBulges(BHG.ICurve curve, out List<BHG.Point> piPoints, out List<double> bulges)
        {
            piPoints = new List<BHG.Point>();
            bulges = new List<double>();

            if (curve == null)
                return false;

            try
            {
                switch (curve)
                {
                    case BHG.Line line:
                        return ExtractFromLine(line, out piPoints, out bulges);

                    case BHG.Arc arc:
                        return ExtractFromArc(arc, out piPoints, out bulges);

                    case BHG.Polyline polyline:
                        return ExtractFromPolyline(polyline, out piPoints, out bulges);

                    case BHG.PolyCurve polyCurve:
                        return ExtractFromPolyCurve(polyCurve, out piPoints, out bulges);

                    default:
                        BH.Engine.Base.Compute.RecordWarning($"Unsupported curve type: {curve.GetType().Name}. Supported types: Line, Arc, Polyline, PolyCurve");
                        return false;
                }
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordError($"Error extracting PI points and bulges: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Extracts PI points and bulges from a Line (2 points, no bulges).
        /// </summary>
        private static bool ExtractFromLine(BHG.Line line, out List<BHG.Point> piPoints, out List<double> bulges)
        {
            piPoints = new List<BHG.Point> { line.Start, line.End };
            bulges = new List<double> { 0.0 };
            return true;
        }

        /// <summary>
        /// Extracts PI points and bulges from an Arc (2 points with bulge).
        /// </summary>
        private static bool ExtractFromArc(BHG.Arc arc, out List<BHG.Point> piPoints, out List<double> bulges)
        {
            piPoints = new List<BHG.Point>
            {
                ArcStartPoint(arc),
                ArcEndPoint(arc)
            };

            // Convert arc to bulge value
            double bulge = ArcToBulge(arc);
            bulges = new List<double> { bulge };
            return true;
        }

        /// <summary>
        /// Extracts PI points and bulges from a Polyline (N points, N-1 zero bulges).
        /// </summary>
        private static bool ExtractFromPolyline(BHG.Polyline polyline, out List<BHG.Point> piPoints, out List<double> bulges)
        {
            if (polyline?.ControlPoints == null || polyline.ControlPoints.Count < 2)
            {
                piPoints = new List<BHG.Point>();
                bulges = new List<double>();
                return false;
            }

            piPoints = new List<BHG.Point>(polyline.ControlPoints);
            
            // All segments in a polyline have zero bulge (straight lines)
            bulges = new List<double>();
            for (int i = 0; i < piPoints.Count - 1; i++)
            {
                bulges.Add(0.0);
            }

            return true;
        }

        /// <summary>
        /// Extracts PI points and bulges from a PolyCurve (concatenates segments).
        /// </summary>
        private static bool ExtractFromPolyCurve(BHG.PolyCurve polyCurve, out List<BHG.Point> piPoints, out List<double> bulges)
        {
            piPoints = new List<BHG.Point>();
            bulges = new List<double>();

            if (polyCurve?.Curves == null || polyCurve.Curves.Count == 0)
                return false;

            // Process each segment in the polycurve
            for (int i = 0; i < polyCurve.Curves.Count; i++)
            {
                var segment = polyCurve.Curves[i];

                if (segment is BHG.Line line)
                {
                    // Add start point if first segment or connect to previous end
                    if (i == 0)
                        piPoints.Add(line.Start);

                    piPoints.Add(line.End);
                    bulges.Add(0.0);
                }
                else if (segment is BHG.Arc arc)
                {
                    // Add start point if first segment
                    var arcStart = ArcStartPoint(arc);
                    var arcEnd = ArcEndPoint(arc);

                    if (i == 0)
                        piPoints.Add(arcStart);

                    piPoints.Add(arcEnd);
                    double bulge = ArcToBulge(arc);
                    bulges.Add(bulge);
                }
                else if (segment is BHG.Polyline polyline)
                {
                    // Add polyline control points
                    if (polyline?.ControlPoints != null && polyline.ControlPoints.Count >= 2)
                    {
                        // Skip first point if not the first segment (already added as end of previous)
                        int startIdx = (i == 0) ? 0 : 1;
                        for (int j = startIdx; j < polyline.ControlPoints.Count; j++)
                        {
                            piPoints.Add(polyline.ControlPoints[j]);
                            
                            // Add zero bulge for each segment
                            if (j < polyline.ControlPoints.Count - 1 || i < polyCurve.Curves.Count - 1)
                                bulges.Add(0.0);
                        }
                    }
                }
                else
                {
                    BH.Engine.Base.Compute.RecordWarning($"PolyCurve contains unsupported segment type: {segment.GetType().Name}");
                }
            }

            // Verify we have valid data
            return piPoints.Count >= 2 && bulges.Count == piPoints.Count - 1;
        }

        /***************************************************/
    }
}


