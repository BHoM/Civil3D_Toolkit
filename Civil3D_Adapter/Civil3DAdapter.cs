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

using BH.Adapter.Socket;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Base.Debugging;
using BH.oM.Adapters.Civil3D;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BH.Adapter.Civil3D
{
    public partial class Civil3DAdapter : BHoMAdapter
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        public Civil3DConfig Civil3DSettings { get; set; } = new Civil3DConfig();


        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        public Civil3DAdapter(Civil3DConfig civil3dSettings = null, bool active = false)
        {
            if (!active)
                return;

            if (civil3dSettings != null)
                Civil3DSettings = civil3dSettings;

            m_linkIn = new SocketLink_Tcp(Civil3DSettings.InPort);
            m_linkOut = new SocketLink_Tcp(Civil3DSettings.ReturnPort);
            m_linkOut.DataObservers += M_linkOut_DataObservers;

            m_waitEvent = new ManualResetEvent(false);
            m_returnPackage = new List<object>();
            m_returnEvents = new List<Event>();

            m_waitTime = Civil3DSettings.MaxMinutesToWait;
        }


        /***************************************************/
        /**** Public methods                            ****/
        /***************************************************/

        /***************************************************/
        /**** Private  Fields                           ****/
        /***************************************************/

        private SocketLink_Tcp m_linkIn;
        private SocketLink_Tcp m_linkOut;

        private ManualResetEvent m_waitEvent;
        private List<object> m_returnPackage;
        private List<Event> m_returnEvents;
        private double m_waitTime;


        /***************************************************/
        /**** Private  Methods                          ****/
        /***************************************************/

        private void M_linkOut_DataObservers(oM.Adapters.Socket.DataPackage package)
        {
            //Store the return data
            m_returnPackage = package.Data;

            //Store the events
            //m_returnEvents = package.Events; //ToDo: Work out if we need this

            //Set the wait event to allow methods to continue
            m_waitEvent.Set();
        }

        /***************************************************/

        private bool CheckConnection()
        {
            m_linkIn.SendData(new List<object> { PackageType.ConnectionCheck });

            bool returned = m_waitEvent.WaitOne(TimeSpan.FromSeconds(5));
            RaiseEvents();
            m_waitEvent.Reset();

            if (!returned)
                Engine.Base.Compute.RecordError("Failed to connect to Civil3D");

            return returned;
        }

        /***************************************************/

        private void TimeOutError()
        {
            Engine.Base.Compute.RecordError("The connection with Civil3D timed out. If working on a big model, try to increase the max wait time");
        }

        /***************************************************/

        private void RaiseEvents()
        {
            if (m_returnEvents == null)
                return;

            Engine.Base.Query.CurrentEvents().AddRange(m_returnEvents);
            Engine.Base.Query.AllEvents().AddRange(m_returnEvents);

            m_returnEvents = new List<Event>();
        }
    }
}


