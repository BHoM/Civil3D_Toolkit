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

using ACG = Autodesk.AutoCAD.Geometry;

using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Converts BH.oM.Geometry.Vector to an AutoCAD Vector3d.")]
        [Input("vec", "BH.oM.Geometry.Vector to convert.")]
        [Output("vector3d", "The converted AutoCAD Vector3d.")]
        public static ACG.Vector3d ToCivil3D(this BHG.Vector vec)
        {
            return new ACG.Vector3d(vec.X, vec.Y, vec.Z);
        }

        /***************************************************/

        [Description("Converts a BH.oM.Geometry.Vector to AutoCAD scale factors.")]
        [Input("vec", "A BH.oM.Geometry.Vector to convert.")]
        [Output("scale3d", "The converted AutoCAD Scale3d.")]
        public static ACG.Scale3d ToCivil3DScale(this BHG.Vector vec)
        {
            return new ACG.Scale3d(vec.X, vec.Y, vec.Z);
        }

        /***************************************************/
    }
}
