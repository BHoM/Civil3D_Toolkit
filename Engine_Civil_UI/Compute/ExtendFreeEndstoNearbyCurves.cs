using System;
using System.Collections.Generic;
using System.Linq;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;

using BH.Engine.Geometry;
using BH.oM.Base;
using BH.Engine.Base;
using System.ComponentModel;

namespace BH.UI.Civil.Engine

{
    public static partial class Modify
    {
        /// <summary>
        /// Spec:
        /// 1) Find free ends (endpoints not coincident with any other curve endpoint).
        /// 2) For each free end, collect candidate target LINE SEGMENTS within candidateRadius of the endpoint.
        /// 3) Attempt extension to those target segments (ray from end intersects segment within snapTol).
        /// 4) If extension succeeds, measure added length = extended.ILength - original.ILength.
        /// 5) Keep only if added length <= maxExtensionLength; otherwise revert.
        ///
        /// Robustness:
        /// - Fully null-safe: will not subtract null Points (prevents Point.op_Subtraction crash).
        /// - Skips curves that cannot provide endpoints.
        /// - Targets are extracted as line segments from Line / Polyline / PolyCurve (linear parts only).
        /// - Uses Extend.cs IExtend dispatcher for actual extension. [2](https://burohappold-my.sharepoint.com/personal/joe_walton_burohappold_com/Documents/Microsoft%20Copilot%20Chat%20Files/Extend.cs?web=1)
        /// </summary>
        public static List<ICurve> ExtendFreeEnds_ToNearbyTargetLines(
            IEnumerable<ICurve> curves,
            double coincidentTol = 1e-3,
            double candidateRadius = 3.0,
            double maxExtensionLength = 15.0,
            double snapTol = 1e-3,
            bool tangentExtensions = true,
            double tolerance = Tolerance.Distance,
            int maxSegmentsPerCurve = 500,
            int maxTotalPairs = 1500000
        )
        {
            if (curves == null)
                return new List<ICurve>();

            // Filter null curves up-front
            List<ICurve> list = curves.Where(c => c != null).ToList();
            if (list.Count == 0)
                return new List<ICurve>();

            // GH safety guard for big sets
            long roughPairs = (long)list.Count * (long)list.Count;
            if (roughPairs > maxTotalPairs)
            {
                BH.Engine.Base.Compute.RecordWarning($"ExtendFreeEnds_ToNearbyTargetLines: input too large for O(n^2) processing (pairs={roughPairs}). Reduce inputs or raise maxTotalPairs.");
                return list;
            }

            // Pre-extract candidate target line segments for each curve
            List<List<Line>> segsByCurve = new List<List<Line>>(list.Count);
            for (int i = 0; i < list.Count; i++)
                segsByCurve.Add(ExtractLineSegments(list[i], maxSegmentsPerCurve));

            // Build endpoint list (skip null endpoints)
            List<EndRef> allEnds = new List<EndRef>(list.Count * 2);
            for (int i = 0; i < list.Count; i++)
            {
                ICurve c = list[i];

                Point s = SafeStartPoint(c, tolerance);
                if (s != null)
                    allEnds.Add(new EndRef(i, true, s));

                Point e = SafeEndPoint(c, tolerance);
                if (e != null)
                    allEnds.Add(new EndRef(i, false, e));
            }

            if (allEnds.Count == 0)
                return list;

            // Find free ends (not coincident with any other endpoint)
            List<EndRef> freeEnds = new List<EndRef>();
            for (int i = 0; i < allEnds.Count; i++)
            {
                bool coincident = false;
                for (int j = 0; j < allEnds.Count; j++)
                {
                    if (i == j) continue;

                    // null-safe distance
                    if (Distance(allEnds[i].Point, allEnds[j].Point) <= coincidentTol)
                    {
                        coincident = true;
                        break;
                    }
                }
                if (!coincident)
                    freeEnds.Add(allEnds[i]);
            }

            if (freeEnds.Count == 0)
                return list;

            // Output copy
            List<ICurve> output = new List<ICurve>(list);

            // Process each free end
            for (int k = 0; k < freeEnds.Count; k++)
            {
                EndRef endRef = freeEnds[k];
                ICurve source = output[endRef.CurveIndex];
                if (source == null || endRef.Point == null)
                    continue;

                // Outward direction at the end (linear-first, chord fallback)
                Vector dir = OutwardDirection(source, endRef.IsStart, tolerance);
                if (dir == null || dir.Length() < tolerance)
                    continue;

                dir = dir.Normalise();
                Point endPt = endRef.Point;

                // Candidate target segments within radius of end point
                List<TargetSeg> candidates = new List<TargetSeg>();

                for (int ti = 0; ti < output.Count; ti++)
                {
                    if (ti == endRef.CurveIndex) continue;

                    List<Line> tSegs = segsByCurve[ti];
                    if (tSegs == null || tSegs.Count == 0) continue;

                    for (int si = 0; si < tSegs.Count; si++)
                    {
                        Line seg = tSegs[si];
                        if (seg == null || seg.Start == null || seg.End == null)
                            continue;

                        double d = DistancePointToSegment(endPt, seg.Start, seg.End);
                        if (d <= candidateRadius)
                            candidates.Add(new TargetSeg(ti, seg, d));
                    }
                }

                if (candidates.Count == 0)
                    continue;

                // Try closest candidates first
                candidates.Sort((a, b) => a.DistanceToEnd.CompareTo(b.DistanceToEnd));

                double originalLen = SafeILength(source);

                bool applied = false;
                ICurve bestCurve = null;
                double bestDelta = double.PositiveInfinity;

                // Attempt extension to each candidate
                for (int ci = 0; ci < candidates.Count; ci++)
                {
                    Line seg = candidates[ci].Segment;

                    double along;
                    if (!RayHitsSegment(endPt, dir, seg.Start, seg.End, snapTol, out along))
                        continue;

                    // Apply extension by 'along' (positive)
                    double startExt = endRef.IsStart ? along : 0.0;
                    double endExt = endRef.IsStart ? 0.0 : along;

                    ICurve extended = null;
                    try
                    {
                        // Extension dispatcher is in your Extend.cs: IExtend(this ICurve...) [2](https://burohappold-my.sharepoint.com/personal/joe_walton_burohappold_com/Documents/Microsoft%20Copilot%20Chat%20Files/Extend.cs?web=1)
                        extended = source.IExtend(startExt, endExt, tangentExtensions, tolerance);
                    }
                    catch (Exception ex)
                    {
                        BH.Engine.Base.Compute.RecordWarning($"Extend attempt failed on curve index {endRef.CurveIndex}: {ex.Message}");
                        continue;
                    }

                    if (extended == null)
                        continue;

                    double newLen = SafeILength(extended);
                    double delta = newLen - originalLen;

                    // Keep only if within max extension length
                    if (delta > maxExtensionLength + tolerance)
                        continue;

                    // Keep best (smallest delta)
                    if (delta < bestDelta)
                    {
                        bestDelta = delta;
                        bestCurve = extended;
                        applied = true;
                    }
                }

                if (applied && bestCurve != null)
                    output[endRef.CurveIndex] = bestCurve;
            }

            return output;
        }

