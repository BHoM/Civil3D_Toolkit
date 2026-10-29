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

using Autodesk.AutoCAD.DatabaseServices;

using System;
using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets the global width of a lightweight polyline: the width it has along its whole length, in drawing units. Returns 0 for any other drawing object, and for a polyline whose width varies or is 0.")]
        [Input("entity", "Drawing object to get the global width of.")]
        [Output("width", "Constant width of the polyline in drawing units, or 0.")]
        public static double GlobalWidth(this Entity entity)
        {
            Polyline polyline = entity as Polyline;
            if (polyline == null || polyline.NumberOfVertices < 2)
                return 0.0;

            //The ConstantWidth getter throws when the widths vary, so the segment widths are compared directly.
            double tolerance = 1e-9;
            int segments = polyline.Closed ? polyline.NumberOfVertices : polyline.NumberOfVertices - 1;
            double width = polyline.GetStartWidthAt(0);
            for (int i = 0; i < segments; i++)
            {
                if (Math.Abs(polyline.GetStartWidthAt(i) - width) > tolerance || Math.Abs(polyline.GetEndWidthAt(i) - width) > tolerance)
                    return 0.0;
            }

            return width;
        }


        /***************************************************/
    }
}
