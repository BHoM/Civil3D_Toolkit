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

using BH.Engine.Geometry;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;

using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Scales geometry uniformly about the origin, e.g. to convert it between drawing units and metres.")]
        [Input("geometry", "Geometry to scale.")]
        [Input("factor", "Scale factor applied in X, Y and Z.")]
        [Output("geometry", "Scaled copy of the geometry, or the input itself if the factor is 1.")]
        public static IGeometry ScaleGeometry(this IGeometry geometry, double factor)
        {
            if (geometry == null || factor == 1.0)
                return geometry;

            return geometry.ITransform(BH.Engine.Geometry.Create.ScaleMatrix(new Point(), new Vector { X = factor, Y = factor, Z = factor }));
        }

        /***************************************************/
    }
}
