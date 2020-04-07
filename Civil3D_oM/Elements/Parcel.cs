using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Civils.Elements
{
    /***************************************************/
    /**** Public Properties                         ****/
    /***************************************************/
    public class Parcel : BHoMObject
    {
        public virtual double Area { get; set; } = 0;
        public virtual Point AreaLocation { get; set; } = new Point();
        public virtual Point Centroid { get; set; } = new Point();
        public virtual string Description { get; set; } = "";
        public virtual int Number { get; set; } = 0;

        /***************************************************/
    }
}
