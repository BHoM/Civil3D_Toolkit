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
    public class Block : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        public virtual string BlockName { get; set; } = "";
        public virtual BoundingBox Bounds { get; set; } = new BoundingBox();
        public virtual bool CanCastShadow { get; set; } = false;
        public virtual CollisionType CollisionType { get; set; } = CollisionType.Undefined;
        public virtual bool IsPersistent { get; set; } = false;
        public virtual bool IsPlanar { get; set; } = false;
        public virtual string Layer { get; set; } = "";
        public virtual string Material { get; set; } = "";
        public virtual Vector Normal { get; set; } = new Vector();
        public virtual Point Position { get; set; } = new Point();
        public virtual bool CanReceiveShadow { get; set; } = false;
        public virtual double Rotation { get; set; } = 0;
        public virtual bool IsVisible { get; set; } = false;

        /***************************************************/
    }
}


