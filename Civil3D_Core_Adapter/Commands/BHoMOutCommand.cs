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

using BH.Adapter.File;
using BH.oM.Adapter;
using BH.oM.Data.Requests;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

//Commands are declared explicitly, so AutoCAD only scans the command classes for them
[assembly: CommandClass(typeof(BH.Civil3D.Adapter.Core.BHoMOutCommand))]

namespace BH.Civil3D.Adapter.Core
{
    //Civil 3D command that pulls every supported object from the active drawing and writes it to a JSON file with the
    //File adapter, without needing Grasshopper or a socket connection.
    public class BHoMOutCommand
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [CommandMethod("BHoMOut")]
        [Description("Civil 3D command BHOMOUT: pulls every supported object in the active drawing and writes it to a JSON file in a BHoMOut folder next to the drawing.")]
        public void BHoMOut()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor editor = doc?.Editor;

            //A command runs on the AutoCAD main thread, so an unexpected failure must not reach Civil 3D
            try
            {
                List<object> objects = new Civil3DListenerAdapter().Pull(new FilterRequest()).ToList();

                string filePath = OutputFilePath(doc);
                new FileAdapter(filePath).Push(objects, "", PushType.DeleteThenCreate);

                editor?.WriteMessage($"\nBHoM Out: wrote {objects.Count} object(s) to {filePath}");
            }
            catch (System.Exception ex)
            {
                BH.Engine.Base.Compute.RecordError("BHoM Out failed: " + ex.Message);
                editor?.WriteMessage("\nBHoM Out: failed, see the BHoM error log for details.");
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
    }
}