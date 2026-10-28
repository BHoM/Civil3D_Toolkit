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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Converts a BH.oM.Geometry.Point to a Civil 3D DBPoint.")]
        [Input("pt", "A BH.oM.Geometry.Point to convert.")]
        [Output("point", "The converted Civil 3D DBPoint.")]
        public static ACD.DBPoint ToCivil3DDBPoint(this BHG.Point pt)
        {
            return new ACD.DBPoint(new ACG.Point3d(pt.X, pt.Y, pt.Z));
        }

        /***************************************************/

        [Description("Converts BH.oM.Geometry.Point to an AutoCAD Point3d.")]
        [Input("pt", "BH.oM.Geometry.Point to convert.")]
        [Output("point3d", "The converted AutoCAD Point3d.")]
        public static ACG.Point3d ToCivil3D(this BHG.Point pt)
        {
            return new ACG.Point3d(pt.X, pt.Y, pt.Z);
        }

        /***************************************************/

        [Description("Converts a BH.oM.Geometry.Point to an AutoCAD Point2d, dropping its Z.")]
        [Input("pt", "A BH.oM.Geometry.Point to convert.")]
        [Output("point2d", "The converted AutoCAD Point2d.")]
        public static ACG.Point2d ToCivil3D2d(this BHG.Point pt)
        {
            return new ACG.Point2d(pt.X, pt.Y);
        }

        /***************************************************/
    }
}
