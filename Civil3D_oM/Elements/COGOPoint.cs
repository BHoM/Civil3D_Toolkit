using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.UI.Civil.Engine
{
    public class COGOPoints : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/
        public double easting { get; set; } = 0;
        public double elevation { get; set; } = 0;
        public string fullDescription { get; set; } = "";
        public double gridEasting { get; set; } = 0;
        public double gridNorthing { get; set; } = 0;
        public double latitude { get; set; } = 0;
        public double location { get; set; } = 0;
        public double longitude { get; set; } = 0;
        public double northing { get; set; } = 0;
        public Point point { get; set; } = new Point();
        public string pointName { get; set; } = "";
        public double pointNumber { get; set; } = 0;
        public double primaryPointGroupID { get; set; } = 0;
        public string rawDescription { get; set; } = "";

        /***************************************************/

    };
}
