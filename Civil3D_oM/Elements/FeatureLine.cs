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
        public virtual string Description { get; set; } = "";
        public virtual double Length2D { get; set; } = 0;
        public virtual double Length3D { get; set; } = 0;
        public virtual double MaxElevation { get; set; } = 0;
        public virtual double MaxGrade { get; set; } = 0;
        public virtual double MinElevation { get; set; } = 0;
        public virtual double MinGrade { get; set; } = 0;

        /***************************************************/
    }
}
