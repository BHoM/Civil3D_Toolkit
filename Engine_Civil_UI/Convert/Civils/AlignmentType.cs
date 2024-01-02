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

using ACG = Autodesk.AutoCAD.Geometry;

using ADC = Autodesk.Civil.DatabaseServices;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHC.CivAlignmentType FromCivil3D(this ADC.AlignmentType alignmentType)
        {
            switch (alignmentType)
            {
                case ADC.AlignmentType.Centerline:
                    return BHC.CivAlignmentType.Centerline;
                case ADC.AlignmentType.CurbReturn:
                    return BHC.CivAlignmentType.CurbReturn;
                case ADC.AlignmentType.Offset:
                    return BHC.CivAlignmentType.Offset;
                case ADC.AlignmentType.Rail:
                    return BHC.CivAlignmentType.Rail;
                case ADC.AlignmentType.Utility:
                    return BHC.CivAlignmentType.Utility;
                default:
                    return BHC.CivAlignmentType.Undefined;
            }
        }

        /***************************************************/
    }
}


