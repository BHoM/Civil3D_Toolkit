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
using BHC = BH.oM.Adapters.Civil3D.Elements;
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

        public static void ToCivil3D(this BHC.CoGoPoint bhPoint, ADC.CogoPoint c3dPoint)
        {
            if (bhPoint == null || c3dPoint == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot apply a null CoGo point, or apply one to a null Civil 3D point.");
                return;
            }

            if (bhPoint.Location != null)
            {
                c3dPoint.Easting = bhPoint.Location.X;
                c3dPoint.Northing = bhPoint.Location.Y;
                c3dPoint.Elevation = bhPoint.Location.Z;
            }

            if (!string.IsNullOrWhiteSpace(bhPoint.Name))
                c3dPoint.PointName = bhPoint.Name;

            if (!string.IsNullOrEmpty(bhPoint.RawDescription))
                c3dPoint.RawDescription = bhPoint.RawDescription;

            // A number of 0 or less keeps the number Civil 3D assigned. A number already in use cannot be applied, so the point keeps its own.
            if (bhPoint.PointNumber > 0)
            {
                try
                {
                    c3dPoint.PointNumber = (uint)bhPoint.PointNumber;
                }
                catch (Exception e)
                {
                    BH.Engine.Base.Compute.RecordWarning($"Point number {bhPoint.PointNumber} could not be used ({e.Message}), so Civil 3D's number {c3dPoint.PointNumber} was kept.");
                }
            }
        }

        /***************************************************/
    }
}
