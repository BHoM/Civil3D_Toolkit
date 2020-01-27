using BH.Adapter.Socket;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Debugging;
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

        private void M_linkOut_DataObservers(oM.Socket.DataPackage package)
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
                Engine.Reflection.Compute.RecordError("Failed to connect to Civil3D");

            return returned;
        }

        /***************************************************/

        private void TimeOutError()
        {
            Engine.Reflection.Compute.RecordError("The connection with Civil3D timed out. If working on a big model, try to increase the max wait time");
        }

        /***************************************************/

        private void RaiseEvents()
        {
            if (m_returnEvents == null)
                return;

            Engine.Reflection.Query.CurrentEvents().AddRange(m_returnEvents);
            Engine.Reflection.Query.AllEvents().AddRange(m_returnEvents);

            m_returnEvents = new List<Event>();
        }
    }
}
