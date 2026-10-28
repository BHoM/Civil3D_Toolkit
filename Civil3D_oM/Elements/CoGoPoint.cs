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
using System.ComponentModel;

namespace BH.oM.Adapters.Civil3D.Elements
{
    [Description("A coordinate geometry (COGO) survey point with a number and descriptions. The point name is stored in the inherited Name property.")]
    public class CoGoPoint : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        [Description("Position of the survey point. X is easting, Y is northing and Z is elevation, in metres.")]
        public virtual Point Location { get; set; } = null;

        [Description("Unique number of the point within the drawing. 0 means a number is assigned on Push.")]
        public virtual int PointNumber { get; set; } = 0;

        [Description("Description as entered by the surveyor (raw description).")]
        public virtual string RawDescription { get; set; } = "";

        [Description("Full description after description-key processing. Read-only in Civil 3D; ignored on Push.")]
        public virtual string FullDescription { get; set; } = "";

        /***************************************************/
    }
}
