using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHG = BH.oM.Geometry;

using ACG = Autodesk.AutoCAD.Geometry;

namespace BH.Engine.GroundSnake
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHG.Vector ToBHoM(this ACG.Vector3d vec)
        {
            return new BHG.Vector { X = vec.X, Y = vec.Y, Z = vec.Z };
        }

        /***************************************************/
    }
}
