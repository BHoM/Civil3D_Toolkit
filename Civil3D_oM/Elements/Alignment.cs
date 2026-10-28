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
using BH.oM.Quantities.Attributes;
using System.ComponentModel;

namespace BH.oM.Adapters.Civil3D.Elements
{
    [Description("A horizontal alignment, such as a road or rail centreline, with stationing.")]
    public class Alignment : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        [Description("Purpose of the alignment.")]
        public virtual AlignmentType AlignmentType { get; set; } = AlignmentType.Undefined;

        [Description("User description of the alignment.")]
        public virtual string Description { get; set; } = "";

        [Description("Plan geometry of the alignment as one continuous curve (usually a PolyCurve of lines and arcs), in metres. Spirals are approximated.")]
        public virtual ICurve Curve { get; set; } = null;

        [Length]
        [Description("Station value at the start of the alignment, in metres.")]
        public virtual double StartingStation { get; set; } = 0.0;

        [Description("Point at which the reference station is defined.")]
        public virtual Point ReferencePoint { get; set; } = null;

        [Length]
        [Description("Station value at the reference point, in metres.")]
        public virtual double ReferencePointStation { get; set; } = 0.0;

        [Description("Name of the Civil 3D site the alignment belongs to. Empty if it is siteless.")]
        public virtual string SiteName { get; set; } = "";

        /***************************************************/
    }
}
