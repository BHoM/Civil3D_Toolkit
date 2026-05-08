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
        /// <summary>
        /// Convert a CAD BlockTableRecord (block definition) into a BHoM BlockDefinition.
        /// - Geometry is stored in local block coordinates (definition space)
        /// - Nested block references are captured as BlockInstance objects (recursive)
        /// - Uses cache to prevent duplication and infinite recursion
        /// </summary>
        public static BHC.BlockDefinition ToBHoMBlockDefinition(
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
                    Name = SafeBlockName(definitionBtr.Name)
                };
                definitionCache[definitionBtr.ObjectId] = xrefPlaceholder;
                return xrefPlaceholder;
            }

            // Create definition first and cache immediately (important for circular nesting)
            var bhDef = new BHC.BlockDefinition
            {
                Name = SafeBlockName(definitionBtr.Name),
                Geometry = new List<IGeometry>(),
                NestedBlocks = new List<BHC.BlockInstance>()
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
                    var nestedBhDef = nestedDefBtr.ToBHoMBlockDefinition(
                        tr, db, definitionCache,
                        includeNestedBlocks: includeNestedBlocks,
                        skipXrefBlocks: skipXrefBlocks);

                    // Create instance (transform relative to THIS definition)
                    var nestedInst = new BHC.BlockInstance
                    {
                        Name = SafeBlockName(nestedBr.BlockName),
                        Position = nestedBr.Position.FromCivil3D(),
                        Rotation = nestedBr.Rotation, // radians (AutoCAD)
                        Scale = nestedBr.ScaleFactors.FromCivil3D()
                    };

                    bhDef.NestedBlocks.Add(nestedInst);
                    continue;
                }

                // Geometry (entities inside definition)
                if (dbo is Entity ent)
                {
                    IGeometry g = ent.FromCivil3D();

                    if (g != null)
                        bhDef.Geometry.Add(g);

                    // Unsupported entities are simply skipped (or log if you prefer)
                    continue;
                }
            }

            return bhDef;
        }

        // -------------------------
        // Helpers
        // -------------------------

        private static string SafeBlockName(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? "" : name;
        }
        
    }
}