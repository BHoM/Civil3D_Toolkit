using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Adapter;
using BH.oM.Base;

namespace BH.Adapter.Civil3D
{
    public partial class Civil3DAdapter : BHoMAdapter
    {
        public override List<object> Push(IEnumerable<object> objects, string tag = "", PushType pushType = PushType.AdapterDefault, ActionConfig actionConfig = null)
        {
            // ----------------------------------------//
            //                 SET-UP                  //
            // ----------------------------------------//

            // If unset, set the pushType to AdapterSettings' value (base AdapterSettings default is FullCRUD).
            if (pushType == PushType.AdapterDefault)
                pushType = m_AdapterSettings.DefaultPushType;

            // Process the objects (verify they are valid; DeepClone them, wrap them, etc).
            IEnumerable<IBHoMObject> objectsToPush = ProcessObjectsForPush(objects, actionConfig); // Note: default Push only supports IBHoMObjects.

            if (objectsToPush.Count() == 0)
            {
                Engine.Base.Compute.RecordError("Input objects were invalid.");
                return new List<object>();
            }

            // ----------------------------------------//
            //               ACTUAL PUSH               //
            // ----------------------------------------//

            //Reset the wait event
            m_waitEvent.Reset();

            if (!CheckConnection())
                return new List<object>();

            //Send data through the socket link
            m_linkIn.SendData(new List<object>() { PackageType.Push, objects.ToList(), actionConfig, Civil3DSettings }, tag);

            //Wait until the return message has been recieved
            if (!m_waitEvent.WaitOne(TimeSpan.FromMinutes(m_waitTime)))
                TimeOutError();

            //Grab the return objects from the latest package
            List<object> returnObjs = m_returnPackage.ToList();

            //Clear the return list
            m_returnPackage.Clear();

            RaiseEvents();

            //Return the package
            return returnObjs;
        }
    }
}
