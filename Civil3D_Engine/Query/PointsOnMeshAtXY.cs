/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using BH.oM.Quantities.Attributes;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Engine.Adapters.Civil3D
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Projects points vertically onto a mesh. For each input point, finds the highest mesh face directly above or below it in plan and returns the point on that face. Points outside the mesh in plan return null.")]
        [Input("mesh", "Mesh to project the points onto.")]
        [Input("xyPoints", "Points whose X and Y are used. Their Z is ignored.")]
        [Input("cellSize", "Size of the spatial grid used to speed up the search, in metres.", typeof(Length))]
        [Output("points", "Projected points, in the same order as the inputs. Null where no face is found.")]
        public static List<Point> PointsOnMeshAtXY(this Mesh mesh, List<Point> xyPoints, double cellSize = 5.0)
        {
            if (mesh == null || mesh.Vertices == null || mesh.Faces == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot project points onto a null mesh or a mesh with no vertices or faces.");
                return new List<Point>();
            }

            if (xyPoints == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot project a null list of points onto a mesh.");
                return new List<Point>();
            }

            if (cellSize <= 0)
            {
                BH.Engine.Base.Compute.RecordError("The cell size must be greater than zero.");
                return new List<Point>();
            }

            Dictionary<long, List<int>> grid = BuildSpatialIndex(mesh, cellSize);

            List<Point> result = new List<Point>(xyPoints.Count);
            foreach (Point p in xyPoints)
                result.Add(p == null ? null : PointOnMeshAtXY(mesh, grid, cellSize, p.X, p.Y));

            return result;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static Point PointOnMeshAtXY(Mesh mesh, Dictionary<long, List<int>> grid, double cellSize, double x, double y)
        {
            List<int> faceIndices;
            if (!grid.TryGetValue(CellKey(x, y, cellSize), out faceIndices))
                return null;

            double bestZ = double.NegativeInfinity;
            bool found = false;

            foreach (int faceIndex in faceIndices)
            {
                foreach (Point[] triangle in Triangles(mesh, mesh.Faces[faceIndex]))
                {
                    if (!InTriangleBoundsXY(triangle, x, y))
                        continue;

                    double z;
                    if (TryIntersectTriangleAtXY(triangle, x, y, out z) && z > bestZ)
                    {
                        bestZ = z;
                        found = true;
                    }
                }
            }

            return found ? new Point { X = x, Y = y, Z = bestZ } : null;
        }

        /***************************************************/

        private static Dictionary<long, List<int>> BuildSpatialIndex(Mesh mesh, double cellSize)
        {
            Dictionary<long, List<int>> grid = new Dictionary<long, List<int>>();

            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                foreach (Point[] triangle in Triangles(mesh, mesh.Faces[i]))
                {
                    int ix0 = (int)Math.Floor(Math.Min(triangle[0].X, Math.Min(triangle[1].X, triangle[2].X)) / cellSize);
                    int iy0 = (int)Math.Floor(Math.Min(triangle[0].Y, Math.Min(triangle[1].Y, triangle[2].Y)) / cellSize);
                    int ix1 = (int)Math.Floor(Math.Max(triangle[0].X, Math.Max(triangle[1].X, triangle[2].X)) / cellSize);
                    int iy1 = (int)Math.Floor(Math.Max(triangle[0].Y, Math.Max(triangle[1].Y, triangle[2].Y)) / cellSize);

                    for (int ix = ix0; ix <= ix1; ix++)
                    {
                        for (int iy = iy0; iy <= iy1; iy++)
                        {
                            long key = CellKey(ix, iy);

                            List<int> list;
                            if (!grid.TryGetValue(key, out list))
                            {
                                list = new List<int>();
                                grid[key] = list;
                            }

                            //A quad adds its index once per triangle; consecutive duplicates are skipped
                            if (list.Count == 0 || list[list.Count - 1] != i)
                                list.Add(i);
                        }
                    }
                }
            }

            return grid;
        }

        /***************************************************/

        //Returns the face as one triangle, or as two (A-B-C and A-C-D) when it is a quad.
        private static List<Point[]> Triangles(Mesh mesh, Face face)
        {
            List<Point[]> triangles = new List<Point[]>();
            int count = mesh.Vertices.Count;

            if (!IsValidIndex(face.A, count) || !IsValidIndex(face.B, count) || !IsValidIndex(face.C, count))
                return triangles;

            triangles.Add(new Point[] { mesh.Vertices[face.A], mesh.Vertices[face.B], mesh.Vertices[face.C] });

            if (IsValidIndex(face.D, count))
                triangles.Add(new Point[] { mesh.Vertices[face.A], mesh.Vertices[face.C], mesh.Vertices[face.D] });

            return triangles;
        }

        /***************************************************/

        private static bool IsValidIndex(int index, int count)
        {
            return index >= 0 && index < count;
        }

        /***************************************************/

        private static long CellKey(double x, double y, double cellSize)
        {
            return CellKey((int)Math.Floor(x / cellSize), (int)Math.Floor(y / cellSize));
        }

        /***************************************************/

        private static long CellKey(int ix, int iy)
        {
            return ((long)ix << 32) | (uint)iy;
        }

        /***************************************************/

        private static bool InTriangleBoundsXY(Point[] t, double x, double y)
        {
            return x >= Math.Min(t[0].X, Math.Min(t[1].X, t[2].X)) && x <= Math.Max(t[0].X, Math.Max(t[1].X, t[2].X))
                && y >= Math.Min(t[0].Y, Math.Min(t[1].Y, t[2].Y)) && y <= Math.Max(t[0].Y, Math.Max(t[1].Y, t[2].Y));
        }

        /***************************************************/

        //Barycentric coordinates of (x, y) in the plan projection of the triangle; Z is interpolated from them.
        private static bool TryIntersectTriangleAtXY(Point[] t, double x, double y, out double z)
        {
            z = 0;
            Point a = t[0], b = t[1], c = t[2];

            double det = (b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y);
            if (Math.Abs(det) < 1e-12)
                return false;

            double l1 = ((b.Y - c.Y) * (x - c.X) + (c.X - b.X) * (y - c.Y)) / det;
            double l2 = ((c.Y - a.Y) * (x - c.X) + (a.X - c.X) * (y - c.Y)) / det;
            double l3 = 1.0 - l1 - l2;

            if (l1 < 0 || l2 < 0 || l3 < 0)
                return false;

            z = l1 * a.Z + l2 * b.Z + l3 * c.Z;
            return true;
        }

        /***************************************************/
    }
}
