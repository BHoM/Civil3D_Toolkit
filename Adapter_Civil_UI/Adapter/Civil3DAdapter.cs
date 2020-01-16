using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Adapter;
using BH.oM.Base;

namespace BH.UI.Civil.Adapter
{
    public partial class GroundSnakeAdapter : BHoMAdapter
    {
        /***************************************************/
        /****           Private Fields                  ****/
        /***************************************************/


        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        public GroundSnakeAdapter()
        {
            Config.UseAdapterId = false; //To be switched to true at later stage;
            
        }



    }
}
