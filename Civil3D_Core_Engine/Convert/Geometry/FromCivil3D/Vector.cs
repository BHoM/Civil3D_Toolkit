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

using ACG = Autodesk.AutoCAD.Geometry;

using BH.oM.Base.Attributes;

using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Converts an AutoCAD Vector3d to BH.oM.Geometry.Vector.")]
        [Input("vec", "An AutoCAD Vector3d to convert.")]
        [Output("vector", "The converted BH.oM.Geometry.Vector.")]
        public static BHG.Vector FromCivil3D(this ACG.Vector3d vec)
        {
            return new BHG.Vector { X = vec.X, Y = vec.Y, Z = vec.Z };
        }

        /***************************************************/

        [Description("Converts an AutoCAD Scale3d to BH.oM.Geometry.Vector.")]
        [Input("vec", "An AutoCAD Scale3d to convert.")]
        [Output("vector", "The converted BH.oM.Geometry.Vector.")]
        public static BHG.Vector FromCivil3D(this ACG.Scale3d vec)
        {
            return new BHG.Vector { X = vec.X, Y = vec.Y, Z = vec.Z };
        }

        /***************************************************/
    }
}
