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

using Autodesk.AutoCAD.Runtime;

using System.ComponentModel;
using System.Runtime.CompilerServices;

[assembly: ExtensionApplication(typeof(BH.Civil3D.Adapter.Core.Civil3DListener))]

namespace BH.Civil3D.Adapter.Core
{
    //Entry point loaded by Civil 3D. It has no BHoM types in its fields, base types or signatures, so it can
    //register the assembly resolver before any BHoM assembly is needed. The work is done in ListenerHost.
    public class Civil3DListener : IExtensionApplication
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Called by Civil 3D when the plug-in loads: registers the BHoM assembly resolver and starts the listener.")]
        public void Initialize()
        {
            AssemblyResolver.Register();
            Start();
        }

        /***************************************************/

        [Description("Called by Civil 3D when it closes: stops the listener.")]
        public void Terminate()
        {
            Stop();
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Start()
        {
            ListenerHost.Start();
        }

        /***************************************************/

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Stop()
        {
            ListenerHost.Stop();
        }

        /***************************************************/
    }
}
