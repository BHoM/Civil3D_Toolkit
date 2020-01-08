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
        public double Area { get; set; } = 0;
        public Point AreaLocation { get; set; } = new Point();
        public Point Centroid { get; set; } = new Point();
        public string Description { get; set; } = "";
        public int Number { get; set; } = 0;

        /***************************************************/
    }
}
