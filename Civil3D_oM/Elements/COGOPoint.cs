using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Civils.Elements
{
    public class CoGoPoint : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/
        public virtual double Easting { get; set; } = 0;
        public virtual double Elevation { get; set; } = 0;
        public virtual string FullDescription { get; set; } = "";
        public virtual double GridEasting { get; set; } = 0;
        public virtual double GridNorthing { get; set; } = 0;
        public virtual double Latitude { get; set; } = 0;
        public virtual Point Location { get; set; } = new Point();
        public virtual double Longitude { get; set; } = 0;
        public virtual double Northing { get; set; } = 0;
        public virtual string PointName { get; set; } = "";
        public virtual double PointNumber { get; set; } = 0;
        public virtual string RawDescription { get; set; } = "";

        /***************************************************/

    };
}
