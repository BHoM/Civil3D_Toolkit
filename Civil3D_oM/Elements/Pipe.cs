using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Civils.Elements
{
    public class Pipe : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        public ICurve CentreLine { get; set; } = new Line();

        public double Diameter { get; set; } = 0;

        public double Thickness { get; set; } = 0;

        /***************************************************/
    }
}
