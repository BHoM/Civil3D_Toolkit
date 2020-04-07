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

        public virtual Point CentrePoint { get; set; } = new Point();

        public virtual double InternalLength { get; set; } = 0;

        public virtual double InternalWidth { get; set; } = 0;

        public virtual double InternalDepth { get; set; } = 0;

        public virtual double WallThickness { get; set; } = 0;

        public virtual double SurroundThickness { get; set; } = 0;

        public virtual double ChamberOrientation { get; set; } = 0;

        public virtual ChamberShape ChamberShape { get; set; } = ChamberShape.Undefined;

        public virtual double CoverLength { get; set; } = 0;

        public virtual double CoverWidth { get; set; } = 0;

        public virtual double CoverDepth { get; set; } = 0;

        public virtual double CoverOrientation { get; set; } = 0;

        public virtual double BeddingDepth { get; set; } = 0;

        /***************************************************/
    }
}