        // --------------------------------------------------------------------
        // Outward direction at an end
        // --------------------------------------------------------------------
        private static Vector OutwardDirection(ICurve curve, bool isStart, double tolerance)
        {
            if (curve == null) return null;

            // Line
            Line ln = curve as Line;
            if (ln != null && ln.Start != null && ln.End != null)
            {
                Vector d = ln.End - ln.Start;
                if (d.Length() < tolerance) return null;
                d = d.Normalise();
                return isStart ? (d * -1.0) : d;
            }

            // Polyline: first/last segment
            Polyline pl = curve as Polyline;
            if (pl != null)
            {
                try
                {
                    var parts = pl.SubParts();
                    if (parts != null && parts.Count > 0)
                    {
                        Line seg = isStart ? parts[0] : parts[parts.Count - 1];
                        if (seg != null && seg.Start != null && seg.End != null)
                        {
                            Vector d = seg.End - seg.Start;
                            if (d.Length() < tolerance) return null;
                            d = d.Normalise();
                            return isStart ? (d * -1.0) : d;
                        }
                    }
                }
                catch { }
            }

            // PolyCurve: try first/last LINE subpart if available
            PolyCurve pc = curve as PolyCurve;
            if (pc != null)
            {
                try
                {
                    var parts = pc.SubParts();
                    if (parts != null && parts.Count > 0)
                    {
                        if (isStart)
                        {
                            for (int i = 0; i < parts.Count; i++)
                            {
                                Line seg = parts[i] as Line;
                                if (seg == null || seg.Start == null || seg.End == null) continue;
                                Vector d = seg.End - seg.Start;
                                if (d.Length() < tolerance) continue;
                                return (d.Normalise() * -1.0);
                            }
                        }
                        else
                        {
                            for (int i = parts.Count - 1; i >= 0; i--)
                            {
                                Line seg = parts[i] as Line;
                                if (seg == null || seg.Start == null || seg.End == null) continue;
                                Vector d = seg.End - seg.Start;
                                if (d.Length() < tolerance) continue;
                                return d.Normalise();
                            }
                        }
                    }
                }
                catch { }
            }

            // Fallback: chord from endpoints
            Point s = SafeStartPoint(curve, tolerance);
            Point e = SafeEndPoint(curve, tolerance);
            if (s == null || e == null) return null;

            Vector chord = e - s;
            if (chord.Length() < tolerance) return null;

            chord = chord.Normalise();
            return isStart ? (chord * -1.0) : chord;
        }

