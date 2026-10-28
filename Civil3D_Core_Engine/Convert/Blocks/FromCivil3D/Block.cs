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

using BHC = BH.oM.Adapters.Civil3D.Elements;
using BHG = BH.oM.Geometry;
using BHCS = BH.oM.Geometry.CoordinateSystem;

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

        [Description("Converts a Civil 3D BlockReference to BH.oM.Adapters.Civil3D.Elements.Block.")]
        [Input("br", "A Civil 3D BlockReference to convert.")]
        [Input("tr", "Open transaction on the drawing.")]
        [Output("block", "The converted BH.oM.Adapters.Civil3D.Elements.Block.")]
        public static BHC.Block FromCivil3D(this BlockReference br, Transaction tr)
        {
            ObjectId defId = br.DynamicBlockTableRecord.IsNull
                ? br.BlockTableRecord
                : br.DynamicBlockTableRecord;

            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(defId, OpenMode.ForRead);

            return new BHC.Block
            {
                DefinitionName = SafeBlockName(btr.Name),
                Orientation = BlockOrientation(br),
                Scale = br.ScaleFactors.FromCivil3D(),
            };
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        //The block transform's axes are scaled by the scale factors. Dividing each axis by its own signed
        //factor gives unit axes, and keeps the sign of a mirrored block in Scale rather than applying it twice.
        private static BHCS.Cartesian BlockOrientation(BlockReference br)
        {
            ACG.CoordinateSystem3d cs = br.BlockTransform.CoordinateSystem3d;
            ACG.Vector3d x = cs.Xaxis.DivideBy(br.ScaleFactors.X);
            ACG.Vector3d y = cs.Yaxis.DivideBy(br.ScaleFactors.Y);

            return BH.Engine.Geometry.Create.CartesianCoordinateSystem(br.Position.FromCivil3D(), x.FromCivil3D(), y.FromCivil3D());
        }

        /***************************************************/
    }
}
