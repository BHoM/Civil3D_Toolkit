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
using BH.Engine.Geometry;

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
        public static ACD.Arc ToCivil3D(this BHG.Arc arc)
        {
            var center = arc.CoordinateSystem.Origin.ToACGPoint3d();
            var normal = arc.CoordinateSystem.Z.ToCivil3D().GetNormal();
            double radius = arc.Radius;

            var startPt = Query.StartPoint(arc).ToACGPoint3d();
            var endPt   = Query.EndPoint(arc).ToACGPoint3d();

            var vStart = startPt - center;
            var vEnd   = endPt   - center;

            double startAngle = AngleFromNormalBasis(normal, vStart);
            double endAngle   = AngleFromNormalBasis(normal, vEnd);

            // Determine original BHoM direction
            bool isCCW = arc.EndAngle > arc.StartAngle;

            if (isCCW)
            {
                if (endAngle < startAngle)
                    endAngle += 2 * Math.PI;
            }
            else
            {
                if (endAngle > startAngle)
                    startAngle += 2 * Math.PI;
            }

            return new ACD.Arc(center, normal, radius, startAngle, endAngle);
        }

        private static double AngleFromNormalBasis(
            ACG.Vector3d normal,
            ACG.Vector3d vector)
        {
            // Ensure unit normal
            normal = normal.GetNormal();

            // AutoCAD-provided safe perpendicular
            ACG.Vector3d xAxis = normal.GetPerpendicularVector().GetNormal();
            ACG.Vector3d yAxis = normal.CrossProduct(xAxis).GetNormal();

            double x = vector.DotProduct(xAxis);
            double y = vector.DotProduct(yAxis);

            return Math.Atan2(y, x);
        }

        /*************** Circle ********************/
        public static ACD.Circle ToCivil3D(this BHG.Circle circle)
        {
            var center = circle.Centre.ToACGPoint3d();
            var normal = new ACG.Vector3d(circle.Normal.X, circle.Normal.Y, circle.Normal.Z);
            
            return new ACD.Circle(center, normal, circle.Radius);
        }

        /*************** Ellipse ********************/
        public static ACD.Ellipse ToCivil3D(this BHG.Ellipse ellipse)
        {
            var center = ellipse.Centre.ToACGPoint3d();
            
            // Scale axis directions by their respective radii to get the major/minor axis vectors
            var majorAxis = new ACG.Vector3d(
                ellipse.Axis1.X * ellipse.Radius1,
                ellipse.Axis1.Y * ellipse.Radius1,
                ellipse.Axis1.Z * ellipse.Radius1
            );
            
            var minorAxis = new ACG.Vector3d(
                ellipse.Axis2.X * ellipse.Radius2,
                ellipse.Axis2.Y * ellipse.Radius2,
                ellipse.Axis2.Z * ellipse.Radius2
            );
            
            double radiusRatio = ellipse.Radius2 / ellipse.Radius1;
            
            return new ACD.Ellipse(center, majorAxis, minorAxis, radiusRatio, 0.0, 2.0 * Math.PI);
        }



        /*************** LWPolyline (2d) & 3DPolyline ********************/
        public static ACD.Polyline3d ToPolyline3D(this BHG.Polyline polyline)
        {
            if (polyline == null || polyline.ControlPoints == null || polyline.ControlPoints.Count < 2)
                return null;

            try
            {
                // Convert control points to ACG.Point3d
                var points = new ACG.Point3dCollection();
                foreach (var pt in polyline.ControlPoints)
                {
                    points.Add(pt.ToACGPoint3d());
                }

                // Create Polyline3d
                var polyline3d = new ACD.Polyline3d(ACD.Poly3dType.SimplePoly, points, false);

                return polyline3d;
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordWarning($"Failed to convert BHoM Polyline to ACD.Polyline3d: {ex.Message}");
                return null;
            }
        }

        public static ACD.Polyline ToLWPolyline(this BHG.Polyline polyline)
        {
            if (polyline == null || polyline.ControlPoints == null || polyline.ControlPoints.Count < 2)
                return null;

            try
            {
                // Create LWPolyline (2D)
                var lwPolyline = new ACD.Polyline();

                // Add vertices (2D projection)
                for (int i = 0; i < polyline.ControlPoints.Count; i++)
                {
                    var pt = polyline.ControlPoints[i];
                    lwPolyline.AddVertexAt(i, new ACG.Point2d(pt.X, pt.Y), 0, 0, 0);
                }

                return lwPolyline;
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordWarning($"Failed to convert BHoM Polyline to ACD.LWPolyline: {ex.Message}");
                return null;
            }
        }

        public static ACD.Polyline3d ToPolyline3D(this BHG.PolyCurve polyCurve)
        {
            if (polyCurve == null || polyCurve.Curves == null || polyCurve.Curves.Count == 0)
                return null;

            try
            {
                var points = new ACG.Point3dCollection();

                // Extract points from all curve segments
                foreach (var curve in polyCurve.Curves)
                {
                    if (curve is BHG.Line line)
                    {
                        points.Add(line.Start.ToACGPoint3d());
                    }
                    else if (curve is BHG.Arc arc)
                    {
                        var startPt = ArcStartPoint(arc);
                        points.Add(startPt.ToACGPoint3d());
                    }
                    else if (curve is BHG.Polyline polyline)
                    {
                        foreach (var pt in polyline.ControlPoints)
                        {
                            points.Add(pt.ToACGPoint3d());
                        }
                    }
                }

                // Add the last point if needed
                if (polyCurve.Curves.Count > 0)
                {
                    var lastCurve = polyCurve.Curves.Last();
                    BHG.Point endPt = null;

                    if (lastCurve is BHG.Line line)
                        endPt = line.End;
                    else if (lastCurve is BHG.Arc arc)
                        endPt = ArcEndPoint(arc);
                    else if (lastCurve is BHG.Polyline polyline)
                        endPt = polyline.ControlPoints.Last();

                    if (endPt != null)
                        points.Add(endPt.ToACGPoint3d());
                }

                return new ACD.Polyline3d(ACD.Poly3dType.SimplePoly, points, false);
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordWarning($"Failed to convert BHoM PolyCurve to ACD.Polyline3d: {ex.Message}");
                return null;
            }
        }

        public static ACD.Polyline ToLWPolyline(this BHG.PolyCurve polyCurve)
        {
            if (polyCurve == null || polyCurve.Curves == null || polyCurve.Curves.Count == 0)
                return null;

            try
            {
                var lwPolyline = new ACD.Polyline();
                int vertexIndex = 0;

                // Process each curve segment
                for (int i = 0; i < polyCurve.Curves.Count; i++)
                {
                    var curve = polyCurve.Curves[i];

                    if (curve is BHG.Line line)
                    {
                        // Add start point for all segments
                        var pt2d = new ACG.Point2d(line.Start.X, line.Start.Y);
                        lwPolyline.AddVertexAt(vertexIndex++, pt2d, 0, 0, 0);

                        // Add end point for last segment
                        if (i == polyCurve.Curves.Count - 1)
                        {
                            var endPt2d = new ACG.Point2d(line.End.X, line.End.Y);
                            lwPolyline.AddVertexAt(vertexIndex++, endPt2d, 0, 0, 0);
                        }
                    }
                    else if (curve is BHG.Arc arc)
                    {
                        // Add start point
                        var startPt = ArcStartPoint(arc);
                        var pt2d = new ACG.Point2d(startPt.X, startPt.Y);
                        double bulge = ArcToBulge(arc);
                        lwPolyline.AddVertexAt(vertexIndex++, pt2d, bulge, 0, 0);

                        // Add end point for last segment
                        if (i == polyCurve.Curves.Count - 1)
                        {
                            var endPt = ArcEndPoint(arc);
                            var endPt2d = new ACG.Point2d(endPt.X, endPt.Y);
                            lwPolyline.AddVertexAt(vertexIndex++, endPt2d, 0, 0, 0);
                        }
                    }
                    else if (curve is BHG.Polyline polyline)
                    {
                        foreach (var pt in polyline.ControlPoints)
                        {
                            var pt2d = new ACG.Point2d(pt.X, pt.Y);
                            lwPolyline.AddVertexAt(vertexIndex++, pt2d, 0, 0, 0);
                        }
                    }
                }

                return lwPolyline;
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordWarning($"Failed to convert BHoM PolyCurve to ACD.LWPolyline: {ex.Message}");
                return null;
            }
        }

        /*************** Spline ********************/
        public static ACD.Spline ToCivil3D(this BHG.NurbsCurve curve)
        {
            if (curve == null || curve.ControlPoints == null || curve.ControlPoints.Count < 2)
                return null;

            try
            {
                // Create empty spline
                var spline = new ACD.Spline();

                // Set control points
                for (int i = 0; i < curve.ControlPoints.Count; i++)
                {
                    spline.SetControlPointAt(i, curve.ControlPoints[i].ToACGPoint3d());
                }

                // Set weights if available
                if (curve.Weights != null && curve.Weights.Count == curve.ControlPoints.Count)
                {
                    for (int i = 0; i < curve.Weights.Count; i++)
                    {
                        spline.SetWeightAt(i, curve.Weights[i]);
                    }
                }

                // Insert knots if available
                // BHoM knots are in compressed format (cp + degree - 1), need to expand to full format (cp + degree + 1)
                if (curve.Knots != null && curve.Knots.Count > 0)
                {
                    List<double> knotsToInsert = curve.Knots.ToList();

                    // Infer degree from knot count: degree = (knots.Count - controlPoints.Count) + 1
                    int degree = (knotsToInsert.Count - curve.ControlPoints.Count) + 1;

                    // If knots are in compressed format (cp + degree - 1), expand to full format
                    int expectedCompressed = curve.ControlPoints.Count + degree - 1;
                    if (knotsToInsert.Count == expectedCompressed && knotsToInsert.Count > 0)
                    {
                        // Add first and last knots to convert compressed -> full format
                        double firstKnot = knotsToInsert[0];
                        double lastKnot = knotsToInsert[knotsToInsert.Count - 1];
                        
                        knotsToInsert.Insert(0, firstKnot);
                        knotsToInsert.Add(lastKnot);
                    }

                    // Insert the knots
                    foreach (var knot in knotsToInsert)
                    {
                        try
                        {
                            spline.InsertKnot(knot);
                        }
                        catch
                        {
                            // Skip knots that cannot be inserted (e.g., duplicates)
                        }
                    }
                }

                return spline;
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordWarning($"Failed to convert BHoM NurbsCurve to ACD.Spline: {ex.Message}");
                return null;
            }
        }


        /////////////////////
        // From Civils 3d //
        ///////////////////        
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


        public static BHG.PolyCurve FromCivil3D(this ACD.Polyline2d polyline2d)
        {
            var segments = new List<BHG.ICurve>();
            var vertices = new List<ACD.Vertex2d>();

            foreach (ACD.ObjectId id in polyline2d)
            {
                var v = id.GetObject(ACD.OpenMode.ForRead) as ACD.Vertex2d;
                if (v != null)
                    vertices.Add(v);
            }

            int count = vertices.Count;

            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;

                if (!polyline2d.Closed && i == count - 1)
                    break;

                var vStart = vertices[i];
                var vEnd   = vertices[next];

                ACG.Point3d start = new ACG.Point3d(vStart.Position.X, vStart.Position.Y, 0);
                ACG.Point3d end   = new ACG.Point3d(vEnd.Position.X,   vEnd.Position.Y,   0);
                double bulge      = vStart.Bulge;

                AddSegment(segments, start, end, bulge);
            }

            return new BHG.PolyCurve { Curves = segments };
        }

        public static BHG.PolyCurve FromCivil3D(this ACD.Polyline3d polyline3d)
        {
            var segments = new List<BHG.ICurve>();
            var vertices = new List<ACD.PolylineVertex3d>();

            // Collect vertices
            foreach (ACD.ObjectId id in polyline3d)
            {
                var v = id.GetObject(ACD.OpenMode.ForRead) as ACD.PolylineVertex3d;
                if (v != null)
                    vertices.Add(v);
            }

            int count = vertices.Count;

            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;

                if (!polyline3d.Closed && i == count - 1)
                    break;

                ACG.Point3d start = vertices[i].Position;
                ACG.Point3d end   = vertices[next].Position;

                // Polyline3d only supports straight segments
                segments.Add(new BHG.Line
                {
                    Start = start.FromCivil3D(),
                    End   = end.FromCivil3D()
                });
            }

            return new BHG.PolyCurve { Curves = segments };
        }


        /*************** Spline ********************/

        public static BHG.NurbsCurve FromCivil3D(this ACD.Spline acSpline)
        {
            if (acSpline == null)
                return null;

            try
            {
                ACD.NurbsData nurbsData = acSpline.NurbsData; 

                ACG.Point3dCollection cp = nurbsData.GetControlPoints();
                DoubleCollection kn     = nurbsData.GetKnots();          
                DoubleCollection we     = nurbsData.GetWeights();        

                if (cp == null || cp.Count < 2 || kn == null || kn.Count == 0)
                    return null;

                // --- Control points
                var controlPoints = new List<BHG.Point>(cp.Count);
                foreach (ACG.Point3d p in cp)
                    controlPoints.Add(p.FromCivil3D());

                // --- Weights
                List<double> weights = (we != null && we.Count > 0)
                    ? we.Cast<double>().ToList()
                    : Enumerable.Repeat(1.0, controlPoints.Count).ToList();

                if (weights.Count != controlPoints.Count)
                    return null;

                // --- Knots (AutoCAD full -> BHoM compressed)
                List<double> knots = kn.Cast<double>().ToList();

                int degree = nurbsData.Degree; // degree is explicit in AutoCAD NurbsData (used only for conversion logic)

                // AutoCAD "full" knot vector commonly: cp + degree + 1
                int expectedAcadFull = controlPoints.Count + degree + 1;

                // BHoM example/primer style "compressed": cp + degree - 1 [6](https://burohappold.sharepoint.com/sites/TechnicalTraining/Shared%20Documents/Buildings-Structures%20Training%20Program/#34 02_List_Management-25022025/PDF - The Grasshopper Primer (EN).pdf?web=1)
                int expectedBhom = controlPoints.Count + degree - 1;

                if (knots.Count == expectedAcadFull && knots.Count >= 2)
                {
                    // Drop one at each end to convert full -> compressed
                    knots.RemoveAt(0);
                    knots.RemoveAt(knots.Count - 1);
                }
                // else: already in compressed form (or unusual case) - leave as-is

                // Optional but useful: normalize knot domain to 0..1 (BHoM examples use 0..1) [5](https://burohappold-my.sharepoint.com/personal/joe_walton_burohappold_com/_layouts/15/Doc.aspx?action=edit&mobileredirect=true&wdorigin=Sharepoint&DefaultItemOpen=1&sourcedoc={49b87c87-ce4b-45be-9e29-2a42b749e12f}&wd=target(/0_NonProject/Automation.one/)&wdpartid={9557023a-96d9-1c09-12e8-4af93a182135}{1}&wdsectionfileid={80ba111e-bb4d-4979-bd51-101470e69015})
                double k0 = knots.First();
                double k1 = knots.Last();
                double span = k1 - k0;
                if (span > 0)
                    knots = knots.Select(k => (k - k0) / span).ToList();

                // Final sanity: BHoM infers degree from counts (per class docs) [5](https://burohappold-my.sharepoint.com/personal/joe_walton_burohappold_com/_layouts/15/Doc.aspx?action=edit&mobileredirect=true&wdorigin=Sharepoint&DefaultItemOpen=1&sourcedoc={49b87c87-ce4b-45be-9e29-2a42b749e12f}&wd=target(/0_NonProject/Automation.one/)&wdpartid={9557023a-96d9-1c09-12e8-4af93a182135}{1}&wdsectionfileid={80ba111e-bb4d-4979-bd51-101470e69015})
                // degree_bhom = (knots.Count - controlPoints.Count) + 1

                return new BHG.NurbsCurve
                {
                    ControlPoints = controlPoints,
                    Knots         = knots,
                    Weights       = weights
                };
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordWarning($"Failed to map spline NurbsData to BHoM NurbsCurve: {ex.Message}");
                return null;
            }
        }



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


