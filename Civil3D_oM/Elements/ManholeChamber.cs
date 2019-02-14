using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Civils.Elements
{
    public class ManholeChamber : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        public Point CentrePoint { get; set; } = new Point();

        public double InternalLength { get; set; } = 0;

        public double InternalWidth { get; set; } = 0;

        public double InternalDepth { get; set; } = 0;

        public double WallThickness { get; set; } = 0;

        public double SurroundThickness { get; set; } = 0;

        public double ChamberOrientation { get; set; } = 0;

        public ChamberShape ChamberShape { get; set; } = ChamberShape.Undefined;

        public double CoverLength { get; set; } = 0;

        public double CoverWidth { get; set; } = 0;

        public double CoverDepth { get; set; } = 0;

        public double CoverOrientation { get; set; } = 0;

        public double BeddingDepth { get; set; } = 0;

        /***************************************************/
    }
}
