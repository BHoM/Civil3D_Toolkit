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
using Autodesk.AutoCAD.Geometry;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHG.Point ArcStartPoint(BHG.Arc arc)
        {
            var cs = arc.CoordinateSystem;
            var x = cs.X;
            var y = cs.Y;

            double cos = Math.Cos(arc.StartAngle);
            double sin = Math.Sin(arc.StartAngle);

            return new BHG.Point
            {
                X = cs.Origin.X + arc.Radius * (cos * x.X + sin * y.X),
                Y = cs.Origin.Y + arc.Radius * (cos * x.Y + sin * y.Y),
                Z = cs.Origin.Z + arc.Radius * (cos * x.Z + sin * y.Z)
            };
        }

        public static BHG.Point ArcEndPoint(BHG.Arc arc)
        {
            var cs = arc.CoordinateSystem;
            var x = cs.X;
            var y = cs.Y;

            double cos = Math.Cos(arc.EndAngle);
            double sin = Math.Sin(arc.EndAngle);

            return new BHG.Point
            {
                X = cs.Origin.X + arc.Radius * (cos * x.X + sin * y.X),
                Y = cs.Origin.Y + arc.Radius * (cos * x.Y + sin * y.Y),
                Z = cs.Origin.Z + arc.Radius * (cos * x.Z + sin * y.Z)
            };
        }

        public static double ArcToBulge(BHG.Arc arc)
        {
            double sweep = arc.EndAngle - arc.StartAngle;
            if (sweep < 0) sweep += 2 * Math.PI;

            // bulge = tan(theta/4) where theta is the included angle
            return Math.Tan(sweep / 4.0);
        }

        public static void AddSegment(
            List<BHG.ICurve> segments,
            ACG.Point3d start,
            ACG.Point3d end,
            double bulge)
        {
            if (Math.Abs(bulge) < 1e-12)
            {
                segments.Add(new BHG.Line
                {
                    Start = start.FromCivil3D(),
                    End   = end.FromCivil3D()
                });
            }
            else
            {
                segments.Add(BulgeToArc(start, end, bulge));
            }
        }
        public static BHG.Arc BulgeToArc(ACG.Point3d start, ACG.Point3d end, double bulge)
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


        public static bool IsZeroLength(BHG.ICurve c, double tol)
        {
            if (!TryGetStartEnd(c, out var s, out var e))
                return true;

            return DistSq(s, e) <= tol * tol;
        }

        public static bool IsSameAsPrevious(BHG.ICurve a, BHG.ICurve b, double tol)
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

        public static double DistSq(BHG.Point a, BHG.Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            double dz = a.Z - b.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        public static BHG.Point ArcPointAtAngle(BHG.Arc arc, double angle)
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
        public static bool TryGetStartEnd(
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

    }
}