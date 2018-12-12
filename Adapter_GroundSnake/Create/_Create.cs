using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;


namespace BH.UI.GroundSnake.Adapter
{
    public partial class GroundSnakeAdapter
    {

        /***************************************************/
        /****           Adapter Methods                 ****/
        /***************************************************/

        //General method called by the adapter for push
        protected override bool Create<T>(IEnumerable<T> objects, bool replaceAll = false)
        {
            return CreateCollection(objects as dynamic);
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private bool CreateCollection(IEnumerable<BH.oM.Civils.Elements.Pipe> pipes)
        {
            //Add code here for creating pipes in Civil3d.
            //Try to keep the converts in the Engine if possible. See the ToCivil3D files.
            throw new NotImplementedException();
        }


        /***************************************************/


    }
}
