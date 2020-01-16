using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Geometry;
using BHG = BH.oM.Geometry;
using ACD = Autodesk.AutoCAD.DatabaseServices;
using ACG = Autodesk.AutoCAD.Geometry;

using ADC = Autodesk.Civil.DatabaseServices;

using BHC = BH.oM.Civils.Elements;
using BH.Engine.Geometry;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHC.AlignmentCurve FromCivil3D(ADC.AlignmentLine aLine)
        {
            BHG.Line line = new oM.Geometry.Line
            {
                Start = aLine.StartPoint.FromCivil3D(),
                End = aLine.EndPoint.FromCivil3D(),
            };

            return new BHC.AlignmentCurve
            {
                Curve = line,
            };
        }

        public static BHC.AlignmentCurve FromCivil3D(ADC.AlignmentArc aArc)
        {
            return new BHC.AlignmentCurve
            {
                Curve = BH.Engine.Geometry.Create.ArcByCentre(aArc.CenterPoint.FromCivil3D(), aArc.StartPoint.FromCivil3D(), aArc.EndPoint.FromCivil3D()),
            };
        }

        public static BHC.AlignmentCurve FromCivil3D(object o)
        {
            return null;
        }
    }
}
