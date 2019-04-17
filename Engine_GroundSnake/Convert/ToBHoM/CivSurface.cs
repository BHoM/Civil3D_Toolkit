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

        public static BHC.CivSurface ToBHoM(this ADC.TinSurface acSurface)
        {
            //Converting a Triangulated Irregular Network Surface (TinSurface)
            List<BHG.Polyline> polylines = new List<oM.Geometry.Polyline>();

            foreach (ADC.TinSurfaceTriangle triangle in acSurface.Triangles)
                polylines.Add(triangle.ToBHoM());

            return new BHC.CivSurface
            {
                Triangles = polylines,
            };
        }

        public static BHG.Polyline ToBHoM(this ADC.TinSurfaceTriangle triangle)
        {
            List<BHG.Point> pts = new List<oM.Geometry.Point>();
            pts.Add(triangle.Vertex1.Location.ToBHoM());
            pts.Add(triangle.Vertex2.Location.ToBHoM());
            pts.Add(triangle.Vertex3.Location.ToBHoM());
            return Geometry.Create.Polyline(pts);
        }

        public static BHC.CivSurface ToBHoM(this ADC.TinVolumeSurface acSurface)
        {
            List<BHG.Polyline> pLines = new List<oM.Geometry.Polyline>();
            
            return new BHC.CivSurface
            {
                Triangles = pLines,
            };
        }
    }
}
