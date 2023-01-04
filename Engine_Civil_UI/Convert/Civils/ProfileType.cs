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

        public static BHC.CivProfileType FromCivil3D(this ADC.ProfileType profileType)
        {
            switch(profileType)
            {
                case ADC.ProfileType.CorridorFeature:
                    return BHC.CivProfileType.CorridorFeature;
                case ADC.ProfileType.CurbReturnProfile:
                    return BHC.CivProfileType.CurbReturnProfile;
                case ADC.ProfileType.EG:
                    return BHC.CivProfileType.EG;
                case ADC.ProfileType.FG:
                    return BHC.CivProfileType.FG;
                case ADC.ProfileType.File:
                    return BHC.CivProfileType.File;
                case ADC.ProfileType.OffsetProfile:
                    return BHC.CivProfileType.OffsetProfile;
                case ADC.ProfileType.Superimposed:
                    return BHC.CivProfileType.Superimposed;
                default:
                    return BHC.CivProfileType.Undefined;
            }
        }

        /***************************************************/
    }
}

