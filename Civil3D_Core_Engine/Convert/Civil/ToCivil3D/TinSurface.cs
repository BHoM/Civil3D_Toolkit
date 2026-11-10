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

using BHC = BH.oM.Adapters.Civil3D.Elements;
using BH.oM.Base.Attributes;
using BHG = BH.oM.Geometry;

using ACG = Autodesk.AutoCAD.Geometry;
using ADC = Autodesk.Civil.DatabaseServices;
using Autodesk.Civil;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Applies the mesh and description of a BH.oM.Adapters.Civil3D.Elements.TinSurface to an empty Civil 3D TinSurface. " +
                     "The mesh vertices are added and Civil 3D triangulates them. The open edges of the mesh are added as boundaries: " +
                     "the largest outline as the outer boundary, holes as hide boundaries and islands within holes as show boundaries, so concave outlines and holes are kept. " +
                     "If the mesh has separate parts, only the largest is kept. " +
                     "Inside the boundaries, Civil 3D's triangles can differ from the mesh faces where the mesh is not a Delaunay triangulation.")]
        [Input("bhSurface", "BH.oM.Adapters.Civil3D.Elements.TinSurface to convert, already in drawing units.")]
        [Input("c3dSurface", "Empty Civil 3D TinSurface, open for write, to apply the values to.")]
        public static void ToCivil3D(this BHC.TinSurface bhSurface, ADC.TinSurface c3dSurface)
        {
            if (bhSurface == null || c3dSurface == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot apply a null surface, or apply one to a null Civil 3D surface.");
                return;
            }

            BHG.Mesh mesh = bhSurface.Mesh;
            if (mesh?.Vertices == null || mesh.Vertices.Count < 3)
            {
                BH.Engine.Base.Compute.RecordError($"Surface {bhSurface.Name} needs a mesh with at least 3 vertices.");
                return;
            }

            if (!string.IsNullOrEmpty(bhSurface.Description))
                c3dSurface.Description = bhSurface.Description;

            ACG.Point3dCollection locations = new ACG.Point3dCollection();
            foreach (BHG.Point vertex in mesh.Vertices)
                locations.Add(vertex.ToCivil3D());

            c3dSurface.AddVertices(locations);

            List<List<int>> loops = BoundaryLoops(mesh);
            if (loops.Count == 0)
            {
                BH.Engine.Base.Compute.RecordWarning($"The outline of surface {bhSurface.Name} could not be found from its mesh faces, so no boundary was added and Civil 3D triangulated all of its vertices.");
            }
            else
            {
                List<int> depths = loops.Select(x => loops.Count(y => y != x && IsInsideInPlan(mesh.Vertices[x[0]], y, mesh.Vertices))).ToList();
                List<int> outer = loops.Where((x, i) => depths[i] == 0).OrderByDescending(x => Math.Abs(PlanArea(x, mesh.Vertices))).First();

                int separateParts = 0;
                for (int i = 0; i < loops.Count; i++)
                {
                    SurfaceBoundaryType boundaryType;
                    if (loops[i] == outer)
                        boundaryType = SurfaceBoundaryType.Outer;
                    else if (depths[i] == 0)
                    {
                        separateParts++;
                        continue;
                    }
                    else
                        boundaryType = depths[i] % 2 == 1 ? SurfaceBoundaryType.Hide : SurfaceBoundaryType.Show;

                    c3dSurface.BoundariesDefinition.AddBoundaries(LoopPoints(loops[i], mesh.Vertices), 0.1, boundaryType, true);
                }

                if (separateParts > 0)
                    BH.Engine.Base.Compute.RecordWarning($"The mesh of surface {bhSurface.Name} has {separateParts + 1} separate parts. Only the largest part was kept, inside the surface's outer boundary; push the other parts as separate surfaces.");
            }

            c3dSurface.Rebuild();
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        //Finds the closed loops of open edges (edges used by exactly one face), as lists of vertex indices.
        private static List<List<int>> BoundaryLoops(BHG.Mesh mesh)
        {
            Dictionary<(int, int), int> edgeUses = new Dictionary<(int, int), int>();
            int invalidFaces = 0;

            foreach (BHG.Face face in mesh.Faces ?? new List<BHG.Face>())
            {
                List<int> corners = face.D < 0 ? new List<int> { face.A, face.B, face.C } : new List<int> { face.A, face.B, face.C, face.D };
                if (corners.Any(x => x < 0 || x >= mesh.Vertices.Count))
                {
                    invalidFaces++;
                    continue;
                }

                for (int i = 0; i < corners.Count; i++)
                {
                    int a = corners[i];
                    int b = corners[(i + 1) % corners.Count];
                    if (a == b)
                        continue;

                    (int, int) key = a < b ? (a, b) : (b, a);
                    int uses;
                    edgeUses[key] = edgeUses.TryGetValue(key, out uses) ? uses + 1 : 1;
                }
            }

            if (invalidFaces > 0)
                BH.Engine.Base.Compute.RecordWarning($"{invalidFaces} mesh faces refer to vertices that do not exist, so they were ignored when finding the surface's outline.");

            Dictionary<int, List<int>> neighbours = new Dictionary<int, List<int>>();
            foreach ((int, int) edge in edgeUses.Where(x => x.Value == 1).Select(x => x.Key))
            {
                AddNeighbour(neighbours, edge.Item1, edge.Item2);
                AddNeighbour(neighbours, edge.Item2, edge.Item1);
            }

            List<List<int>> loops = new List<List<int>>();
            while (neighbours.Count > 0)
            {
                int start = neighbours.Keys.First();
                List<int> loop = new List<int> { start };
                int current = start;
                bool closed = false;

                List<int> next;
                while (neighbours.TryGetValue(current, out next) && next.Count > 0)
                {
                    int following = next[0];
                    RemoveNeighbour(neighbours, current, following);
                    RemoveNeighbour(neighbours, following, current);

                    if (following == start)
                    {
                        closed = true;
                        break;
                    }

                    loop.Add(following);
                    current = following;
                }

                if (closed && loop.Count >= 3)
                    loops.Add(loop);
            }

            return loops;
        }

        /***************************************************/

        private static void AddNeighbour(Dictionary<int, List<int>> neighbours, int vertex, int neighbour)
        {
            List<int> list;
            if (!neighbours.TryGetValue(vertex, out list))
            {
                list = new List<int>();
                neighbours[vertex] = list;
            }

            list.Add(neighbour);
        }

        /***************************************************/

        private static void RemoveNeighbour(Dictionary<int, List<int>> neighbours, int vertex, int neighbour)
        {
            List<int> list;
            if (!neighbours.TryGetValue(vertex, out list))
                return;

            list.Remove(neighbour);
            if (list.Count == 0)
                neighbours.Remove(vertex);
        }

        /***************************************************/

        //Signed area of a loop in plan (shoelace formula).
        private static double PlanArea(List<int> loop, List<BHG.Point> vertices)
        {
            double area = 0.0;
            for (int i = 0; i < loop.Count; i++)
            {
                BHG.Point a = vertices[loop[i]];
                BHG.Point b = vertices[loop[(i + 1) % loop.Count]];
                area += a.X * b.Y - b.X * a.Y;
            }

            return area / 2.0;
        }

        /***************************************************/

        //Whether a point lies inside a loop in plan (ray casting).
        private static bool IsInsideInPlan(BHG.Point point, List<int> loop, List<BHG.Point> vertices)
        {
            bool inside = false;
            for (int i = 0, j = loop.Count - 1; i < loop.Count; j = i++)
            {
                BHG.Point a = vertices[loop[i]];
                BHG.Point b = vertices[loop[j]];
                if ((a.Y > point.Y) != (b.Y > point.Y) && point.X < (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y) + a.X)
                    inside = !inside;
            }

            return inside;
        }

        /***************************************************/

        //Points of a loop, with the first point repeated at the end to close it.
        private static ACG.Point3dCollection LoopPoints(List<int> loop, List<BHG.Point> vertices)
        {
            ACG.Point3dCollection points = new ACG.Point3dCollection();
            foreach (int index in loop)
                points.Add(vertices[index].ToCivil3D());

            points.Add(vertices[loop[0]].ToCivil3D());
            return points;
        }

        /***************************************************/
    }
}