        // --------------------------------------------------------------------
        // Extract linear target segments from curve
        // --------------------------------------------------------------------
        private static List<Line> ExtractLineSegments(ICurve curve, int maxSegments)
        {
            List<Line> result = new List<Line>();
            if (curve == null || maxSegments <= 0) return result;

            Line ln = curve as Line;
            if (ln != null)
            {
                if (ln.Start != null && ln.End != null)
                    result.Add(ln);
                return result;
            }

            Polyline pl = curve as Polyline;
            if (pl != null)
            {
                try
                {
                    var parts = pl.SubParts();
                    if (parts != null)
                    {
                        for (int i = 0; i < parts.Count && result.Count < maxSegments; i++)
                        {
                            Line seg = parts[i];
                            if (seg != null && seg.Start != null && seg.End != null)
                                result.Add(seg);
                        }
                    }
                }
                catch { }
                return result;
            }

            PolyCurve pc = curve as PolyCurve;
            if (pc != null)
            {
                try
                {
                    var parts = pc.SubParts();
                    if (parts != null)
                    {
                        for (int i = 0; i < parts.Count && result.Count < maxSegments; i++)
                        {
                            Line seg = parts[i] as Line;
                            if (seg != null && seg.Start != null && seg.End != null)
                                result.Add(seg);
                        }
                    }
                }
                catch { }
                return result;
            }

            return result;
        }

        // --------------------------------------------------------------------
        // Candidate test: does the ray from end hit the segment (within snapTol)?
        // Uses closest approach ray-segment, requires separation <= snapTol
        // --------------------------------------------------------------------
        private static bool RayHitsSegment(Point rayOrigin, Vector rayDirUnit, Point a, Point b, double snapTol, out double along)
        {
            along = 0.0;

            if (rayOrigin == null || rayDirUnit == null || a == null || b == null)
                return false;

            double sRay, tSeg, dist;
            Point pRay, pSeg;
            ClosestPoints_Ray_Segment(rayOrigin, rayDirUnit, a, b, out sRay, out tSeg, out pRay, out pSeg, out dist);

            if (sRay <= 0.0) return false;
            if (dist > snapTol) return false;

            along = sRay;
            return true;
        }

        // Ray: O + s*D (s>=0), Segment: A + t*(B-A) (t in [0,1])
        private static void ClosestPoints_Ray_Segment(
            Point o, Vector dUnit,
            Point a, Point b,
            out double sRay, out double tSeg,
            out Point pRay, out Point pSeg,
            out double dist)
        {
            sRay = 0.0; tSeg = 0.0; dist = double.PositiveInfinity;
            pRay = null; pSeg = null;

            Vector v = b - a;
            Vector w0 = o - a;

            double B = Dot(dUnit, v);
            double C = Dot(v, v);
            double D = Dot(dUnit, w0);
            double E = Dot(v, w0);

            double denom = C - B * B;
            double s = 0.0;
            double t = 0.0;

            if (Math.Abs(denom) > 1e-12)
            {
                s = (B * E - C * D) / denom;
                t = (E - B * D) / denom;
            }

            if (t < 0.0) t = 0.0;
            if (t > 1.0) t = 1.0;

            Point q = a + v * t;
            s = Dot(q - o, dUnit);
            if (s < 0.0) s = 0.0;

            pRay = o + dUnit * s;
            pSeg = q;

            sRay = s;
            tSeg = t;
            dist = Distance(pRay, pSeg);
        }

