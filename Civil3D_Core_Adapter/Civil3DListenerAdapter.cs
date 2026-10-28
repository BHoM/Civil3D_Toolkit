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

using BH.Adapter;
using BH.oM.Adapter;

using System.ComponentModel;

namespace BH.Civil3D.Adapter.Core
{
    //Adapter that runs inside Civil 3D; the listener passes it the packages sent by the BHoM client adapter.
    public partial class Civil3DListenerAdapter : BHoMAdapter
    {
        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        [Description("Adapter that runs inside Civil 3D and reads and writes the active drawing. It is created by the BHoM listener; use Civil3DAdapter from Grasshopper or Excel.")]
        public Civil3DListenerAdapter()
        {
            m_AdapterSettings.DefaultPushType = PushType.CreateOnly;
            m_AdapterSettings.UseAdapterId = false;
            m_AdapterSettings.WrapNonBHoMObjects = true;
        }

        /***************************************************/
    }
}
