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

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static ACD.Curve ToCivil3D(this BHG.Polyline polyline)
        {
            if (polyline == null)
                return null;

            //Same selection Push used before; Step 6.3 replaces it with a planarity check
            bool hasZ = polyline.ControlPoints != null && polyline.ControlPoints.Any(pt => Math.Abs(pt.Z) > 1e-3);
            return hasZ ? (ACD.Curve)polyline.ToCivil3D3d() : polyline.ToCivil3DLightweight();
        }

        /***************************************************/

        public static ACD.Curve ToCivil3D(this BHG.PolyCurve polyCurve)
        {
            if (polyCurve == null)
                return null;

            return HasNonZeroZ(polyCurve) ? (ACD.Curve)polyCurve.ToCivil3D3d() : polyCurve.ToCivil3DLightweight();
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/
        private static ACD.Polyline3d ToCivil3D3d(this BHG.Polyline polyline)
        {
            if (polyline == null || polyline.ControlPoints == null || polyline.ControlPoints.Count < 2)
                return null;

            try
            {
                // Convert control points to ACG.Point3d
                var points = new ACG.Point3dCollection();
                foreach (var pt in polyline.ControlPoints)
                {
                    points.Add(pt.ToCivil3D());
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

        /***************************************************/

        private static ACD.Polyline ToCivil3DLightweight(this BHG.Polyline polyline)
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

        /***************************************************/

        private static ACD.Polyline3d ToCivil3D3d(this BHG.PolyCurve polyCurve)
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
                        points.Add(line.Start.ToCivil3D());
                    }
                    else if (curve is BHG.Arc arc)
                    {
                        var startPt = ArcStartPoint(arc);
                        points.Add(startPt.ToCivil3D());
                    }
                    else if (curve is BHG.Polyline polyline)
                    {
                        foreach (var pt in polyline.ControlPoints)
                        {
                            points.Add(pt.ToCivil3D());
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
                        points.Add(endPt.ToCivil3D());
                }

                return new ACD.Polyline3d(ACD.Poly3dType.SimplePoly, points, false);
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordWarning($"Failed to convert BHoM PolyCurve to ACD.Polyline3d: {ex.Message}");
                return null;
            }
        }

        /***************************************************/

        private static ACD.Polyline ToCivil3DLightweight(this BHG.PolyCurve polyCurve)
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


        /***************************************************/

        // Helper: check if PolyCurve has any non-zero Z values
        private static bool HasNonZeroZ(BH.oM.Geometry.PolyCurve polyCurve)
        {
            if (polyCurve == null || polyCurve.Curves == null)
                return false;

            foreach (var curve in polyCurve.Curves)
            {
                if (curve is BH.oM.Geometry.Line line)
                {
                    if (Math.Abs(line.Start.Z) > 1e-10 || Math.Abs(line.End.Z) > 1e-3)
                        return true;
                }
                else if (curve is BH.oM.Geometry.Arc arc)
                {
                    // Check arc coordinate system origin for Z
                    if (Math.Abs(arc.CoordinateSystem.Origin.Z) > 1e-3)
                        return true;
                }
                else if (curve is BH.oM.Geometry.Polyline polyline)
                {
                    if (polyline.ControlPoints != null && polyline.ControlPoints.Any(pt => Math.Abs(pt.Z) > 1e-3))
                        return true;
                }
            }

            return false;
        }
        /***************************************************/
    }
}
