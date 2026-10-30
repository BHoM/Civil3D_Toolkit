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

using BH.Engine.Serialiser;
using BH.oM.Adapter;
using BHC = BH.oM.Adapters.Civil3D.Elements;
using BH.oM.Data.Requests;
using BHG = BH.oM.Geometry;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

//Commands are declared explicitly, so AutoCAD only scans the command classes for them
[assembly: CommandClass(typeof(BH.Civil3D.Adapter.Core.BHoMOutCommand))]

namespace BH.Civil3D.Adapter.Core
{
    //Standalone AutoCAD command that extracts every BHoM data type supported by the
    //Civil3D toolkit's Read methods from the active drawing and writes it to a JSON file,
    //without needing a Grasshopper/Dynamo definition or a socket link to be open.
    public class BHoMOutCommand
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [CommandMethod("BHoMOut")]
        [Description("Civil 3D command BHOMOUT: writes every supported object in the active drawing to a JSON file in a BHoMOut folder next to the drawing.")]
        public void BHoMOut()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor editor = doc?.Editor;

            try
            {
                Civil3DListenerAdapter adapter = new Civil3DListenerAdapter();
                List<object> extracted = new List<object>();

                foreach (Type type in m_SupportedTypes)
                {
                    FilterRequest request = new FilterRequest { Type = type };
                    IEnumerable<object> pulled = adapter.Pull(request, PullType.AdapterDefault);
                    if (pulled != null)
                        extracted.AddRange(pulled);
                }

                string filePath = OutputFilePath(doc);
                File.WriteAllText(filePath, extracted.ToJsonArray());

                editor?.WriteMessage($"\nBHoM Out: extracted {extracted.Count} object(s) to {filePath}");
            }
            catch (System.Exception ex)
            {
                BH.Engine.Base.Compute.RecordError("BHoM Out failed: " + ex.Message);
                editor?.WriteMessage("\nBHoM Out: failed, see BHoM error log for details.");
            }
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        //Writes into a "BHoMOut" subfolder next to the drawing file, so the output always
        //sits alongside it and the command never needs a path provided.
        private string OutputFilePath(Document doc)
        {
            string drawingPath = doc?.Name;
            string drawingDir = string.IsNullOrWhiteSpace(drawingPath) ? Directory.GetCurrentDirectory() : Path.GetDirectoryName(drawingPath);
            string folder = Path.Combine(drawingDir, "BHoMOut");
            Directory.CreateDirectory(folder);

            string drawingName = string.IsNullOrWhiteSpace(drawingPath) ? "Civil3DOut" : Path.GetFileNameWithoutExtension(drawingPath);
            return Path.Combine(folder, drawingName + ".json");
        }


        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        //The BHoM types currently wired up in the Civil3D toolkit's Read methods.
        private static readonly Type[] m_SupportedTypes = new Type[]
        {
            typeof(BHC.TextNote),
            typeof(BHG.Point),
            typeof(BHG.ICurve),
            typeof(BHC.Block),
            typeof(BHC.BlockDefinition),
            typeof(BHC.SurveyPoint)
        };

        /***************************************************/
    }
}
