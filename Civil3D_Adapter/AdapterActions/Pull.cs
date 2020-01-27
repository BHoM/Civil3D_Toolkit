using BH.oM.Adapter;
using BH.Adapter.Socket;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Debugging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BH.oM.Reflection.Attributes;
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
