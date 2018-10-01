using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Adapters.Civil3D;

namespace BH.Engine.Adapters.Civil3D
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Civil3DConfig Civil3DConfig(int inPort = 14230, int returnPort = 14230, int maxMinToWait = 10)
        {
            if (inPort == returnPort)
            {
                Engine.Reflection.Compute.RecordError("Can not use the same in port as return port");
                return null;
            }

            if (inPort < 3001 || returnPort < 3001)
            {
                Engine.Reflection.Compute.RecordError("Can not use ports with number lower than 3000");
                return null;
            }

            return new Civil3DConfig { InPort = inPort, ReturnPort = returnPort, MaxMinutesToWait = maxMinToWait };
        }

        /***************************************************/
    }
}
