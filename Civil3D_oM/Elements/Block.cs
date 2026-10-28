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
using BH.oM.Geometry.CoordinateSystem;
using System.ComponentModel;

namespace BH.oM.Adapters.Civil3D.Elements
{
    [Description("An instance of a block definition placed in a drawing.")]
    public class Block : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        [Description("Name of the block definition this instance refers to.")]
        public virtual string DefinitionName { get; set; } = "";

        [Description("Insertion point (origin) and orientation of the instance. The local Z axis is the block normal.")]
        public virtual Cartesian Orientation { get; set; } = null;

        [Description("Scale factors along the instance's local X, Y and Z axes. Null means 1, 1, 1.")]
        public virtual Vector Scale { get; set; } = null;

        /***************************************************/
    }
}
