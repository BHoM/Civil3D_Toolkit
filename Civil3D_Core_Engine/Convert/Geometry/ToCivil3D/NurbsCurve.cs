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
            if (curve == null || curve.ControlPoints == null || curve.ControlPoints.Count < 2)
                return null;

            try
            {
                // Create empty spline
                var spline = new ACD.Spline();

                // Set control points
                for (int i = 0; i < curve.ControlPoints.Count; i++)
                {
                    spline.SetControlPointAt(i, curve.ControlPoints[i].ToCivil3D());
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
                        catch (Exception ex)
                        {
                            BH.Engine.Base.Compute.RecordWarning($"A knot could not be inserted into the spline and was skipped: {ex.Message}");
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

        /***************************************************/
    }
}
