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

        public virtual string BlockName { get; set; } = "";
        public virtual BoundingBox Bounds { get; set; } = new BoundingBox();
        public virtual bool CanCastShadow { get; set; } = false;
        public virtual CollisionType CollisionType { get; set; } = CollisionType.Undefined;
        public virtual bool IsPersistent { get; set; } = false;
        public virtual bool IsPlanar { get; set; } = false;
        public virtual string Layer { get; set; } = "";
        public virtual string Material { get; set; } = "";
        public virtual Vector Normal { get; set; } = new Vector();
        public virtual Point Position { get; set; } = new Point();
        public virtual bool CanReceiveShadow { get; set; } = false;
        public virtual double Rotation { get; set; } = 0;
        public virtual bool IsVisible { get; set; } = false;

        /***************************************************/
    }
}
