using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHG = BH.oM.Geometry;
using BHC = BH.oM.Civils.Elements;
using ADC = Autodesk.Civil.DatabaseServices;

namespace BH.Engine.GroundSnake
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHC.Surface ToBHoM(this ADC.TinSurface acTinSurface)
        {
            List<BHG.Polyline> polylines = new List<oM.Geometry.Polyline>();

            foreach (ADC.TinSurfaceTriangle triangle in acTinSurface.Triangles)
            {

                List<BHG.Point> points = new List<BHG.Point> { ToBHoM(triangle.Vertex1.Location),
                                                               ToBHoM(triangle.Vertex2.Location),
                                                               ToBHoM(triangle.Vertex3.Location)};

                polylines.Add(Geometry.Create.Polyline(points));
            }

            return new BHC.Surface
            {
                Triangles = polylines
            };
        }
    }
}
