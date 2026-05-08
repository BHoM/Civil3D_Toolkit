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
using BH.oM.Geometry;

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
        public static BlockReference ToCivil3D(this BHC.BlockInstance blockInstance)
        {
            if (blockInstance == null)
                return null;
                
            /// Still to do:
            ///get objectid of block definition from Civil 3D drawing using blockInstance.Name
            /// If this doesnt exist, create a new block definition with circle geometry as a placeholder, and use that objectid

            BlockReference blockRef = new BlockReference(
                new ACG.Point3d(blockInstance.Position.X, blockInstance.Position.Y, blockInstance.Position.Z),
                ObjectId.Null // This will need to be set to the actual BlockTableRecord ID of the block definition in the Civil 3D drawing
            )
            {
                Rotation = blockInstance.Rotation,
                ScaleFactors = new ACG.Scale3d(blockInstance.Scale.X, blockInstance.Scale.Y, blockInstance.Scale.Z),
                Normal = new ACG.Vector3d(blockInstance.Normal.X, blockInstance.Normal.Y, blockInstance.Normal.Z)
            };
            return blockRef;
        }

        public static BHC.BlockInstance FromCivil3D(this BlockReference br, Transaction tr)
        {
            ObjectId defId = br.DynamicBlockTableRecord.IsNull
                ? br.BlockTableRecord
                : br.DynamicBlockTableRecord;

            BlockTableRecord btr =
                (BlockTableRecord)tr.GetObject(defId, OpenMode.ForRead);

            string blockName = btr.Name;
            string blockHandle = btr.Handle.ToString();
            

            return new BHC.BlockInstance
            {
                Name = SafeBlockName(btr.Name),
                Position  = br.Position.FromCivil3D(),
                Rotation  = br.Rotation,
                Scale     = new Vector
                {
                    X = br.ScaleFactors.X,
                    Y = br.ScaleFactors.Y,
                    Z = br.ScaleFactors.Z
                },
                Normal  = br.Normal.FromCivil3D(),
            };
        }

    }
}


