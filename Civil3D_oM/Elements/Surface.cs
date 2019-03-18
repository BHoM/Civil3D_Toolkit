using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Civils.Elements
{
    public class Surface : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        public List<Polyline> Triangles { get; set; } = new List<Polyline>();

        public double MaximumGradeOrSlope { get; set; } = 0;

        public double MeanGradeOrSlope { get; set; } = 0;

        public double MinimumGradeOrSlope { get; set; } = 0;

        public double SurfaceArea2D { get; set; } = 0;

        public double SurfaceArea3D { get; set; } = 0;

        /***************************************************/
    }
}
