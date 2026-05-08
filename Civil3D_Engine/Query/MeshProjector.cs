using System;
using System.Collections.Generic;
using BH.oM.Geometry;

namespace BH.Engine.Adapters.Civil3D
{
    /// <summary>
    /// Fast XY → Z projection onto a mesh using a uniform spatial grid.
    /// Designed for batch queries (e.g. Grasshopper, terrain sampling).
    /// </summary>
    internal class MeshXYProjector
    {
        private readonly Mesh _mesh;
        private readonly IList<Point> _vertices;
        private readonly IList<Face> _faces;

        private readonly double _cellSize;
        private readonly Dictionary<long, List<int>> _grid;

        public MeshXYProjector(Mesh mesh, double cellSize = 5.0)
        {
            if (mesh == null || mesh.Vertices == null || mesh.Faces == null)
                throw new ArgumentNullException("Mesh or mesh data is null.");

            _mesh = mesh;
            _vertices = mesh.Vertices;
            _faces = mesh.Faces;
            _cellSize = cellSize;

            _grid = BuildSpatialIndex();
        }

        #region Public API

        public Point PointOnMeshAtXY(double x, double y)
        {
            double bestZ = double.NegativeInfinity;
            bool found = false;

            long key = CellKey(x, y);

            List<int> faceIndices;
            if (!_grid.TryGetValue(key, out faceIndices))
                return null;

            for (int i = 0; i < faceIndices.Count; i++)
            {
                Face f = _faces[faceIndices[i]];

                if (f.A < 0 || f.B < 0 || f.C < 0)
                    continue;

                Point a = _vertices[f.A];
                Point b = _vertices[f.B];
                Point c = _vertices[f.C];

                // XY AABB reject (very cheap)
                if (!PointInTriangleBoundsXY(a, b, c, x, y))
                    continue;

                double z;
                if (TryIntersectTriangleAtXY(a, b, c, x, y, out z))
                {
                    if (z > bestZ)
                    {
                        bestZ = z;
                        found = true;
                    }
                }
            }

            if (!found)
                return null;

            return new Point { X = x, Y = y, Z = bestZ };
        }

        public IList<Point> PointsOnMeshAtXY(IList<Point> xyPoints)
        {
            if (xyPoints == null)
                return null;

            List<Point> result = new List<Point>(xyPoints.Count);

            for (int i = 0; i < xyPoints.Count; i++)
            {
                Point p = xyPoints[i];
                if (p == null)
                {
                    result.Add(null);
                    continue;
                }

                result.Add(PointOnMeshAtXY(p.X, p.Y));
            }

            return result;
        }

        #endregion

        #region Spatial index

        private Dictionary<long, List<int>> BuildSpatialIndex()
        {
            Dictionary<long, List<int>> grid = new Dictionary<long, List<int>>();

            for (int i = 0; i < _faces.Count; i++)
            {
                Face f = _faces[i];
                if (f.A < 0 || f.B < 0 || f.C < 0)
                    continue;

                Point a = _vertices[f.A];
                Point b = _vertices[f.B];
                Point c = _vertices[f.C];

                double minX = Math.Min(a.X, Math.Min(b.X, c.X));
                double minY = Math.Min(a.Y, Math.Min(b.Y, c.Y));
                double maxX = Math.Max(a.X, Math.Max(b.X, c.X));
                double maxY = Math.Max(a.Y, Math.Max(b.Y, c.Y));

                int ix0 = (int)Math.Floor(minX / _cellSize);
                int iy0 = (int)Math.Floor(minY / _cellSize);
                int ix1 = (int)Math.Floor(maxX / _cellSize);
                int iy1 = (int)Math.Floor(maxY / _cellSize);

                for (int ix = ix0; ix <= ix1; ix++)
                for (int iy = iy0; iy <= iy1; iy++)
                {
                    long key = CellKey(ix, iy);

                    List<int> list;
                    if (!grid.TryGetValue(key, out list))
                    {
                        list = new List<int>();
                        grid[key] = list;
                    }

                    list.Add(i);
                }
            }

            return grid;
        }

        private long CellKey(double x, double y)
        {
            int ix = (int)Math.Floor(x / _cellSize);
            int iy = (int)Math.Floor(y / _cellSize);
            return CellKey(ix, iy);
        }

        private static long CellKey(int ix, int iy)
        {
            return ((long)ix << 32) | (uint)iy;
        }

        #endregion

        #region Geometry helpers

        private static bool PointInTriangleBoundsXY(Point a, Point b, Point c, double x, double y)
        {
            double minX = Math.Min(a.X, Math.Min(b.X, c.X));
            double maxX = Math.Max(a.X, Math.Max(b.X, c.X));
            double minY = Math.Min(a.Y, Math.Min(b.Y, c.Y));
            double maxY = Math.Max(a.Y, Math.Max(b.Y, c.Y));

            return !(x < minX || x > maxX || y < minY || y > maxY);
        }

        private static bool TryIntersectTriangleAtXY(
            Point a, Point b, Point c,
            double x, double y,
            out double z)
        {
            z = 0;

            double det =
                (b.Y - c.Y) * (a.X - c.X) +
                (c.X - b.X) * (a.Y - c.Y);

            if (Math.Abs(det) < 1e-12)
                return false;

            double l1 =
                ((b.Y - c.Y) * (x - c.X) +
                 (c.X - b.X) * (y - c.Y)) / det;

            if (l1 < 0)
                return false;

            double l2 =
                ((c.Y - a.Y) * (x - c.X) +
                 (a.X - c.X) * (y - c.Y)) / det;

            if (l2 < 0)
                return false;

            double l3 = 1.0 - l1 - l2;
            if (l3 < 0)
                return false;

            z = l1 * a.Z + l2 * b.Z + l3 * c.Z;
            return true;
        }

        #endregion
    }
}