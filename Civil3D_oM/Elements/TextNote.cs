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
using BH.oM.Geometry.CoordinateSystem;
using BH.oM.Quantities.Attributes;
using System.ComponentModel;

namespace BH.oM.Adapters.Civil3D.Elements
{
    [Description("A single- or multi-line text annotation placed in a drawing.")]
    public class TextNote : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        [Description("The text content without formatting codes.")]
        public virtual string Text { get; set; } = "";

        [Description("Insertion point (origin) and orientation of the text. The local X axis is the reading direction and the local Z axis is the text normal.")]
        public virtual Cartesian Orientation { get; set; } = null;

        [Length]
        [Description("Text height, in metres.")]
        public virtual double Height { get; set; } = 0.0;

        /***************************************************/
    }
}
