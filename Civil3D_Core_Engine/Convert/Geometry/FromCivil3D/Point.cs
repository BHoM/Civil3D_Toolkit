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

using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Converts a Civil 3D DBPoint to BH.oM.Geometry.Point.")]
        [Input("acPoint", "A Civil 3D DBPoint to convert.")]
        [Output("point", "The converted BH.oM.Geometry.Point.")]
        public static BHG.Point FromCivil3D(this ACD.DBPoint acPoint)
        {
            return acPoint.Position.FromCivil3D();
        }

        /***************************************************/

        [Description("Converts an AutoCAD Point3d to BH.oM.Geometry.Point.")]
        [Input("pt", "An AutoCAD Point3d to convert.")]
        [Output("point", "The converted BH.oM.Geometry.Point.")]
        public static BHG.Point FromCivil3D(this ACG.Point3d pt)
        {
            return new BHG.Point { X = pt.X, Y = pt.Y, Z = pt.Z };
        }

        /***************************************************/

        [Description("Converts an AutoCAD Point2d to BH.oM.Geometry.Point.")]
        [Input("pt", "An AutoCAD Point2d to convert.")]
        [Output("point", "The converted BH.oM.Geometry.Point.")]
        public static BHG.Point FromCivil3D(this ACG.Point2d pt)
        {
            return new BHG.Point { X = pt.X, Y = pt.Y, Z = 0 };
        }

        /***************************************************/
    }
}
