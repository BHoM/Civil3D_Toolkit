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

        [Description("Converts a Civil 3D Spline to BH.oM.Geometry.NurbsCurve.")]
        [Input("acSpline", "A Civil 3D Spline to convert.")]
        [Output("nurbsCurve", "The converted BH.oM.Geometry.NurbsCurve.")]
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

                // BHoM "compressed" knot vector: cp + degree - 1
                int expectedBhom = controlPoints.Count + degree - 1;

                if (knots.Count == expectedAcadFull && knots.Count >= 2)
                {
                    // Drop one at each end to convert full -> compressed
                    knots.RemoveAt(0);
                    knots.RemoveAt(knots.Count - 1);
                }
                // else: already in compressed form (or unusual case) - leave as-is

                // Normalise the knot domain to 0..1, as BHoM uses
                double k0 = knots.First();
                double k1 = knots.Last();
                double span = k1 - k0;
                if (span > 0)
                    knots = knots.Select(k => (k - k0) / span).ToList();

                // BHoM infers the degree from the counts:
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

        /***************************************************/
    }
}
