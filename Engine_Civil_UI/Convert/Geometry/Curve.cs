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
using BHG = BH.oM.Geometry;
using ACD = Autodesk.AutoCAD.DatabaseServices;
using ACG = Autodesk.AutoCAD.Geometry;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        ///////////////////
        // To Civils 3d //
        /////////////////
        /*************** Line  ********************/
        public static ACD.Line ToCivil3D(this BHG.Line line)
        {
            return new ACD.Line(line.Start.ToACGPoint3d(), line.End.ToACGPoint3d());
        }

        /*************** Arc ********************/


        /*************** Circle ********************/


        /*************** Ellipse ********************/


        /*************** Ellipsical Arc ********************/


        /*************** LWPolyline (2d) & 3DPolyline ********************/


        /*************** Spline ********************/


        /////////////////////
        // From Civils 3d //
        ///////////////////        
        /// 
        /// 
        /// 
        //

        /*************** Line ********************/
        public static BHG.Line FromCivil3D(this ACD.Line line)
        {
            return new BHG.Line
            {
                Start = line.StartPoint.FromCivil3D(),
                End = line.EndPoint.FromCivil3D()
            };
        }

        /*************** Arc ********************/

        public static BHG.Arc FromCivil3D(this ACD.Arc acArc)
        {
            var c = new BHG.Point { X = acArc.Center.X, Y = acArc.Center.Y, Z = acArc.Center.Z };

            var n = new BHG.Vector { X = acArc.Normal.X, Y = acArc.Normal.Y, Z = acArc.Normal.Z };
            n = BH.Engine.Geometry.Modify.Normalise(n);

            var x = BH.Engine.Geometry.Modify.Rotate(
                BH.Engine.Geometry.Create.Vector(1, 0, 0),
                acArc.StartAngle,
                n
            );
            x = BH.Engine.Geometry.Modify.Normalise(x);

            var y = BH.Engine.Geometry.Modify.Normalise(
                BH.Engine.Geometry.Query.CrossProduct(n, x)
            );

            var cs = BH.Engine.Geometry.Create.CartesianCoordinateSystem(c, x, y);

            double sweep = acArc.EndAngle - acArc.StartAngle;
            if (sweep < 0) sweep += 2 * Math.PI;

            return BH.Engine.Geometry.Create.Arc(cs, acArc.Radius, 0.0, sweep);
        }
        
        /*************** Circle ********************/
        public static BHG.Circle FromCivil3D(this ACD.Circle acCircle)
        {
            return new BHG.Circle
            {
                Centre = acCircle.Center.FromCivil3D(),
                Normal = acCircle.Normal.FromCivil3D(),
                Radius = acCircle.Radius
            };
        }

        /*************** Ellipse ********************/

        public static BHG.Ellipse FromCivil3D(this ACD.Ellipse acEllipse)
        {
            // Normalise the major and minor axes
            var axis1 = acEllipse.MajorAxis.GetNormal();
            var axis2 = acEllipse.MinorAxis.GetNormal();

            return new BHG.Ellipse
            {
                Centre = acEllipse.Center.FromCivil3D(),
                Axis1 = axis1.FromCivil3D(),
                Axis2 = axis2.FromCivil3D(),
                Radius1 = acEllipse.MajorRadius,
                Radius2 = acEllipse.MinorRadius
            };
        }

        /*************** Polyline ********************/

        public static BHG.PolyCurve FromCivil3D(this ACD.Polyline acadPolyline)
        {
            List<BHG.ICurve> segments = new List<BHG.ICurve>();
            int numVerts = acadPolyline.NumberOfVertices;
            for (int i = 0; i < numVerts; i++)
            {
                int next = (i + 1) % numVerts;
                // Handle open polyline
                if (!acadPolyline.Closed && i == numVerts - 1)
                    break;
                ACG.Point3d start = acadPolyline.GetPoint3dAt(i);
                ACG.Point3d end = acadPolyline.GetPoint3dAt(next);
                double bulge = acadPolyline.GetBulgeAt(i);
                if (bulge == 0)
                {
                    // Straight segment → Line
                    segments.Add(new BHG.Line
                    {
                        Start = start.FromCivil3D(),
                        End = end.FromCivil3D()
                    });
                }
                else
                {
                    // Arc segment
                    BHG.Arc arc = BulgeToArc(start, end, bulge);
                    segments.Add(arc);
                }
            }
            return new BHG.PolyCurve { Curves = segments };
        }

        private static BHG.Arc BulgeToArc(ACG.Point3d start, ACG.Point3d end, double bulge)
        {
            double chord = start.DistanceTo(end);
            if (System.Math.Abs(bulge) < 1e-10 || chord < 1e-10)
                return null;

            // Included angle (signed). bulge = tan(theta/4)  [2](https://lee-mac.com/bulgeconversion.html)
            double theta = 4.0 * System.Math.Atan(bulge);
            double half = System.Math.Abs(theta) / 2.0;

            double sinHalf = System.Math.Sin(half);
            if (System.Math.Abs(sinHalf) < 1e-10)
                return null;

            // Positive radius
            double radius = chord / (2.0 * sinHalf);

            // Midpoint of chord
            ACG.Point3d mid = new ACG.Point3d(
                (start.X + end.X) / 2.0,
                (start.Y + end.Y) / 2.0,
                (start.Z + end.Z) / 2.0
            );

            // Perpendicular unit (left of chord, in XY)
            ACG.Vector3d chordVec = end - start;
            ACG.Vector3d perpDir = new ACG.Vector3d(-chordVec.Y, chordVec.X, 0.0);
            if (perpDir.Length < 1e-10)
                perpDir = new ACG.Vector3d(1, 0, 0);
            perpDir = perpDir.GetNormal();

            // **Apothem** (distance from chord midpoint to center), not sagitta
            double halfChord = chord / 2.0;
            double apothem = System.Math.Sqrt(System.Math.Max(0.0, radius * radius - halfChord * halfChord));

            // Center: choose side based on bulge sign (CW vs CCW) [2](https://lee-mac.com/bulgeconversion.html)
            ACG.Point3d center = mid + perpDir * (System.Math.Sign(bulge) * apothem);

            // Local axes
            ACG.Vector3d centerToStart = (start - center).GetNormal();
            BHG.Vector xAxis = centerToStart.FromCivil3D();

            // Flip handedness for negative bulge so positive sweep goes the "right" way
            BHG.Vector zAxis = new BHG.Vector { X = 0, Y = 0, Z = (bulge >= 0.0) ? 1.0 : -1.0 };
            BHG.Vector yAxis = BH.Engine.Geometry.Query.CrossProduct(zAxis, xAxis);
            yAxis = BH.Engine.Geometry.Modify.Normalise(yAxis);

            var system = BH.Engine.Geometry.Create.CartesianCoordinateSystem(
                center.FromCivil3D(),
                xAxis,
                yAxis
            );

            // Use magnitude only; DO NOT reflex-convert (the flipped system handles CW)
            double sweep = System.Math.Abs(theta);

            return BH.Engine.Geometry.Create.Arc(system, radius, 0.0, sweep);
        }



        /*************** Spline ********************/

        // public static BHG.NurbsCurve FromCivil3D(this ACD.Spline acSpline)
        // {
        //     if (acSpline == null)
        //         return null;

        //     try
        //     {

        //     ACG.Curve3d ac = acSpline.GetCurveData();

        //     // NURBS definition data bundle (control points, knots, weights, etc.)
        //     ACG.NurbCurve3dData def = ac.DefinitionData;

        //     // -----------------------------
        //     // Control points
        //     // -----------------------------
        //     var controlPoints = new List<BHG.Point>();

        //     foreach (ACG.Point3d p in def.ControlPoints)
        //     {
        //         controlPoints.Add(p.FromCivil3D());
        //     }

        //     if (controlPoints.Count < 2)
        //         return null;

        //     // -----------------------------
        //     // Knots
        //     // -----------------------------
        //     var knots = new List<double>();

        //     foreach (double k in def.Knots)
        //     {
        //         knots.Add(k);
        //     }

        //     if (knots.Count == 0)
        //         return null;

        //     // -----------------------------
        //     // Weights
        //     // -----------------------------
        //     var weights = new List<double>();

        //     foreach (double w in def.Weights)
        //     {
        //         weights.Add(w);
        //     }

        //     // If spline is non‑rational, Civil 3D returns zero weights.
        //     // BHoM expects one weight per control point.
        //     if (weights.Count == 0)
        //     {
        //         for (int i = 0; i < controlPoints.Count; i++)
        //             weights.Add(1.0);
        //     }

        //     // Final sanity check
        //     if (weights.Count != controlPoints.Count)
        //         return null;

        //     // -----------------------------
        //     // Construct BHoM NurbsCurve
        //     // -----------------------------
        //     return new BHG.NurbsCurve
        //     {
        //         ControlPoints = controlPoints,
        //         Knots         = knots,
        //         Weights       = weights
        //     };
        //     }
        //     catch
        //     {
        //         BH.Engine.Base.Compute.RecordWarning("Failed to extract NURBS data from Spline entity.");
        //         return null;
        //     }
        // }



        /////////////////////
        // From Civils 3d //
        ///////////////////        
        /*************** Line & LineSegment 3d & 2d ********************/


        public static BHG.Line FromCivil3D(this ACG.Line3d line)
        {
            return new BHG.Line
            {
                Start = line.StartPoint.FromCivil3D(),
                End   = line.EndPoint.FromCivil3D()
            };
        }

        public static BHG.Line FromCivil3D(this ACG.Line2d line)
        {
            return new BHG.Line
            {
                Start = line.StartPoint.FromCivil3D(),
                End   = line.EndPoint.FromCivil3D()
            };
        }

        public static BHG.Line FromCivil3D(this ACG.LineSegment3d line)
        {
            return new BHG.Line
            {
                Start = line.StartPoint.FromCivil3D(),
                End   = line.EndPoint.FromCivil3D()
            };
        }

        public static BHG.Line FromCivil3D(this ACG.LineSegment2d line)
        {
            return new BHG.Line
            {
                Start = line.StartPoint.FromCivil3D(),
                End   = line.EndPoint.FromCivil3D()
            };
        }


        /*************** Arc 3d & 2d ********************/

        public static BHG.ICurve FromCivil3D(this ACG.CircularArc3d acCircArc)
        {
            if (acCircArc.IsClosed())
            {
                return new BHG.Circle 
                { 
                    Centre = acCircArc.Center.FromCivil3D(), 
                    Normal = acCircArc.Normal.FromCivil3D(), 
                    Radius = acCircArc.Radius 
                };
            }
            else
            {
                BHG.CoordinateSystem.Cartesian system = BH.Engine.Geometry.Create.CartesianCoordinateSystem
                (
                    acCircArc.Center.FromCivil3D(), 
                    acCircArc.ReferenceVector.FromCivil3D(), 
                    acCircArc.Normal.CrossProduct(acCircArc.ReferenceVector).FromCivil3D()
                );

                return BH.Engine.Geometry.Create.Arc(system, acCircArc.Radius, acCircArc.StartAngle, acCircArc.EndAngle);
            }
        }


        public static BHG.ICurve FromCivil3D(this ACG.CircularArc2d acCircArc)
        {
            // Lift centre into 3D
            var centre = new BHG.Point
            {
                X = acCircArc.Center.X,
                Y = acCircArc.Center.Y,
                Z = 0
            };

            var normal = new BHG.Vector { X = 0, Y = 0, Z = 1 };

            if (acCircArc.IsClosed())
            {
                return new BHG.Circle
                {
                    Centre = centre,
                    Normal = normal,
                    Radius = acCircArc.Radius
                };
            }

            // X axis from start angle (2D analogue of ReferenceVector)
            var xAxis = new BHG.Vector
            {
                X = Math.Cos(acCircArc.StartAngle),
                Y = Math.Sin(acCircArc.StartAngle),
                Z = 0
            };

            xAxis = BH.Engine.Geometry.Modify.Normalise(xAxis);

            var yAxis = BH.Engine.Geometry.Query.CrossProduct(normal, xAxis);

            var system = BH.Engine.Geometry.Create.CartesianCoordinateSystem(
                centre,
                xAxis,
                yAxis
            );

            return BH.Engine.Geometry.Create.Arc(
                system,
                acCircArc.Radius,
                acCircArc.StartAngle,
                acCircArc.EndAngle
            );
        }



        /*************** Nurbs 3d & 2d  ********************/

        public static BHG.NurbsCurve FromCivil3D(this ACG.NurbCurve3d ac)
        {
            if (ac == null)
                return null;

            // NURBS definition data bundle (control points, knots, weights, etc.)
            ACG.NurbCurve3dData def = ac.DefinitionData;

            // -----------------------------
            // Control points
            // -----------------------------
            var controlPoints = new List<BHG.Point>();

            foreach (ACG.Point3d p in def.ControlPoints)
            {
                controlPoints.Add(p.FromCivil3D());
            }

            if (controlPoints.Count < 2)
                return null;

            // -----------------------------
            // Knots (use as-is from Civil 3D)
            // -----------------------------
            var knots = new List<double>();

            foreach (double k in def.Knots)
            {
                knots.Add(k);
            }

            if (knots.Count == 0)
                return null;

            // Validate knot vector structure
            if (knots.Count != controlPoints.Count + ac.Degree + 1)
            {
                BH.Engine.Base.Compute.RecordWarning(
                    $"Knot vector size ({knots.Count}) does not match expected size for degree {ac.Degree} " +
                    $"with {controlPoints.Count} control points. Expected: {controlPoints.Count + ac.Degree + 1}. " +
                    $"The curve may not evaluate correctly.");
            }

            // Verify knot vector is non-decreasing
            for (int i = 1; i < knots.Count; i++)
            {
                if (knots[i] < knots[i - 1])
                {
                    BH.Engine.Base.Compute.RecordError("Knot vector is not non-decreasing.");
                    return null;
                }
            }

            // -----------------------------
            // Weights
            // -----------------------------
            var weights = new List<double>();

            foreach (double w in def.Weights)
            {
                weights.Add(w);
            }

            // If spline is non‑rational, Civil 3D returns zero weights.
            // BHoM expects one weight per control point.
            if (weights.Count == 0)
            {
                for (int i = 0; i < controlPoints.Count; i++)
                    weights.Add(1.0);
            }

            // Final sanity check
            if (weights.Count != controlPoints.Count)
            {
                BH.Engine.Base.Compute.RecordError("Weight count does not match control point count.");
                return null;
            }

            // -----------------------------
            // Construct BHoM NurbsCurve
            // -----------------------------
            return new BHG.NurbsCurve
            {
                ControlPoints = controlPoints,
                Knots         = knots,
                Weights       = weights
            };
        }


        public static BHG.NurbsCurve FromCivil3D(this ACG.NurbCurve2d acNurbsCurve)
        {
            var pts = new List<BHG.Point>();

            for (int i = 0; i < acNurbsCurve.NumFitPoints; i++)
            {
                var p2 = acNurbsCurve.GetFitPointAt(i);

                pts.Add(new BHG.Point
                {
                    X = p2.X,
                    Y = p2.Y,
                    Z = 0
                });
            }

            var knots = new List<double>();
            foreach (double d in acNurbsCurve.Knots)
            {
                knots.Add(d);
            }

            var weights = new List<double>();
            for (int i = 0; i < acNurbsCurve.NumFitPoints; i++)
            {
                weights.Add(acNurbsCurve.GetWeightAt(i));
            }

            return new BHG.NurbsCurve
            {
                ControlPoints = pts,
                Knots          = knots,
                Weights        = weights
            };
        }

        /*********************Ellipses and Elliptical Arcs 3d & 2d******************************/

        public static BHG.ICurve FromCivil3D(this ACG.EllipticalArc3d acEllipse)
        {
            // Full ellipse -> map to BH.oM.Geometry.Ellipse (per your class definition)
            if (acEllipse.IsClosed())
            {
                // Axis vectors should be direction-only in BHoM, so normalise them on the AutoCAD side
                // (Vector3d.GetNormal() returns the unit vector)
                var axis1 = acEllipse.MajorAxis.GetNormal();
                var axis2 = acEllipse.MinorAxis.GetNormal();

                return new BHG.Ellipse
                {
                    Centre  = acEllipse.Center.FromCivil3D(),
                    Axis1   = axis1.FromCivil3D(),
                    Axis2   = axis2.FromCivil3D(),
                    Radius1 = acEllipse.MajorRadius,
                    Radius2 = acEllipse.MinorRadius
                };
            }

            // Elliptical arc segment -> cannot be a BH.oM.Geometry.Arc (circular-only overloads)
            // Convert to a geometrically identical NURBS curve using AutoCAD’s constructor,
            // then reuse your existing NurbCurve3d -> BHoM conversion.
            try
            {
            ACG.NurbCurve3d nurbs = new ACG.NurbCurve3d(acEllipse);
            return nurbs.FromCivil3D();
            }
            catch
            {
                BH.Engine.Base.Compute.RecordWarning("Failed to convert EllipticalArc3d to NurbCurve3d. Returning null.");
                return null;
            }
        }


        public static BHG.ICurve FromCivil3D(this ACG.EllipticalArc2d acEllipse)
        {
            // Closed ellipse -> BHoM Ellipse
            if (acEllipse.IsClosed())
            {
                var centre = new BHG.Point
                {
                    X = acEllipse.Center.X,
                    Y = acEllipse.Center.Y,
                    Z = 0
                };

                var axis1 = new BHG.Vector
                {
                    X = Math.Cos(acEllipse.StartAngle),
                    Y = Math.Sin(acEllipse.StartAngle),
                    Z = 0
                };
                axis1 = BH.Engine.Geometry.Modify.Normalise(axis1);

                var axis2 = new BHG.Vector
                {
                    X = -axis1.Y,
                    Y =  axis1.X,
                    Z = 0
                };

                return new BHG.Ellipse
                {
                    Centre  = centre,
                    Axis1   = axis1,
                    Axis2   = axis2,
                    Radius1 = acEllipse.MajorRadius,
                    Radius2 = acEllipse.MinorRadius
                };
            }

            // Open ellipse -> NURBS fallback
            try
            {
                // Build 2D NURBS
                ACG.NurbCurve2d nurbs2d = new ACG.NurbCurve2d(acEllipse);
                return nurbs2d.FromCivil3D();
            }
            catch
            {
                BH.Engine.Base.Compute.RecordWarning("Failed to convert EllipticalArc2d to NurbCurve2d. Returning null.");
                return null;
            }
        }

        /*********************Polylines 3d & 2d******************************/


        public static BHG.Polyline FromCivil3D(this ACG.PolylineCurve3d acPolyLine)
        {

            List<BHG.Point> pts = new List<BHG.Point>();

            for (int i = 0; i < acPolyLine.NumberOfControlPoints; i++)
            {
                pts.Add(acPolyLine.ControlPointAt(i).FromCivil3D());
            }

            return new BHG.Polyline { ControlPoints = pts };
        }


        public static BHG.Polyline FromCivil3D(this ACG.PolylineCurve2d acPolyLine)
        {
            List<BHG.Point> pts = new List<BHG.Point>();
            
            for (int i = 0; i < acPolyLine.NumControlPoints; i++)
            {
                var p = acPolyLine.FitPointAt(i);

                pts.Add(new BHG.Point
                {
                    X = p.X,
                    Y = p.Y,
                    Z = 0
                });
            }

            return new BHG.Polyline { ControlPoints = pts };
        }

        /*********************CompositeCurve 3d & 2d******************************/


        public static BHG.ICurve FromCivil3D(this ACG.CompositeCurve3d acCurve)
        {
            const double tol = 1e-6;

            var bhCurves = new List<BHG.ICurve>();
            BHG.ICurve last = null;

            foreach (ACG.Curve3d c in acCurve.GetCurves())
            {
                if (c == null)
                    continue;

                BHG.ICurve converted = null;

                switch (c)
                {
                    case ACG.CircularArc3d arc:
                        converted = arc.FromCivil3D();
                        break;

                    case ACG.PolylineCurve3d plc:
                        converted = plc.FromCivil3D();
                        break;

                    case ACG.LineSegment3d ls:
                        converted = ls.FromCivil3D();
                        break;

                    case ACG.Line3d li:
                        converted = li.FromCivil3D();
                        break;

                    case ACG.NurbCurve3d nc:
                        converted = nc.FromCivil3D();
                        break;

                    case ACG.CompositeCurve3d cc:
                        // IMPORTANT:
                        // GetCurves() is already flattened – do NOT recurse
                        continue;

                    default:
                        BH.Engine.Base.Compute.RecordWarning(
                            $"Skipped unsupported Curve3d subtype: {c.GetType().Name}");
                        continue;
                }

                if (converted == null)
                    continue;

                // ---- Guard 1: skip zero‑length curves ----
                if (IsZeroLength(converted, tol))
                    continue;

                // ---- Guard 2: skip consecutive geometric duplicates ----
                if (IsSameAsPrevious(last, converted, tol))
                    continue;

                bhCurves.Add(converted);
                last = converted;
            }

            return new BHG.PolyCurve { Curves = bhCurves };
        }


        private static bool IsZeroLength(BHG.ICurve c, double tol)
        {
            if (!TryGetStartEnd(c, out var s, out var e))
                return true;

            return DistSq(s, e) <= tol * tol;
        }

        private static bool IsSameAsPrevious(BHG.ICurve a, BHG.ICurve b, double tol)
        {
            if (a == null || b == null)
                return false;

            if (a.GetType() != b.GetType())
                return false;

            if (!TryGetStartEnd(a, out var a0, out var a1))
                return false;

            if (!TryGetStartEnd(b, out var b0, out var b1))
                return false;

            return
                DistSq(a0, b0) <= tol * tol &&
                DistSq(a1, b1) <= tol * tol;
        }

        private static double DistSq(BHG.Point a, BHG.Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            double dz = a.Z - b.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        private static BHG.Point ArcPointAtAngle(BHG.Arc arc, double angle)
        {
            var cs = arc.CoordinateSystem;

            // Local x/y directions of the arc plane
            var x = cs.X;
            var y = cs.Y;

            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);

            return new BHG.Point
            {
                X = cs.Origin.X + arc.Radius * (cos * x.X + sin * y.X),
                Y = cs.Origin.Y + arc.Radius * (cos * x.Y + sin * y.Y),
                Z = cs.Origin.Z + arc.Radius * (cos * x.Z + sin * y.Z)
            };
        }
        private static bool TryGetStartEnd(
            BHG.ICurve c,
            out BHG.Point start,
            out BHG.Point end)
        {
            start = null;
            end = null;

            switch (c)
            {
                case BHG.Line l:
                    start = l.Start;
                    end = l.End;
                    return true;

                case BHG.Arc a:
                    start = ArcPointAtAngle(a, a.StartAngle);
                    end   = ArcPointAtAngle(a, a.EndAngle);
                    return true;

                case BHG.Circle ci:
                    // Closed curve – ignore for dedupe
                    return false;

                case BHG.NurbsCurve n when n.ControlPoints?.Count >= 2:
                    start = n.ControlPoints[0];
                    end = n.ControlPoints[n.ControlPoints.Count - 1];
                    return true;

                case BHG.Polyline pl when pl.ControlPoints?.Count >= 2:
                    start = pl.ControlPoints[0];
                    end = pl.ControlPoints[pl.ControlPoints.Count - 1];
                    return true;

                case BHG.PolyCurve pc when pc.Curves?.Count >= 1:
                    return TryGetStartEnd(pc.Curves.First(), out start, out _) &&
                        TryGetStartEnd(pc.Curves.Last(), out _, out end);

                default:
                    return false;
            }
        }

        /***********************ACG Curve Types Not Implemented****************************/
        // Ray3d, Ray2d
        // OffsetCurve3d, OffsetCurve2d
        //AugmentedPolylineCurve3d, AugmentedPolylineCurve2d
        // CubicSplineCurve3d, CubicSplineCurve2d
        // ExternalCurve3d, ExternalCurve2d

        /***************************************************/
        /**** Public Methods - Interface                ****/
        /***************************************************/

        // public static BHG.ICurve FromCivil3D(this ACG.Curve3d acCurve)
        // {
        //     return ToBHoM(acCurve as dynamic);
        // }

        /***************************************************/
    }
}


