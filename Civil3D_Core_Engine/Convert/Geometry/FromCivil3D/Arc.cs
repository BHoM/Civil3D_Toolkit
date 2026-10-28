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

        [Description("Converts a Civil 3D Arc to BH.oM.Geometry.Arc.")]
        [Input("acArc", "A Civil 3D Arc to convert.")]
        [Output("arc", "The converted BH.oM.Geometry.Arc.")]
        public static BHG.Arc FromCivil3D(this ACD.Arc acArc)
        {
            if (acArc == null)
                return null;

            ACG.Point3d mid = acArc.GetPointAtParameter((acArc.StartParam + acArc.EndParam) / 2);
            return BH.Engine.Geometry.Create.Arc(acArc.StartPoint.FromCivil3D(), mid.FromCivil3D(), acArc.EndPoint.FromCivil3D());
        }

        /***************************************************/
    }
}
