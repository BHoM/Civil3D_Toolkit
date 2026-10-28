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

using BH.oM.Adapter;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Data.Requests;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace BH.Adapter.Civil3D
{
    public partial class Civil3DAdapter : BHoMAdapter
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public override IEnumerable<object> Pull(IRequest request, PullType pullType = PullType.AdapterDefault, ActionConfig actionConfig = null)
        {
            if (!IsActive())
                return new List<object>();

            FilterRequest filterRequest = request as FilterRequest;
            if (filterRequest == null)
            {
                Engine.Base.Compute.RecordError("Civil 3D only supports FilterRequest.");
                return new List<object>();
            }

            if (filterRequest.Type == null)
            {
                Engine.Base.Compute.RecordError("The FilterRequest has no Type. Set the type of object to pull from Civil 3D.");
                return new List<object>();
            }

            //Reset the wait event
            m_waitEvent.Reset();

            if (!CheckConnection())
                return new List<object>();

            //Send data through the socket link
            m_linkIn.SendData(new List<object>() { PackageType.Pull, filterRequest, actionConfig, Civil3DSettings });

            //Wait until the return message has been recieved
            if (!m_waitEvent.WaitOne(TimeSpan.FromMinutes(m_waitTime)))
                TimeOutError();

            //Grab the return objects from the latest package
            List<object> returnObjs = new List<object>(m_returnPackage);

            //Clear the return list
            m_returnPackage.Clear();

            //Raise returned events
            RaiseEvents();

            //A single string returned is an error message from Civil 3D
            if (returnObjs.Count == 1 && returnObjs[0] is string)
            {
                Engine.Base.Compute.RecordError(returnObjs[0] as string);
                return new List<object>();
            }

            //Return the package
            return returnObjs;
        }

        /***************************************************/
    }
}
