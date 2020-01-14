using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHG = BH.oM.Geometry;
using ACD = Autodesk.AutoCAD.DatabaseServices;
using ACG = Autodesk.AutoCAD.Geometry;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static ACD.Line ToCivil3D(this BHG.Line line)
        {
            return new Autodesk.AutoCAD.DatabaseServices.Line(line.Start.ToCivil3D(), line.End.ToCivil3D());
        }

        public static BHG.ICurve FromCivil3D(this ACD.Curve acCurve)
        {
            ACD.NurbsData nurbsData = acCurve.Spline.NurbsData;
            ACG.Point3dCollection ptCollection = nurbsData.GetControlPoints();

            if (ptCollection.Count == 2)
                return new BHG.Line { Start = ptCollection[0].FromCivil3D(), End = ptCollection[1].FromCivil3D() };

            List<BHG.Point> pts = new List<oM.Geometry.Point>();

            foreach (ACG.Point3d pt in ptCollection)
            {
                pts.Add(pt.FromCivil3D());
            }

            if (nurbsData.Degree == 1)
            {
                return new BHG.Polyline { ControlPoints = pts };
            }
            else
            {
                return new BHG.NurbsCurve { ControlPoints = pts, Knots = nurbsData.GetKnots().ToArray().ToList(), Weights = nurbsData.GetWeights().ToArray().ToList() };
            }
        }

        /***************************************************/

        public static BHG.ICurve FromCivil3D(this ACG.CircularArc3d acCircArc)
        {
            if (acCircArc.IsClosed())
            {
                return new BHG.Circle { Centre = acCircArc.Center.FromCivil3D(), Normal = acCircArc.Normal.FromCivil3D(), Radius = acCircArc.Radius };
            }
            else
            {
                BHG.CoordinateSystem.Cartesian system = BH.Engine.Geometry.Create.CartesianCoordinateSystem(acCircArc.Center.FromCivil3D(), acCircArc.ReferenceVector.FromCivil3D(), acCircArc.Normal.CrossProduct(acCircArc.ReferenceVector).FromCivil3D());

                return BH.Engine.Geometry.Create.Arc(system, acCircArc.Radius, acCircArc.StartAngle, acCircArc.EndAngle);
            }
        }

        /***************************************************/

        public static BHG.ICurve FromCivil3D(this ACG.CompositeCurve3d acCurve)
        {
            return new BHG.PolyCurve { Curves = acCurve.GetCurves().Select(x => x.FromCivil3D()).ToList() };
        }

        /***************************************************/

        public static BHG.ICurve FromCivil3D(this ACG.EllipticalArc3d acEllipse)
        {
            throw new NotImplementedException();
        }

        /***************************************************/

        public static BHG.Line FromCivil3D(this ACG.LinearEntity3d acLine)
        {
            return new BHG.Line { Start = acLine.StartPoint.FromCivil3D(), End = acLine.EndPoint.FromCivil3D() };
        }

        /***************************************************/

        public static BHG.Polyline FromCivil3D(this ACG.PolylineCurve3d acPolyLine)
        {

            List<BHG.Point> pts = new List<oM.Geometry.Point>();

            for (int i = 0; i < acPolyLine.NumberOfControlPoints; i++)
            {
                pts.Add(acPolyLine.ControlPointAt(i).FromCivil3D());
            }

            return new BHG.Polyline { ControlPoints = pts };
        }

        public static BHG.NurbsCurve FromCivil3D(this ACG.NurbCurve3d acNurbsCurve)
        {

            List<BHG.Point> pts = new List<oM.Geometry.Point>();

            for (int i = 0; i < acNurbsCurve.NumberOfControlPoints; i++)
            {
                pts.Add(acNurbsCurve.ControlPointAt(i).FromCivil3D());
            }

            List<double> knots = new List<double>();

            foreach (double d in acNurbsCurve.Knots)
            {
                knots.Add(d);
            }

            List<double> weights = new List<double>();

            for (int i = 0; i < acNurbsCurve.NumWeights; i++)
            {
                weights.Add(acNurbsCurve.GetWeightAt(i));
            }


            return new BHG.NurbsCurve { ControlPoints = pts, Knots = knots, Weights = weights  };
        }

        /***************************************************/

        /***************************************************/
        /**** Public Methods - Interface                ****/
        /***************************************************/

        public static BHG.ICurve FromCivil3D(this ACG.Curve3d acCurve)
        {
            return ToBHoM(acCurve as dynamic);
        }

        /***************************************************/
    }
}
