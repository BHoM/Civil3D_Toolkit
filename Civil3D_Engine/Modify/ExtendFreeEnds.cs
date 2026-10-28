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
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using BH.oM.Quantities.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Civil3D
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Extends the free ends of curves to nearby straight segments. A free end is a start or end point that does not touch any other curve's end. " +
                     "For each free end, straight segments of the other curves within the search radius are tried, closest first: the end is extended along its outward direction until it meets a segment, " +
                     "and the extension is kept only if it adds no more than the maximum extension length. Where several targets qualify, the shortest extension wins.")]
        [Input("curves", "Curves to extend. Lines, polylines and the straight parts of polycurves can act as targets.")]
        [Input("coincidentTolerance", "Distance within which two curve ends count as touching, so neither is a free end.", typeof(Length))]
        [Input("searchRadius", "Maximum distance from a free end to a target segment for the segment to be considered.", typeof(Length))]
        [Input("maxExtension", "Maximum length that may be added to a curve end.", typeof(Length))]
        [Input("tangentExtensions", "If true, curved ends are extended along their tangent; if false, they are extended along the curve.")]
        [Input("tolerance", "Distance tolerance for geometric checks, including how close an extended end must come to a target to count as meeting it.", typeof(Length))]
        [Output("curves", "Copies of the input curves, with free ends extended where a target was found. Unchanged curves are returned as they are.")]
        public static List<ICurve> ExtendFreeEnds(this List<ICurve> curves, double coincidentTolerance = Tolerance.Distance, double searchRadius = 3.0, double maxExtension = 15.0, bool tangentExtensions = true, double tolerance = Tolerance.Distance)
        {
            if (curves == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot extend the free ends of a null list of curves.");
                return new List<ICurve>();
            }

            if (curves.Count > 2000)
                BH.Engine.Base.Compute.RecordWarning("Extending free ends compares every curve end with every other curve, so it may be slow for more than 2000 curves.");

            List<List<Line>> segmentsByCurve = curves.Select(x => LineSegments(x)).ToList();

            List<(int CurveIndex, bool IsStart, Point Point)> ends = new List<(int, bool, Point)>();
            for (int i = 0; i < curves.Count; i++)
            {
                if (curves[i] == null)
                    continue;

                Point start = curves[i].IStartPoint();
                if (start != null)
                    ends.Add((i, true, start));

                Point end = curves[i].IEndPoint();
                if (end != null)
                    ends.Add((i, false, end));
            }

            List<(int CurveIndex, bool IsStart, Point Point)> freeEnds = ends
                .Where(x => !ends.Any(y => !ReferenceEquals(x.Point, y.Point) && x.Point.Distance(y.Point) <= coincidentTolerance))
                .ToList();

            List<ICurve> output = new List<ICurve>(curves);

            foreach ((int CurveIndex, bool IsStart, Point Point) freeEnd in freeEnds)
            {
                ICurve source = output[freeEnd.CurveIndex];

                Vector direction = OutwardDirection(source, freeEnd.IsStart, tolerance);
                if (direction == null)
                    continue;

                List<(Line Segment, double Distance)> candidates = new List<(Line, double)>();
                for (int i = 0; i < segmentsByCurve.Count; i++)
                {
                    if (i == freeEnd.CurveIndex)
                        continue;

                    foreach (Line segment in segmentsByCurve[i])
                    {
                        double distance = freeEnd.Point.Distance(segment);
                        if (distance <= searchRadius)
                            candidates.Add((segment, distance));
                    }
                }

                double originalLength = source.ILength();
                ICurve best = null;
                double bestAdded = double.PositiveInfinity;

                foreach ((Line Segment, double Distance) candidate in candidates.OrderBy(x => x.Distance))
                {
                    double along;
                    if (!RayHitsSegment(freeEnd.Point, direction, candidate.Segment, tolerance, out along))
                        continue;

                    ICurve extended = source.IExtend(freeEnd.IsStart ? along : 0.0, freeEnd.IsStart ? 0.0 : along, tangentExtensions, tolerance);
                    if (extended == null)
                        continue;

                    double added = extended.ILength() - originalLength;
                    if (added <= maxExtension + tolerance && added < bestAdded)
                    {
                        bestAdded = added;
                        best = extended;
                    }
                }

                if (best != null)
                    output[freeEnd.CurveIndex] = best;
            }

            return output;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        //Unit vector pointing away from the curve at the given end, from the end segment where it is straight, otherwise from the chord.
        private static Vector OutwardDirection(ICurve curve, bool isStart, double tolerance)
        {
            Line endSegment = null;

            if (curve is Line)
            {
                endSegment = (Line)curve;
            }
            else if (curve is Polyline)
            {
                List<Line> parts = ((Polyline)curve).SubParts();
                if (parts.Count > 0)
                    endSegment = isStart ? parts.First() : parts.Last();
            }
            else if (curve is PolyCurve)
            {
                List<Line> lines = ((PolyCurve)curve).SubParts().OfType<Line>().Where(x => x.Start != null && x.End != null && x.Length() >= tolerance).ToList();
                if (lines.Count > 0)
                    endSegment = isStart ? lines.First() : lines.Last();
            }

            Point from = endSegment != null ? endSegment.Start : curve.IStartPoint();
            Point to = endSegment != null ? endSegment.End : curve.IEndPoint();
            if (from == null || to == null)
                return null;

            Vector direction = to - from;
            if (direction.Length() < tolerance)
                return null;

            direction = direction.Normalise();
            return isStart ? direction * -1.0 : direction;
        }

        /***************************************************/

        //Straight segments of a curve that free ends can be extended to.
        private static List<Line> LineSegments(ICurve curve)
        {
            List<Line> segments = new List<Line>();

            if (curve is Line)
                segments.Add((Line)curve);
            else if (curve is Polyline)
                segments.AddRange(((Polyline)curve).SubParts());
            else if (curve is PolyCurve)
                segments.AddRange(((PolyCurve)curve).SubParts().OfType<Line>());

            return segments.Where(x => x != null && x.Start != null && x.End != null).ToList();
        }

        /***************************************************/

        //Checks whether the ray from the origin along the unit direction meets the segment within the tolerance, and returns the distance along the ray.
        private static bool RayHitsSegment(Point origin, Vector direction, Line segment, double tolerance, out double along)
        {
            along = 0.0;

            Vector v = segment.End - segment.Start;
            Vector w0 = origin - segment.Start;

            double b = direction.DotProduct(v);
            double c = v.DotProduct(v);
            double d = direction.DotProduct(w0);
            double e = v.DotProduct(w0);

            double denominator = c - b * b;
            double t = denominator > 1e-12 ? (e - b * d) / denominator : 0.0;
            t = System.Math.Max(0.0, System.Math.Min(1.0, t));

            Point onSegment = segment.Start + v * t;
            double s = (onSegment - origin).DotProduct(direction);
            if (s <= 0.0)
                return false;

            Point onRay = origin + direction * s;
            if (onRay.Distance(onSegment) > tolerance)
                return false;

            along = s;
            return true;
        }

        /***************************************************/
    }
}
