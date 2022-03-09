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
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Civils.Elements
{
    public class ManholeChamber : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        public virtual Point CentrePoint { get; set; } = new Point();

        public virtual double InternalLength { get; set; } = 0;

        public virtual double InternalWidth { get; set; } = 0;

        public virtual double InternalDepth { get; set; } = 0;

        public virtual double WallThickness { get; set; } = 0;

        public virtual double SurroundThickness { get; set; } = 0;

        public virtual double ChamberOrientation { get; set; } = 0;

        public virtual ChamberShape ChamberShape { get; set; } = ChamberShape.Undefined;

        public virtual double CoverLength { get; set; } = 0;

        public virtual double CoverWidth { get; set; } = 0;

        public virtual double CoverDepth { get; set; } = 0;

        public virtual double CoverOrientation { get; set; } = 0;

        public virtual double BeddingDepth { get; set; } = 0;

        /***************************************************/
    }
}
