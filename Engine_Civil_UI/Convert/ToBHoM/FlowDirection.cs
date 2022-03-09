/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using ADC = Autodesk.Civil.DatabaseServices;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {
        public static BHC.FlowDirection ToBHoM(this ADC.FlowDirectionMethodType acFlowDirectionType)
        {
            switch (acFlowDirectionType)
            {
                case Autodesk.Civil.DatabaseServices.FlowDirectionMethodType.Bidirectional:
                    return BHC.FlowDirection.Bidirectional;
                case Autodesk.Civil.DatabaseServices.FlowDirectionMethodType.BySlope:
                    return BHC.FlowDirection.BySlope;
                case Autodesk.Civil.DatabaseServices.FlowDirectionMethodType.EndToStart:
                    return BHC.FlowDirection.EndToStart;
                case Autodesk.Civil.DatabaseServices.FlowDirectionMethodType.StartToEnd:
                    return BHC.FlowDirection.StartToEnd;
                default:
                    return BHC.FlowDirection.Undefined;
            }
        }
    }
}
