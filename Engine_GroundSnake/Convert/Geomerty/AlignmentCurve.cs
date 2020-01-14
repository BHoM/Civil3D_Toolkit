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
            /* List<Point> controlpoints = new List<Point>();
             controlpoints.Add(aArc.StartPoint.FromCivil3D());
             controlpoints.Add(aArc.CenterPoint.FromCivil3D());
             controlpoints.Add(aArc.EndPoint.FromCivil3D()); */

            /*Point origin = new BHG.Point { X = 0, Y = 0, Z = 0 };
            Point axis = new BHG.Point { X = 10, Y = 0, Z = 0 };

            // Vector v = new BHG.Vector { X = aArc.CenterPoint.FromCivil3D().X, Y = aArc.CenterPoint.FromCivil3D().Y, Z = aArc.CenterPoint.FromCivil3D().Z };

            //Cartesian system = Create.CartesianCoordinateSystem(aArc.CenterPoint.FromCivil3D(),v , v);
            BH.oM.Geometry.CoordinateSystem.Cartesian cs = BH.Engine.Geometry.Create.CartesianCoordinateSystem(aArc.CenterPoint.FromCivil3D(), v, v);

            double startAngle = BH.Engine.Geometry.Query.Angle(aArc.StartPoint.FromCivil3D(), origin, axis);
            double endAngle = BH.Engine.Geometry.Query.Angle(aArc.EndPoint.FromCivil3D(), origin, axis);
            double radius = aArc.CenterPoint.FromCivil3D().Distance(aArc.StartPoint.FromCivil3D());

            double t1 =  aArc.StartDirection;
            double t2 = aArc.EndDirection;

           // BHG.Arc arc = new Arc { CoordinateSystem = cs, Radius = radius, StartAngle = t2, EndAngle = t1 };
            

            //BHG.Polyline pl = new Polyline { ControlPoints = controlpoints };

            return new BHC.AlignmentCurve
            {
                Curve = arc,
            };*/

            Arc a = BH.Engine.Geometry.Create.Arc(aArc.StartPoint.FromCivil3D(), aArc.PassThroughPoint2.FromCivil3D(), aArc.EndPoint.FromCivil3D());
            return new BHC.AlignmentCurve
            {
                Curve = a,
            };
        }


        public static BHC.AlignmentCurve FromCivil3D(object o)
        {
            return null;
        }
    }
}
