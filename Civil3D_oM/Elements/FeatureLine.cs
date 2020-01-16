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
    public class FeatureLine : BHoMObject
    {
        public string Description { get; set; } = "";
        public double Length2D { get; set; } = 0;
        public double Length3D { get; set; } = 0;
        public double MaxElevation { get; set; } = 0;
        public double MaxGrade { get; set; } = 0;
        public double MinElevation { get; set; } = 0;
        public double MinGrade { get; set; } = 0;

        /***************************************************/
    }
}
