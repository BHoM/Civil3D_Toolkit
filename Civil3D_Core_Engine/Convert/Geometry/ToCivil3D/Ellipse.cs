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
using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Converts BH.oM.Geometry.Ellipse to a Civil 3D Ellipse.")]
        [Input("ellipse", "BH.oM.Geometry.Ellipse to convert.")]
        [Output("ellipse", "The converted Civil 3D Ellipse.")]
        public static ACD.Ellipse ToCivil3D(this BHG.Ellipse ellipse)
        {
            if (ellipse == null)
                return null;

            bool firstIsMajor = ellipse.Radius1 >= ellipse.Radius2;
            double majorRadius = firstIsMajor ? ellipse.Radius1 : ellipse.Radius2;
            double minorRadius = firstIsMajor ? ellipse.Radius2 : ellipse.Radius1;
            ACG.Vector3d majorDirection = (firstIsMajor ? ellipse.Axis1 : ellipse.Axis2).ToCivil3D().GetNormal();

            ACG.Vector3d unitNormal = ellipse.Axis1.ToCivil3D().CrossProduct(ellipse.Axis2.ToCivil3D()).GetNormal();
            ACG.Vector3d majorAxis = majorDirection * majorRadius;

            return new ACD.Ellipse(ellipse.Centre.ToCivil3D(), unitNormal, majorAxis, minorRadius / majorRadius, 0.0, 2.0 * Math.PI);
        }

        /***************************************************/
    }
}
