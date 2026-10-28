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

using BH.oM.Base;
using System.ComponentModel;

namespace BH.oM.Adapters.Civil3D
{
    [Description("Connection settings for the Civil 3D adapter.")]
    public class Civil3DConfig : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        [Description("Port used to send data to Civil 3D. Must match the listener's input port.")]
        public virtual int InPort { get; set; } = 14230;

        [Description("Port used to receive data from Civil 3D. Must match the listener's output port.")]
        public virtual int ReturnPort { get; set; } = 14231;

        [Description("Maximum time, in minutes, to wait for Civil 3D to respond before timing out.")]
        public virtual int MaxMinutesToWait { get; set; } = 10;

        /***************************************************/
    }
}
