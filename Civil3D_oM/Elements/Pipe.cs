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

        public virtual ICurve CentreLine { get; set; } = new Polyline();

        public virtual double Diameter { get; set; } = 0;

        public virtual double Thickness { get; set; } = 0;

        public virtual FlowDirection FlowDirection { get; set; } = FlowDirection.Undefined;

        /***************************************************/
    }
}
