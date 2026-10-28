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

using BHC = BH.oM.Adapters.Civil3D.Elements;

using ACG = Autodesk.AutoCAD.Geometry;

using Autodesk.AutoCAD.DatabaseServices;

using BH.oM.Base.Attributes;

using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Converts a BH.oM.Adapters.Civil3D.Elements.Block to a Civil 3D BlockReference of an existing block definition. Returns null if the definition is not in the drawing.")]
        [Input("block", "BH.oM.Adapters.Civil3D.Elements.Block to convert.")]
        [Input("blockTable", "Block table of the drawing, used to find the block definition.")]
        [Input("transaction", "Open transaction on the drawing.")]
        [Output("blockReference", "The converted Civil 3D BlockReference.")]
        public static BlockReference ToCivil3D(this BHC.Block block, BlockTable blockTable, Transaction transaction)
        {
            if (block == null || blockTable == null || transaction == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot convert a null block, or convert a block without a block table and transaction.");
                return null;
            }

            if (block.Orientation == null)
            {
                BH.Engine.Base.Compute.RecordError("A Block has no Orientation, so it could not be converted to a Civil 3D block reference.");
                return null;
            }

            if (string.IsNullOrEmpty(block.DefinitionName) || !blockTable.Has(block.DefinitionName))
            {
                BH.Engine.Base.Compute.RecordError($"Block definition '{block.DefinitionName}' does not exist in the drawing. Pushing block definitions is not supported yet.");
                return null;
            }

            ACG.Vector3d normal = block.Orientation.Z.ToCivil3D().GetNormal();

            //Rotation is the angle of the local X axis in the plane of the block's OCS, which AutoCAD derives from the normal
            ACG.Vector3d xInOcs = block.Orientation.X.ToCivil3D().TransformBy(ACG.Matrix3d.WorldToPlane(normal));

            BlockReference blockReference = new BlockReference(block.Orientation.Origin.ToCivil3D(), blockTable[block.DefinitionName])
            {
                Normal = normal,
                Rotation = System.Math.Atan2(xInOcs.Y, xInOcs.X),
                ScaleFactors = block.Scale == null ? new ACG.Scale3d(1, 1, 1) : block.Scale.ToCivil3DScale(),
            };

            return blockReference;
        }

        /***************************************************/
    }
}
