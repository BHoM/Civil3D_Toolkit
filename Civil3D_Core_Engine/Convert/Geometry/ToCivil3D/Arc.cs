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

        public static ACD.Arc ToCivil3D(this BHG.Arc arc)
        {
            var center = arc.CoordinateSystem.Origin.ToCivil3D();
            var normal = arc.CoordinateSystem.Z.ToCivil3D().GetNormal();
            double radius = arc.Radius;

            var startPt = BH.Engine.Geometry.Query.StartPoint(arc).ToCivil3D();
            var endPt   = BH.Engine.Geometry.Query.EndPoint(arc).ToCivil3D();

            var vStart = startPt - center;
            var vEnd   = endPt   - center;

            double startAngle = AngleFromNormalBasis(normal, vStart);
            double endAngle   = AngleFromNormalBasis(normal, vEnd);

            // Determine original BHoM direction
            bool isCCW = arc.EndAngle > arc.StartAngle;

            if (isCCW)
            {
                if (endAngle < startAngle)
                    endAngle += 2 * Math.PI;
            }
            else
            {
                if (endAngle > startAngle)
                    startAngle += 2 * Math.PI;
            }

            return new ACD.Arc(center, normal, radius, startAngle, endAngle);
        }

        /***************************************************/

        private static double AngleFromNormalBasis(
            ACG.Vector3d normal,
            ACG.Vector3d vector)
        {
            // Ensure unit normal
            normal = normal.GetNormal();

            // AutoCAD-provided safe perpendicular
            ACG.Vector3d xAxis = normal.GetPerpendicularVector().GetNormal();
            ACG.Vector3d yAxis = normal.CrossProduct(xAxis).GetNormal();

            double x = vector.DotProduct(xAxis);
            double y = vector.DotProduct(yAxis);

            return Math.Atan2(y, x);
        }

        /***************************************************/
    }
}
