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

        [Description("Converts a Civil 3D Spline to BH.oM.Geometry.NurbsCurve.")]
        [Input("acSpline", "A Civil 3D Spline to convert.")]
        [Output("nurbsCurve", "The converted BH.oM.Geometry.NurbsCurve.")]
        public static BHG.NurbsCurve FromCivil3D(this ACD.Spline acSpline)
        {
            if (acSpline == null)
                return null;

            ACD.NurbsData nurbsData = acSpline.NurbsData;
            List<BHG.Point> controlPoints = nurbsData.GetControlPoints().Cast<ACG.Point3d>().Select(x => x.FromCivil3D()).ToList();
            List<double> knots = nurbsData.GetKnots().Cast<double>().ToList();
            List<double> weights = nurbsData.GetWeights().Cast<double>().ToList();

            //A non-rational spline has no weights in AutoCAD; BHoM expects one per control point
            if (weights.Count == 0)
                weights = Enumerable.Repeat(1.0, controlPoints.Count).ToList();

            //AutoCAD stores the full knot vector (control points + degree + 1). BHoM stores it without the first and last knot,
            //with the same values, so the curve and its parameters are unchanged.
            if (controlPoints.Count < 2 || knots.Count != controlPoints.Count + nurbsData.Degree + 1 || weights.Count != controlPoints.Count)
            {
                BH.Engine.Base.Compute.RecordWarning($"A spline with {controlPoints.Count} control points, {knots.Count} knots and {weights.Count} weights for degree {nurbsData.Degree} could not be converted.");
                return null;
            }

            return new BHG.NurbsCurve
            {
                ControlPoints = controlPoints,
                Weights = weights,
                Knots = knots.GetRange(1, knots.Count - 2),
            };
        }
        /***************************************************/
    }
}
