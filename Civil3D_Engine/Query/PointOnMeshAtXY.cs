using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BH.oM.Analytical.Elements;
using BH.oM.Analytical.Graph;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Civils.Elements;
using BH.oM.Geometry;
using BH.Engine.Geometry;


namespace BH.Engine.Adapters.Civil3D
{
    public static partial class Query
    {

        [Description("Finds points on a mesh by vertically projecting from XY coordinates of a list of points.")]
        [Input("mesh", "Mesh to query.")]
        [Input("xyPoints", "List of points to project from XY to get Z values.")]
        [Output("points", "Points on the mesh.")]
        public static List<Point> PointsOnMeshAtXY(
            this Mesh mesh,
            List<Point> xyPoints)
        {
            var projector = new MeshXYProjector(mesh);
            return new List<Point>(projector.PointsOnMeshAtXY(xyPoints));
        }

    }
}