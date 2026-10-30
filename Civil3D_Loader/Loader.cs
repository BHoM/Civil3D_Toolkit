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

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;

using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;

[assembly: ExtensionApplication(typeof(BH.Civil3D.Loader.Loader))]

namespace BH.Civil3D.Loader
{
    //The only DLL in the BHoM_Civil3D bundle. AutoCAD loads it from the bundle; it then loads the Core adapter of the same
    //year from C:\ProgramData\BHoM\Assemblies, as NETLOAD would, so the adapter finds the BHoM assemblies next to it.
    //It must not reference BHoM.
    public class Loader : IExtensionApplication
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Called by Civil 3D when the bundle loads: loads the BHoM Civil 3D listener from C:\\ProgramData\\BHoM\\Assemblies.")]
        public void Initialize()
        {
            //Civil3D_Loader_2026 loads Civil3D_Core_Adapter_2026
            string adapterName = Assembly.GetExecutingAssembly().GetName().Name.Replace("Civil3D_Loader", "Civil3D_Core_Adapter") + ".dll";
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BHoM", "Assemblies", adapterName);

            if (!File.Exists(path))
            {
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\nBHoM: {path} was not found, so the Civil 3D listener was not loaded. Install BHoM or build Civil3D_Toolkit.\n");
                return;
            }

            ExtensionLoader.Load(path);
        }

        /***************************************************/

        [Description("Called by Civil 3D when it closes. The Core adapter stops its own listener.")]
        public void Terminate()
        {
        }

        /***************************************************/
    }
}
