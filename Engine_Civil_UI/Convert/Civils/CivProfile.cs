/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

using ACG = Autodesk.AutoCAD.Geometry;

using ADC = Autodesk.Civil.DatabaseServices;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHC.CivProfile FromCivil3D(this ADC.Profile civProfile)
        {
            return new BHC.CivProfile
            {
                DataSourceName = civProfile.DataSourceName,
                Description = civProfile.Description,
                DisplayName = civProfile.DisplayName,
                ElevationMax = civProfile.ElevationMax,
                ElevationMin = civProfile.ElevationMin,
                EndingStation = civProfile.EndingStation,
                Length = civProfile.Length,
                Name = civProfile.Name,
                Offset = civProfile.Offset,
                ProfileType = civProfile.ProfileType.FromCivil3D(),
                StartingStation = civProfile.StartingStation,
            };
        }

        /***************************************************/
    }
}

