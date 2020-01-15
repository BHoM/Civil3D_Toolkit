using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Civils.Elements
{
    public class Block : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        public string BlockName { get; set; } = "";
        public BoundingBox Bounds { get; set; } = new BoundingBox();
        public bool CanCastShadow { get; set; } = false;
        public CollisionType CollisionType { get; set; } = CollisionType.Undefined;
        public bool IsPersistent { get; set; } = false;
        public bool IsPlanar { get; set; } = false;
        public string Layer { get; set; } = "";
        public string Material { get; set; } = "";
        public Vector Normal { get; set; } = new Vector();
        public Point Position { get; set; } = new Point();
        public bool CanReceiveShadow { get; set; } = false;
        public double Rotation { get; set; } = 0;
        public bool IsVisible { get; set; } = false;

        /***************************************************/
    }
}
