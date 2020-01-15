using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHG = BH.oM.Geometry;

using ACG = Autodesk.AutoCAD.Geometry;

using Autodesk.AutoCAD.DatabaseServices;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHG.BoundingBox FromCivil3D(this Extents3d extents)
        {
            return new oM.Geometry.BoundingBox
            {
                Max = extents.MaxPoint.FromCivil3D(),
                Min = extents.MinPoint .FromCivil3D(),
            };
        }
    }
}
