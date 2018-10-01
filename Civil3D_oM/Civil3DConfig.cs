using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;

namespace BH.oM.Adapters.Civil3D
{
    public class Civil3DConfig : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        public int InPort { get; set; } = 14230;
        public int ReturnPort { get; set; } = 14231;
        public int MaxMinutesToWait { get; set; } = 10;

        /***************************************************/
    }
}