        // --------------------------------------------------------------------
        // Point-to-segment distance (for candidateRadius filtering)
        // --------------------------------------------------------------------
        private static double DistancePointToSegment(Point p, Point a, Point b)
        {
            if (p == null || a == null || b == null)
                return double.PositiveInfinity;

            Vector ab = b - a;
            double ab2 = Dot(ab, ab);
            if (ab2 < 1e-18)
                return Distance(p, a);

            double t = Dot(p - a, ab) / ab2;
            if (t < 0.0) t = 0.0;
            if (t > 1.0) t = 1.0;

            Point q = a + ab * t;
            return Distance(p, q);
        }

        // --------------------------------------------------------------------
        // Safe endpoints: prefer interface wrappers when present; else dynamic fallback
        // (Your internal patterns use IStartPoint/IEndPoint.) [1](https://burohappold-my.sharepoint.com/personal/joe_walton_burohappold_com/_layouts/15/Doc.aspx?action=edit&mobileredirect=true&wdorigin=Sharepoint&DefaultItemOpen=1&sourcedoc={49b87c87-ce4b-45be-9e29-2a42b749e12f}&wd=target(/0_NonProject/Automation.one/)&wdpartid={9557023a-96d9-1c09-12e8-4af93a182135}{1}&wdsectionfileid={80ba111e-bb4d-4979-bd51-101470e69015})
        // --------------------------------------------------------------------
        private static Point SafeStartPoint(ICurve curve, double tolerance)
        {
            if (curve == null) return null;

            try
            {
                // If IStartPoint wrapper exists in this build, this is the safest route
                return curve.IStartPoint();
            }
            catch
            {
                // Fallback for Line
                Line ln = curve as Line;
                if (ln != null) return ln.Start;

                // Last resort: dynamic StartPoint()
                try { dynamic dc = curve; return (Point)dc.StartPoint(); }
                catch { return null; }
            }
        }

        private static Point SafeEndPoint(ICurve curve, double tolerance)
        {
            if (curve == null) return null;

            try
            {
                return curve.IEndPoint();
            }
            catch
            {
                Line ln = curve as Line;
                if (ln != null) return ln.End;

                try { dynamic dc = curve; return (Point)dc.EndPoint(); }
                catch { return null; }
            }
        }

        // --------------------------------------------------------------------
        // Safe ILength: prefer ILength wrapper when present; fallback to basic
        // --------------------------------------------------------------------
        private static double SafeILength(ICurve curve)
        {
            if (curve == null) return double.NaN;

            try
            {
                return curve.ILength();
            }
            catch
            {
                Line ln = curve as Line;
                if (ln != null && ln.Start != null && ln.End != null)
                    return (ln.End - ln.Start).Length();

                return double.NaN;
            }
        }

        // --------------------------------------------------------------------
        // Null-safe distance (prevents Point.op_Subtraction null crash)
        // --------------------------------------------------------------------
        private static double Distance(Point a, Point b)
        {
            if (a == null || b == null)
                return double.PositiveInfinity;

            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            double dz = a.Z - b.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private static double Dot(Vector a, Vector b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        private static double Length(this Vector v)
        {
            if (v == null) return 0.0;
            return Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        }

        private static Vector Normalise(this Vector v)
        {
            double len = v.Length();
            if (len < 1e-12) return new Vector();
            return new Vector { X = v.X / len, Y = v.Y / len, Z = v.Z / len };
        }

        private class EndRef
        {
            public int CurveIndex;
            public bool IsStart;
            public Point Point;

            public EndRef(int curveIndex, bool isStart, Point pt)
            {
                CurveIndex = curveIndex;
                IsStart = isStart;
                Point = pt;
            }
        }

        private class TargetSeg
        {
            public int TargetCurveIndex;
            public Line Segment;
            public double DistanceToEnd;

            public TargetSeg(int idx, Line seg, double d)
            {
                TargetCurveIndex = idx;
                Segment = seg;
                DistanceToEnd = d;
            }
        }
    }
}