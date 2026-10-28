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

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHG.Arc FromCivil3D(this ACD.Arc acArc)
        {
            var c = new BHG.Point { X = acArc.Center.X, Y = acArc.Center.Y, Z = acArc.Center.Z };

            var n = new BHG.Vector { X = acArc.Normal.X, Y = acArc.Normal.Y, Z = acArc.Normal.Z };
            n = BH.Engine.Geometry.Modify.Normalise(n);

            var x = BH.Engine.Geometry.Modify.Rotate(
                BH.Engine.Geometry.Create.Vector(1, 0, 0),
                acArc.StartAngle,
                n
            );
            x = BH.Engine.Geometry.Modify.Normalise(x);

            var y = BH.Engine.Geometry.Modify.Normalise(
                BH.Engine.Geometry.Query.CrossProduct(n, x)
            );

            var cs = BH.Engine.Geometry.Create.CartesianCoordinateSystem(c, x, y);

            double sweep = acArc.EndAngle - acArc.StartAngle;
            if (sweep < 0) sweep += 2 * Math.PI;

            return BH.Engine.Geometry.Create.Arc(cs, acArc.Radius, 0.0, sweep);
        }

        /***************************************************/
    }
}
