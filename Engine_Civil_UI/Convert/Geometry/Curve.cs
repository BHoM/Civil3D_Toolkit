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
            return new ACD.Line(line.Start.ToCivil3D(), line.End.ToCivil3D());
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
            // Knots
            // -----------------------------
            var knots = new List<double>();

            foreach (double k in def.Knots)
            {
                knots.Add(k);
            }

            if (knots.Count == 0)
                return null;

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
                return null;

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
            var bhCurves = new List<BHG.ICurve>();

            foreach (ACG.Curve3d c in acCurve.GetCurves())
            {
                if (c == null)
                    continue;

                // IMPORTANT: avoid calling c.FromCivil3D() where c is Curve3d (dynamic path)

                if (c is ACG.CompositeCurve3d cc)
                {
                    bhCurves.Add(cc.FromCivil3D());                 // recursion uses this same method
                }
                else if (c is ACG.CircularArc3d arc)
                {
                    bhCurves.Add(arc.FromCivil3D()); 
                }
                else if (c is ACG.PolylineCurve3d plc)
                {
                    bhCurves.Add(plc.FromCivil3D());                
                }
                else if (c is ACG.LineSegment3d ls)
                {
                    bhCurves.Add(ls.FromCivil3D());                 
                }
                else if (c is ACG.Line3d li)
                {
                    bhCurves.Add(li.FromCivil3D());                 
                }
                else if (c is ACG.NurbCurve3d nc)
                {
                    bhCurves.Add(nc.FromCivil3D());                 
                }
                else
                {
                BH.Engine.Base.Compute.RecordWarning($"Skipped unsupported Curve3d subtype: {c.GetType().Name}");
                }
            }

            return new BHG.PolyCurve { Curves = bhCurves };
        }

        public static BHG.ICurve FromCivil3D(this ACG.CompositeCurve2d acCurve)
        {
            var bhCurves = new List<BHG.ICurve>();

            foreach (ACG.Curve2d c in acCurve.GetCurves())
            {
                if (c == null)
                    continue;

                // IMPORTANT: avoid calling c.FromCivil3D() where c is Curve3d (dynamic path)

                if (c is ACG.CompositeCurve2d cc)
                {
                    bhCurves.Add(cc.FromCivil3D());                 // recursion uses this same method
                }
                else if (c is ACG.CircularArc2d arc)
                {
                    bhCurves.Add(arc.FromCivil3D()); 
                }
                else if (c is ACG.PolylineCurve2d plc)
                {
                    bhCurves.Add(plc.FromCivil3D());                
                }
                else if (c is ACG.LineSegment2d ls)
                {
                    bhCurves.Add(ls.FromCivil3D());                 
                }
                else if (c is ACG.Line2d li)
                {
                    bhCurves.Add(li.FromCivil3D());                 
                }
                else if (c is ACG.NurbCurve2d nc)
                {
                    bhCurves.Add(nc.FromCivil3D());                 
                }
                else
                {
                BH.Engine.Base.Compute.RecordWarning($"Skipped unsupported Curve2d subtype: {c.GetType().Name}");
                }
            }

            return new BHG.PolyCurve { Curves = bhCurves };
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


