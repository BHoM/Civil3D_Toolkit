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
using BH.oM.Base.Attributes;
using BH.oM.Geometry;

using Autodesk.AutoCAD.DatabaseServices;
using ADC = Autodesk.Civil.DatabaseServices;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        [Description("Converts a Civil 3D BlockTableRecord to BH.oM.Adapters.Civil3D.Elements.BlockDefinition, with its geometry in definition coordinates and its block references as nested blocks.")]
        [Input("definitionBtr", "A Civil 3D BlockTableRecord to convert.")]
        [Input("tr", "Open transaction on the drawing.")]
        [Input("db", "Drawing database.")]
        [Input("definitionCache", "Block definitions already converted, reused to avoid duplicates and endless recursion.")]
        [Input("includeNestedBlocks", "If true, block references inside the definition are returned as nested blocks.")]
        [Input("skipXrefBlocks", "If true, external reference blocks are returned as empty placeholders.")]
        [Output("blockDefinition", "The converted BH.oM.Adapters.Civil3D.Elements.BlockDefinition.")]
        public static BHC.BlockDefinition FromCivil3D(
            this BlockTableRecord definitionBtr,
            Transaction tr,
            Database db,
            IDictionary<ObjectId, BHC.BlockDefinition> definitionCache = null,
            bool includeNestedBlocks = true,
            bool skipXrefBlocks = true)
        {
            if (definitionBtr == null) throw new ArgumentNullException(nameof(definitionBtr));
            if (tr == null) throw new ArgumentNullException(nameof(tr));
            if (db == null) throw new ArgumentNullException(nameof(db));

            if (definitionCache == null)
            {
                definitionCache = new Dictionary<ObjectId, BHC.BlockDefinition>();
            }

            // If already converted, return cached instance (break recursion / reuse)
            if (definitionCache.TryGetValue(definitionBtr.ObjectId, out BHC.BlockDefinition cached))
                return cached;

            // Optionally skip external reference blocks (xref)
            if (skipXrefBlocks && definitionBtr.IsFromExternalReference)
            {
                // Still cache a minimal placeholder so nested refs won’t recurse forever
                var xrefPlaceholder = new BHC.BlockDefinition
                {
                    Name = SafeBlockName(definitionBtr.Name),
                    BasePoint = definitionBtr.Origin.FromCivil3D()
                };
                definitionCache[definitionBtr.ObjectId] = xrefPlaceholder;
                return xrefPlaceholder;
            }

            // Create definition first and cache immediately (important for circular nesting)
            var bhDef = new BHC.BlockDefinition
            {
                Name = SafeBlockName(definitionBtr.Name),
                BasePoint = definitionBtr.Origin.FromCivil3D(),
                Geometry = new List<IGeometry>(),
                NestedBlocks = new List<BHC.Block>()
            };
            definitionCache[definitionBtr.ObjectId] = bhDef;

            // Iterate entities within the definition
            foreach (ObjectId entId in definitionBtr)
            {
                if (!entId.IsValid || entId.IsErased) continue;

                var dbo = tr.GetObject(entId, OpenMode.ForRead, false);
                if (dbo == null) continue;

                // Nested block reference inside definition
                if (includeNestedBlocks && dbo is BlockReference nestedBr)
                {
                    // Resolve the referenced definition record
                    ObjectId nestedDefId = nestedBr.IsDynamicBlock
                        ? nestedBr.DynamicBlockTableRecord
                        : nestedBr.BlockTableRecord;

                    if (nestedDefId.IsNull || !nestedDefId.IsValid)
                        continue;

                    var nestedDefBtr = tr.GetObject(nestedDefId, OpenMode.ForRead) as BlockTableRecord;
                    if (nestedDefBtr == null)
                        continue;

                    // Recursively convert nested definition (cached)
                    var nestedBhDef = nestedDefBtr.FromCivil3D(
                        tr, db, definitionCache,
                        includeNestedBlocks: includeNestedBlocks,
                        skipXrefBlocks: skipXrefBlocks);

                    // Create instance (transform relative to THIS definition). BlockReference.BlockName is the
                    // name of the block that owns the reference, so the referenced definition's name is used.
                    var nestedInst = new BHC.Block
                    {
                        DefinitionName = SafeBlockName(nestedDefBtr.Name),
                        Orientation = BlockOrientation(nestedBr),
                        Scale = nestedBr.ScaleFactors.FromCivil3D()
                    };

                    bhDef.NestedBlocks.Add(nestedInst);
                    continue;
                }

                // Geometry (entities inside definition)
                if (dbo is Entity ent)
                {
                    IGeometry g = ent.IFromCivil3D(tr) as IGeometry;

                    if (g != null)
                        bhDef.Geometry.Add(g);

                    // Unsupported entities are skipped
                    continue;
                }
            }

            return bhDef;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static string SafeBlockName(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? "" : name;
        }

        /***************************************************/
    }
}
