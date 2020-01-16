using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHC = BH.oM.Civils.Elements;

using ACG = Autodesk.AutoCAD.Geometry;

using ADC = Autodesk.Civil.DatabaseServices;

using Autodesk.AutoCAD.DatabaseServices;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHC.CollisionType FromCivil3D(this CollisionType collisionType)
        {
            switch (collisionType)
            {
                case CollisionType.None:
                    return BHC.CollisionType.None;
                case CollisionType.Solid:
                    return BHC.CollisionType.Solid;
                default:
                    return BHC.CollisionType.Undefined;
            }
        }

        /***************************************************/
    }
}
