using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Adapter;
using BH.oM.Adapters.Civil3D;
using BH.oM.Base;

namespace BH.Adapter.Robot
{
    public partial class Civil3DAdapter : BHoMAdapter
    {
        /***************************************************/
        /****           Private Fields                  ****/
        /***************************************************/

        public Civil3DConfig Civil3DConfig { get; set; } = new Civil3DConfig();

        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        public Civil3DAdapter(string filePath = "", Civil3DConfig civil3DConfig = null, bool Active = false)
        {
            if (Active)
            {

            }
        }

        protected override bool Create<T>(IEnumerable<T> objects, bool replaceAll = false)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<IBHoMObject> Read(Type type, IList ids)
        {
            throw new NotImplementedException();
        }
    }
}
