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
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Civils.Elements
{
    public class CivProfileTangent : BHoMObject, ICivProfileEntity
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        public virtual CivProfileConstraintType Constraint1 { get; set; } = CivProfileConstraintType.Undefined;

        public virtual CivProfileConstraintType Constraint2 { get; set; } = CivProfileConstraintType.Undefined;

        public virtual double EndElevation { get; set; } = 0;

        public virtual double EndStation { get; set; } = 0;

        public virtual int EntityAfter { get; set; } = 0;

        public virtual int EntityBefore { get; set; } = 0;

        public virtual int EntityId { get; set; } = 0;

        public virtual CivProfileEntityType EntityType { get; set; } = CivProfileEntityType.Undefined;

        public virtual double Grade { get; set; } = 0;

        public virtual double HighestDesignSpeed { get; set; } = 0;

        public virtual double Length { get; set; } = 0;

        public virtual double MinimumKValueHSD { get; set; } = 0;

        public virtual double MinimumKValuePSD { get; set; } = 0;

        public virtual double MinimumKValueSSD { get; set; } = 0;

        public virtual double StartElevation { get; set; } = 0;

        public virtual double StartStation { get; set; } = 0;

        public virtual double ThroughPoint1Elevation { get; set; } = 0;

        public virtual double ThroughPoint1Station { get; set; } = 0;

        public virtual double ThroughPoint2Elevation { get; set; } = 0;

        public virtual double ThroughPoint2Station { get; set; } = 0;

        /***************************************************/
    }
}





