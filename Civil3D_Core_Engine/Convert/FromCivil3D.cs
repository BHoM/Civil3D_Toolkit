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

using ACD = Autodesk.AutoCAD.DatabaseServices;
using ADC = Autodesk.Civil.DatabaseServices;

using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Converts a drawing object to the matching BHoM object: geometry for points and curves, TextNote for text, Block for block references, SurveyPoint for COGO points, and the Civil 3D element for feature lines and parcels. Returns null for any other type.")]
        [Input("entity", "Drawing object to convert.")]
        [Input("transaction", "Open transaction, used to look up block definitions. Only needed for block references.")]
        [Output("object", "The converted BHoM object, or null if the type is not supported.")]
        public static object IFromCivil3D(this ACD.Entity entity, ACD.Transaction transaction)
        {
            switch (entity)
            {
                case null:
                    return null;
                case ADC.CogoPoint cogoPoint:
                    return cogoPoint.FromCivil3D();
                case ADC.FeatureLine featureLine:
                    return featureLine.FromCivil3D();
                case ADC.Parcel parcel:
                    return parcel.FromCivil3D();
                case ACD.DBPoint point:
                    return point.FromCivil3D();
                case ACD.Line line:
                    return line.FromCivil3D();
                case ACD.Arc arc:
                    return arc.FromCivil3D();
                case ACD.Circle circle:
                    return circle.FromCivil3D();
                case ACD.Ellipse ellipse:
                    return ellipse.FromCivil3D();
                case ACD.Polyline polyline:
                    return polyline.FromCivil3D();
                case ACD.Polyline2d polyline2d:
                    return polyline2d.FromCivil3D();
                case ACD.Polyline3d polyline3d:
                    return polyline3d.FromCivil3D();
                case ACD.Spline spline:
                    return spline.FromCivil3D();
                case ACD.MText mtext:
                    return mtext.FromCivil3D();
                case ACD.DBText text:
                    return text.FromCivil3D();
                case ACD.BlockReference blockReference:
                    if (transaction == null)
                    {
                        BH.Engine.Base.Compute.RecordError("A transaction is needed to convert a block reference.");
                        return null;
                    }
                    return blockReference.FromCivil3D(transaction);
                default:
                    return null;
            }
        }

        /***************************************************/
    }
}
