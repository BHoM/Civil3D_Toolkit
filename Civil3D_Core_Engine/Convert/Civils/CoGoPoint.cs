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
using BHC = BH.oM.Civils.Elements;
using BHG = BH.oM.Geometry;

using ACG = Autodesk.AutoCAD.Geometry;

using ADC = Autodesk.Civil.DatabaseServices;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHC.CoGoPoint FromCivil3D(this ADC.CogoPoint cogoPoint)
        {
            BHC.CoGoPoint pt = new BHC.CoGoPoint
            {
                Point = new BHG.Point { X = cogoPoint.Easting, Y = cogoPoint.Northing, Z = cogoPoint.Elevation },
                PointName = cogoPoint.PointName,
                PointNumber = cogoPoint.PointNumber,
                FullDescription = cogoPoint.FullDescription,
                RawDescription = cogoPoint.RawDescription
            };

            return pt;
        }

        public static void ToCivil3D(this BHC.CoGoPoint bhPoint, ADC.CogoPoint c3dPoint)
        {
            c3dPoint.Easting = bhPoint.Point.X;
            c3dPoint.Northing = bhPoint.Point.Y;
            c3dPoint.Elevation = bhPoint.Point.Z;
            c3dPoint.PointName = bhPoint.PointName;
        }

        /***************************************************/
    }
}


