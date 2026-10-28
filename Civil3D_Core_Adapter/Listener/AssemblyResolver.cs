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

using System;
using System.IO;
using System.Reflection;

namespace BH.Civil3D.Adapter.Core
{
    //Lets Civil 3D find the BHoM assemblies in ProgramData, since the bundle only holds the Core DLLs.
    internal static class AssemblyResolver
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static void Register()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string name = new AssemblyName(args.Name).Name;
            string path = Path.Combine(m_BHoMFolder, name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        }


        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        private static readonly string m_BHoMFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BHoM", "Assemblies");

        /***************************************************/
    }
}
