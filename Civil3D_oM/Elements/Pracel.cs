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
    public class Pracel : BHoMObject
    {
        public Point AreaLocation { get; set; } = new Point();
        public Point Centroid { get; set; } = new Point();
        public string Desctiption { get; set; } = "";
        public string ParcelName { get; set; } = "";
        public int Number { get; set; } = 0;

        /***************************************************/
    }
}
