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
using BH.oM.Adapters.Civil3D;

using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Engine.Adapters.Civil3D
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Create a config to connect to Civil3D on the specific ports.")]
        [Input("inPort", "The incoming port for connection to Civil3D.")]
        [Input("returnPort", "The outgoing port for connection to Civil3D.")]
        [Input("maxMinToWait", "Set the maximum wait time in minutes for a connection to be made.")]
        [Output("config", "The connection configuration for Civil3D.")]
        public static Civil3DConfig Civil3DConfig(int inPort = 14230, int returnPort = 14231, int maxMinToWait = 10)
        {
            if (inPort == returnPort)
            {
                Engine.Base.Compute.RecordError("Can not use the same in port as return port");
                return null;
            }

            if (inPort < 3001 || returnPort < 3001)
            {
                Engine.Base.Compute.RecordError("Can not use ports with number lower than 3000");
                return null;
            }

            return new Civil3DConfig { InPort = inPort, ReturnPort = returnPort, MaxMinutesToWait = maxMinToWait };
        }

        /***************************************************/
    }
}


