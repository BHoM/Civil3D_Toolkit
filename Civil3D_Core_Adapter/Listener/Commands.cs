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

[assembly: CommandClass(typeof(BH.Civil3D.Adapter.Core.ListenerCommands))]

namespace BH.Civil3D.Adapter.Core
{
    //Civil 3D commands to start and stop the BHoM listener, like Revit's Activate and Deactivate buttons.
    public class ListenerCommands
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Civil 3D command BHOMACTIVATE: starts the BHoM listener on ports 14230/14231, or restarts it if it is running.")]
        [CommandMethod("BHOMACTIVATE")]
        public void Activate()
        {
            Civil3DListener.Activate();
        }

        /***************************************************/

        [Description("Civil 3D command BHOMDEACTIVATE: stops the BHoM listener until BHOMACTIVATE is run.")]
        [CommandMethod("BHOMDEACTIVATE")]
        public void Deactivate()
        {
            Civil3DListener.Deactivate();
        }

        /***************************************************/
    }
}