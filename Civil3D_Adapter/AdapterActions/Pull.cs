/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using BH.oM.Adapter;
using BH.Adapter.Socket;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Base.Debugging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Adapter.Civil3D
{
    public partial class Civil3DAdapter : BHoMAdapter
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public override IEnumerable<object> Pull(IRequest request, PullType pullType = PullType.AdapterDefault, ActionConfig actionConfig = null)
        {
            //Reset the wait event
            m_waitEvent.Reset();

            if (!CheckConnection())
                return new List<object>();

            if (!(request is FilterRequest))
                return new List<object>();

            //Send data through the socket link
            m_linkIn.SendData(new List<object>() { PackageType.Pull, request as FilterRequest, actionConfig, Civil3DSettings });

            //Wait until the return message has been recieved
            if (!m_waitEvent.WaitOne(TimeSpan.FromMinutes(m_waitTime)))
                TimeOutError();

            //Grab the return objects from the latest package
            List<object> returnObjs = new List<object>(m_returnPackage);

            //Clear the return list
            m_returnPackage.Clear();

            //Raise returned events
            RaiseEvents();

            //Return the package
            return returnObjs;
        }

        /***************************************************/
    }
}
