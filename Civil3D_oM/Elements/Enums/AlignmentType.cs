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

using System.ComponentModel;

namespace BH.oM.Adapters.Civil3D.Elements
{
    [Description("Defines the purpose of a horizontal alignment.")]
    public enum AlignmentType
    {
        [Description("The alignment type is not known or not set.")]
        Undefined,
        [Description("A road or corridor centreline.")]
        Centreline,
        [Description("An alignment offset from a parent alignment.")]
        Offset,
        [Description("A kerb return at a junction.")]
        KerbReturn,
        [Description("A utility alignment, e.g. for services.")]
        Utility,
        [Description("A rail alignment.")]
        Rail,
    }
}
