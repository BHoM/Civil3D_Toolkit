using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHG = BH.oM.Geometry;
using ACD = Autodesk.AutoCAD.DatabaseServices;
using ACG = Autodesk.AutoCAD.Geometry;

namespace BH.Engine.GroundSnake
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHG.ICurve ToBHoM(this ACD.Curve acCurve)
        {
            ACD.NurbsData nurbsData = acCurve.Spline.NurbsData;
            ACG.Point3dCollection ptCollection = nurbsData.GetControlPoints();

            if (ptCollection.Count == 2)
                return new BHG.Line { Start = ptCollection[0].ToBHoM(), End = ptCollection[1].ToBHoM() };

            List<BHG.Point> pts = new List<oM.Geometry.Point>();

            foreach (ACG.Point3d pt in ptCollection)
            {
                pts.Add(pt.ToBHoM());
            }

            if (nurbsData.Degree == 1)
            {
                return new BHG.Polyline { ControlPoints = pts };
            }
            else
            {
                return new BHG.NurbCurve { ControlPoints = pts, Knots = nurbsData.GetKnots().ToArray().ToList(), Weights = nurbsData.GetWeights().ToArray().ToList() };
            }
        }

        /***************************************************/

        public static BHG.ICurve ToBHoM(this ACG.CircularArc3d acCircArc)
        {
            if (acCircArc.IsClosed())
            {
                return new BHG.Circle { Centre = acCircArc.Center.ToBHoM(), Normal = acCircArc.Normal.ToBHoM(), Radius = acCircArc.Radius };
            }
            else
            {
                BHG.CoordinateSystem system = BH.Engine.Geometry.Create.CoordinateSystem(acCircArc.Center.ToBHoM(), acCircArc.ReferenceVector.ToBHoM(), acCircArc.Normal.CrossProduct(acCircArc.ReferenceVector).ToBHoM());

                return BH.Engine.Geometry.Create.Arc(system, acCircArc.Radius, acCircArc.StartAngle, acCircArc.EndAngle);
            }
        }

        /***************************************************/

        public static BHG.ICurve ToBHoM(this ACG.CompositeCurve3d acCurve)
        {
            return new BHG.PolyCurve { Curves = acCurve.GetCurves().Select(x => x.IToBHoM()).ToList() };
        }

        /***************************************************/

        public static BHG.ICurve ToBHoM(this ACG.EllipticalArc3d acEllipse)
        {
            throw new NotImplementedException();
        }

        /***************************************************/

        public static BHG.Line ToBHoM(this ACG.LinearEntity3d acLine)
        {
            return new BHG.Line { Start = acLine.StartPoint.ToBHoM(), End = acLine.EndPoint.ToBHoM() };
        }

        /***************************************************/

        public static BHG.Polyline ToBHoM(this ACG.PolylineCurve3d acPolyLine)
        {

            List<BHG.Point> pts = new List<oM.Geometry.Point>();

            for (int i = 0; i < acPolyLine.NumberOfControlPoints; i++)
            {
                pts.Add(acPolyLine.ControlPointAt(i).ToBHoM());
            }

            return new BHG.Polyline { ControlPoints = pts };
        }

        public static BHG.NurbCurve ToBHoM(this ACG.NurbCurve3d acNurbsCurve)
        {

            List<BHG.Point> pts = new List<oM.Geometry.Point>();

            for (int i = 0; i < acNurbsCurve.NumberOfControlPoints; i++)
            {
                pts.Add(acNurbsCurve.ControlPointAt(i).ToBHoM());
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


            return new BHG.NurbCurve { ControlPoints = pts, Knots = knots, Weights = weights  };
        }

        /***************************************************/

        /***************************************************/
        /**** Public Methods - Interface                ****/
        /***************************************************/

        public static BHG.ICurve IToBHoM(this ACG.Curve3d acCurve)
        {
            return ToBHoM(acCurve as dynamic);
        }

        /***************************************************/
    }
}
