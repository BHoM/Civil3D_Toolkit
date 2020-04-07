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

        public virtual CivAlignmentType AlignmentType { get; set; } = CivAlignmentType.Undefined;
        public virtual string Description { get; set; } = "";
        public virtual List<double> DesignSpeeds { get; set; } = new List<double>();
        public virtual double EndingStation { get; set; } = 0;
        public virtual double Length { get; set; } = 0;
        public virtual Point ReferencePoint { get; set; } = new Point();
        public virtual double ReferencePointStation { get; set; } = 0;
        public virtual string SiteName { get; set; } = "";
        public virtual double StartingStation { get; set; } = 0;

        public virtual List<AlignmentCurve> AlignmentCurves { get; set; } = new List<AlignmentCurve>();

        /***************************************************/
    }
}
