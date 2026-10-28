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

using BH.oM.Adapters.Civil3D;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Civil3D
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates the connection settings for the Civil 3D adapter, using the given ports.")]
        [Input("inPort", "Port used to send data to Civil 3D. Must match the listener's input port.")]
        [Input("returnPort", "Port used to receive data from Civil 3D. Must match the listener's output port.")]
        [Input("maxMinToWait", "Maximum time, in minutes, to wait for Civil 3D to respond before timing out.")]
        [Output("config", "Connection settings for the Civil 3D adapter.")]
        public static Civil3DConfig Civil3DConfig(int inPort = 14230, int returnPort = 14231, int maxMinToWait = 10)
        {
            if (inPort == returnPort)
            {
                BH.Engine.Base.Compute.RecordError("Cannot use the same port for input and return.");
                return null;
            }

            if (inPort < 3001 || returnPort < 3001)
            {
                BH.Engine.Base.Compute.RecordError("Ports must be greater than 3000.");
                return null;
            }

            return new Civil3DConfig { InPort = inPort, ReturnPort = returnPort, MaxMinutesToWait = maxMinToWait };
        }

        /***************************************************/
    }
}
