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

using BH.oM.Base;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.oM.Adapters.Civil3D.Elements
{
    [Description("A named, reusable group of geometry that block instances refer to. The name is stored in the inherited Name property.")]
    public class BlockDefinition : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        [Description("Base point of the definition, in metres.")]
        public virtual Point BasePoint { get; set; } = null;

        [Description("Geometry of the definition, excluding nested blocks, in metres.")]
        public virtual List<IGeometry> Geometry { get; set; } = new List<IGeometry>();

        [Description("Block instances nested inside this definition.")]
        public virtual List<Block> NestedBlocks { get; set; } = new List<Block>();

        /***************************************************/
    }
}
