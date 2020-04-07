using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Civils.Elements
{
    public class CivSurface : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        public virtual List<Polyline> Triangles { get; set; } = new List<Polyline>();

        /***************************************************/
    }
}
