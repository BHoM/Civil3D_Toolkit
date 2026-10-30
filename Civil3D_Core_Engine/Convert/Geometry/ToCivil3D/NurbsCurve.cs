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

using BH.oM.Base.Attributes;
using BHG = BH.oM.Geometry;

using ACD = Autodesk.AutoCAD.DatabaseServices;
using ACG = Autodesk.AutoCAD.Geometry;

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

        [Description("Converts BH.oM.Geometry.NurbsCurve to a Civil 3D Spline.")]
        [Input("curve", "BH.oM.Geometry.NurbsCurve to convert.")]
        [Output("spline", "The converted Civil 3D Spline.")]
        public static ACD.Spline ToCivil3D(this BHG.NurbsCurve curve)
        {
            if (curve == null || curve.ControlPoints == null || curve.ControlPoints.Count < 2 || curve.Knots == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot push a NurbsCurve with fewer than two control points or no knots.");
                return null;
            }

            int count = curve.ControlPoints.Count;

            //BHoM stores the compressed knot vector (control points + degree - 1); AutoCAD needs the full one (+ 2)
            int degree = curve.Knots.Count - count + 1;
            if (degree < 1)
            {
                BH.Engine.Base.Compute.RecordError("Cannot push a NurbsCurve whose knot count does not match its control points.");
                return null;
            }

            List<double> weights = curve.Weights != null && curve.Weights.Count > 0 ? curve.Weights : Enumerable.Repeat(1.0, count).ToList();
            if (weights.Count != count || weights.Any(x => x <= 0))
            {
                BH.Engine.Base.Compute.RecordError("Cannot push a NurbsCurve whose weights do not match its control points or are not positive.");
                return null;
            }

            ACG.Point3dCollection controlPoints = new ACG.Point3dCollection();
            foreach (BHG.Point point in curve.ControlPoints)
                controlPoints.Add(point.ToCivil3D());

            ACG.DoubleCollection knots = new ACG.DoubleCollection();
            knots.Add(curve.Knots.First());
            foreach (double knot in curve.Knots)
                knots.Add(knot);
            knots.Add(curve.Knots.Last());

            bool rational = weights.Any(x => Math.Abs(x - 1.0) > 1e-12);
            ACG.DoubleCollection acadWeights = new ACG.DoubleCollection(rational ? weights.ToArray() : new double[0]);

            return new ACD.Spline(degree, rational, false, false, controlPoints, knots, acadWeights, 0.0, 0.0);
        }
        /***************************************************/
    }
}
