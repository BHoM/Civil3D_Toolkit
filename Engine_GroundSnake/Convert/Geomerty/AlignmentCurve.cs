using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHG = BH.oM.Geometry;
using ACD = Autodesk.AutoCAD.DatabaseServices;
using ACG = Autodesk.AutoCAD.Geometry;

using ADC = Autodesk.Civil.DatabaseServices;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHG.Line FromCivil3D(ADC.AlignmentLine aLine)
        {
            return new oM.Geometry.Line
            {
                Start = aLine.StartPoint.FromCivil3D(),
                End = aLine.EndPoint.FromCivil3D(),
            };
        }

        public static BHG.ICurve FromCivil3D(object o)
        {
            return null;
        }
    }
}
