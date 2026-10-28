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

using BHC = BH.oM.Adapters.Civil3D.Elements;
using BH.oM.Base.Attributes;

using ADC = Autodesk.Civil.DatabaseServices;

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

        [Description("Converts a Civil 3D Alignment to BH.oM.Adapters.Civil3D.Elements.Alignment, with its plan geometry as one PolyCurve.")]
        [Input("alignmentType", "A Civil 3D Alignment to convert.")]
        [Output("alignmentType", "The converted BH.oM.Adapters.Civil3D.Elements.AlignmentType.")]
        public static BHC.AlignmentType FromCivil3D(this ADC.AlignmentType alignmentType)
        {
            switch (alignmentType)
            {
                case ADC.AlignmentType.Centerline:
                    return BHC.AlignmentType.Centreline;
                case ADC.AlignmentType.CurbReturn:
                    return BHC.AlignmentType.KerbReturn;
                case ADC.AlignmentType.Offset:
                    return BHC.AlignmentType.Offset;
                case ADC.AlignmentType.Rail:
                    return BHC.AlignmentType.Rail;
                case ADC.AlignmentType.Utility:
                    return BHC.AlignmentType.Utility;
                default:
                    return BHC.AlignmentType.Undefined;
            }
        }

        /***************************************************/
    }
}
