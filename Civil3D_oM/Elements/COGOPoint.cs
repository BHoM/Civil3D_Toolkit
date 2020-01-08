using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Civil.Elements
{
    public class CoGoPoint : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/
        public double Easting { get; set; } = 0;
        public double Elevation { get; set; } = 0;
        public string FullDescription { get; set; } = "";
        public double GridEasting { get; set; } = 0;
        public double GridNorthing { get; set; } = 0;
        public double Latitude { get; set; } = 0;
        public Point Location { get; set; } = new Point();
        public double Longitude { get; set; } = 0;
        public double Northing { get; set; } = 0;
        public string PointName { get; set; } = "";
        public double PointNumber { get; set; } = 0;
        public string RawDescription { get; set; } = "";

        /***************************************************/

    };
}
