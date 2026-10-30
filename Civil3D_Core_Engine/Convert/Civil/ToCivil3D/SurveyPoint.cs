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

using BHC = BH.oM.Adapters.Civil3D.Elements;
using BH.oM.Base.Attributes;

using ADC = Autodesk.Civil.DatabaseServices;

using System;
using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Applies the location, name, raw description and point number of a BH.oM.Adapters.Civil3D.Elements.SurveyPoint to an existing Civil 3D CogoPoint. A point number must not already be in use in the drawing.")]
        [Input("bhPoint", "BH.oM.Adapters.Civil3D.Elements.SurveyPoint to convert.")]
        [Input("c3dPoint", "Civil 3D point to apply the values to.")]
        public static void ToCivil3D(this BHC.SurveyPoint bhPoint, ADC.CogoPoint c3dPoint)
        {
            if (bhPoint == null || c3dPoint == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot apply a null survey point, or apply one to a null Civil 3D point.");
                return;
            }

            if (bhPoint.Location != null)
            {
                c3dPoint.Easting = bhPoint.Location.X;
                c3dPoint.Northing = bhPoint.Location.Y;
                c3dPoint.Elevation = bhPoint.Location.Z;
            }

            if (!string.IsNullOrWhiteSpace(bhPoint.Name))
                c3dPoint.PointName = bhPoint.Name;

            if (!string.IsNullOrEmpty(bhPoint.RawDescription))
                c3dPoint.RawDescription = bhPoint.RawDescription;

            // A number of 0 or less keeps the number Civil 3D assigned. The caller checks that the number is free.
            if (bhPoint.PointNumber > 0)
                c3dPoint.PointNumber = (uint)bhPoint.PointNumber;
        }

        /***************************************************/
    }
}
