using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Civils.Elements
{
    public class Alignment : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/
        public CivAlignmentType AlignmentType { get; set; } = CivAlignmentType.Undefined;
        public string Description { get; set; } = "";
        public double DesignSpeed { get; set; } = 0;
        public double EndingStation { get; set; } = 0;
        public double Length { get; set; } = 0;
        public string AlignmentName { get; set; } = "";
        public Point ReferencePoint { get; set; } = new Point();
        public double ReferencePointStation { get; set; } = 0;
        public double SiteID { get; set; } = 0;
        public string SiteName { get; set; } = "";
        public double StartingStation { get; set; } = 0;
        public double SuperelevationCriticalStations { get; set; } = 0;
        public ICurve SuperelevationCurves { get; set; } = new Polyline();

        /***************************************************/
    }
}
