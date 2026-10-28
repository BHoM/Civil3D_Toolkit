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

using BH.oM.Base.Attributes;

using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Converts a BH.oM.Geometry.Polyline to a Civil 3D curve: a lightweight polyline when it is flat at a constant elevation, otherwise a 3D polyline.")]
        [Input("polyline", "A BH.oM.Geometry.Polyline to convert.")]
        [Output("curve", "The converted Civil 3D Curve.")]
        public static ACD.Curve ToCivil3D(this BHG.Polyline polyline)
        {
            if (polyline == null || polyline.ControlPoints == null || polyline.ControlPoints.Count < 2)
            {
                BH.Engine.Base.Compute.RecordError("Cannot push a polyline with fewer than two points.");
                return null;
            }

            return SegmentsToCivil3D(polyline.SubParts().Cast<BHG.ICurve>().ToList());
        }

        /***************************************************/

        [Description("Converts a BH.oM.Geometry.PolyCurve to a Civil 3D curve: a lightweight polyline (arcs as bulges) when it is flat at a constant elevation, otherwise a 3D polyline. Returns null if it is not flat and contains arcs.")]
        [Input("polyCurve", "A BH.oM.Geometry.PolyCurve to convert.")]
        [Output("curve", "The converted Civil 3D Curve.")]
        public static ACD.Curve ToCivil3D(this BHG.PolyCurve polyCurve)
        {
            if (polyCurve == null || polyCurve.Curves == null || polyCurve.Curves.Count == 0)
            {
                BH.Engine.Base.Compute.RecordError("Cannot push an empty polycurve.");
                return null;
            }

            List<BHG.ICurve> segments = new List<BHG.ICurve>();
            if (!TryFlatten(polyCurve, segments))
            {
                BH.Engine.Base.Compute.RecordError("Cannot push a polycurve that contains curves other than lines, arcs and polylines.");
                return null;
            }

            return SegmentsToCivil3D(segments);
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        //A flat curve at a constant height becomes a lightweight polyline at that elevation (arcs as bulges).
        //Otherwise a curve made only of straight segments becomes a 3D polyline. Anything else cannot be pushed.
        private static ACD.Curve SegmentsToCivil3D(List<BHG.ICurve> segments)
        {
            if (segments.Count == 0)
            {
                BH.Engine.Base.Compute.RecordError("Cannot push a curve with no segments.");
                return null;
            }

            double elevation = segments[0].IStartPoint().Z;
            bool isFlat = segments.All(x => IsAtElevation(x, elevation));

            if (isFlat)
            {
                ACD.Polyline polyline = new ACD.Polyline { Elevation = elevation };
                for (int i = 0; i < segments.Count; i++)
                {
                    BHG.Point start = segments[i].IStartPoint();
                    double bulge = segments[i] is BHG.Arc ? ((BHG.Arc)segments[i]).ArcToBulge() : 0.0;
                    polyline.AddVertexAt(i, start.ToCivil3D2d(), bulge, 0, 0);
                }

                polyline.AddVertexAt(segments.Count, segments.Last().IEndPoint().ToCivil3D2d(), 0, 0, 0);
                return polyline;
            }

            if (segments.All(x => x is BHG.Line))
            {
                ACG.Point3dCollection points = new ACG.Point3dCollection();
                foreach (BHG.ICurve segment in segments)
                    points.Add(segment.IStartPoint().ToCivil3D());

                points.Add(segments.Last().IEndPoint().ToCivil3D());
                return new ACD.Polyline3d(ACD.Poly3dType.SimplePoly, points, false);
            }

            BH.Engine.Base.Compute.RecordError("Cannot push a non-planar curve that contains arcs. Convert it to a polyline or NURBS curve first.");
            return null;
        }

        /***************************************************/

        //Collects the lines and arcs of a polycurve, including those inside nested polylines and polycurves.
        private static bool TryFlatten(BHG.PolyCurve polyCurve, List<BHG.ICurve> segments)
        {
            foreach (BHG.ICurve curve in polyCurve.Curves)
            {
                if (curve is BHG.Line || curve is BHG.Arc)
                    segments.Add(curve);
                else if (curve is BHG.Polyline)
                    segments.AddRange(((BHG.Polyline)curve).SubParts());
                else if (curve is BHG.PolyCurve)
                {
                    if (!TryFlatten((BHG.PolyCurve)curve, segments))
                        return false;
                }
                else
                    return false;
            }

            return true;
        }

        /***************************************************/

        //An arc is at the elevation only if its start, middle and end are, which means its plane is horizontal.
        private static bool IsAtElevation(BHG.ICurve segment, double elevation)
        {
            List<BHG.Point> points = new List<BHG.Point> { segment.IStartPoint(), segment.IEndPoint() };
            if (segment is BHG.Arc)
                points.Add(((BHG.Arc)segment).PointAtParameter(0.5));

            return points.All(x => Math.Abs(x.Z - elevation) <= BHG.Tolerance.Distance);
        }

        /***************************************************/
    }
}
