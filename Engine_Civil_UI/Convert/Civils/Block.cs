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
using BHC = BH.oM.Civils.Elements;

using ACG = Autodesk.AutoCAD.Geometry;

using ADC = Autodesk.Civil.DatabaseServices;

using Autodesk.AutoCAD.DatabaseServices;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHC.Block FromCivil3D(this BlockReference civBlock)
        {
            return new BHC.Block
            {
                BlockName = civBlock.BlockName,
                CanCastShadow = civBlock.CastShadows,
                IsPersistent = civBlock.IsPersistent,
                IsPlanar = civBlock.IsPlanar,
                Layer = civBlock.Layer,
                Material = civBlock.Material,
                CanReceiveShadow = civBlock.ReceiveShadows,
                Rotation = civBlock.Rotation,
                IsVisible = civBlock.Visible,
                Position = civBlock.Position.FromCivil3D(),
                Normal = civBlock.Normal.FromCivil3D(),
                CollisionType = civBlock.CollisionType.FromCivil3D(),
                Bounds = civBlock.Bounds.HasValue ? civBlock.Bounds.Value.FromCivil3D() : new oM.Geometry.BoundingBox(),
                Name = civBlock.Name,
            };
        }

        /***************************************************/
    }
}


