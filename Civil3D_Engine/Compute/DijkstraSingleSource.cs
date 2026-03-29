/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BH.oM.Analytical.Elements;
using BH.oM.Analytical.Graph;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Civils.Elements;


namespace BH.Engine.Adapters.Civil3D
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Computes a reusable shortest-path tree (single-source Dijkstra) from a start node across the graph.")]
        [Input("graph", "The Graph to query.")]
        [Input("start", "The Guid entity used for the start of the tree.")]
        [Output("tree", "A reusable shortest-path tree result containing distances and predecessor pointers.")]
        public static DijkstraTreeResult DijkstraShortestPathTree(this Graph graph, Guid start)
        {
            if (graph == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot compute shortest-path tree from a null graph.");
                return null;
            }

            if (graph.Entities == null || !graph.Entities.ContainsKey(start))
            {
                BH.Engine.Base.Compute.RecordError("Start node is not present in graph.Entities.");
                return null;
            }

            // Build adjacency once: O(E)
            Dictionary<Guid, List<IRelation>> adj = BuildAdjacency(graph);

            Dictionary<Guid, double> dist = new Dictionary<Guid, double>();
            Dictionary<Guid, Guid> prev = new Dictionary<Guid, Guid>();
            Dictionary<Guid, IRelation> prevRel = new Dictionary<Guid, IRelation>();
            HashSet<Guid> visited = new HashSet<Guid>();

            dist[start] = 0.0;

            // Use heap with duplicate entries allowed (no decrease-key).
            MinHeap heap = new MinHeap();
            heap.Push(start, 0.0);

            while (heap.Count > 0)
            {
                HeapItem item = heap.PopMin();
                Guid u = item.Node;
                double du = item.Priority;

                if (visited.Contains(u))
                    continue;

                // If this is a stale entry, skip (happens because we push duplicates)
                double best;
                if (!dist.TryGetValue(u, out best) || du > best)
                    continue;

                visited.Add(u);

                List<IRelation> outEdges;
                if (!adj.TryGetValue(u, out outEdges))
                    continue;

                for (int i = 0; i < outEdges.Count; i++)
                {
                    IRelation r = outEdges[i];
                    if (r == null)
                        continue;

                    Guid v = r.Target;
                    double w = r.Weight;

                    // Dijkstra requires non-negative weights
                    if (w < 0)
                        continue;

                    double alt = best + w;

                    double dv;
                    if (!dist.TryGetValue(v, out dv) || alt < dv)
                    {
                        dist[v] = alt;
                        prev[v] = u;
                        prevRel[v] = r;
                        heap.Push(v, alt);
                    }
                }
            }

            return new DijkstraTreeResult
            {
                GraphId = graph.BHoM_Guid,
                Start = start,
                Dist = dist,
                Prev = prev,
                PrevRelation = prevRel,
                Visited = visited,
                Adjacency = adj
            };
        }

        [Description("Computes a reusable shortest-path tree (single-source Dijkstra) from a start object across the graph.")]
        [Input("graph", "The Graph to query.")]
        [Input("start", "The IBHoMObject entity used for the start of the tree.")]
        [Output("tree", "A reusable shortest-path tree result containing distances and predecessor pointers.")]
        public static DijkstraTreeResult DijkstraShortestPathTree(this Graph graph, IBHoMObject start)
        {
            if (start == null)
            {
                BH.Engine.Base.Compute.RecordError("Start object is null.");
                return null;
            }

            return DijkstraShortestPathTree(graph, start.BHoM_Guid);
        }

        /***************************************************/
        /**** Private Helpers                           ****/
        /***************************************************/

        private static Dictionary<Guid, List<IRelation>> BuildAdjacency(Graph graph)
        {
            Dictionary<Guid, List<IRelation>> adj = new Dictionary<Guid, List<IRelation>>();

            if (graph.Relations == null)
                return adj;

            for (int i = 0; i < graph.Relations.Count; i++)
            {
                IRelation r = graph.Relations[i];
                if (r == null)
                    continue;

                List<IRelation> list;
                if (!adj.TryGetValue(r.Source, out list))
                {
                    list = new List<IRelation>();
                    adj[r.Source] = list;
                }
                list.Add(r);
            }

            return adj;
        }

        /***************************************************/
        /**** Simple Min-Heap (Guid, double)            ****/
        /***************************************************/

        private struct HeapItem
        {
            public Guid Node;
            public double Priority;
        }

        // Minimal binary heap. Allows duplicates (no decrease-key).
        private class MinHeap
        {
            private readonly List<HeapItem> _data = new List<HeapItem>();

            public int Count { get { return _data.Count; } }

            public void Push(Guid node, double priority)
            {
                _data.Add(new HeapItem { Node = node, Priority = priority });
                SiftUp(_data.Count - 1);
            }

            public HeapItem PopMin()
            {
                HeapItem root = _data[0];
                int last = _data.Count - 1;
                _data[0] = _data[last];
                _data.RemoveAt(last);
                if (_data.Count > 0)
                    SiftDown(0);
                return root;
            }

            private void SiftUp(int i)
            {
                while (i > 0)
                {
                    int p = (i - 1) / 2;
                    if (_data[p].Priority <= _data[i].Priority)
                        break;
                    Swap(i, p);
                    i = p;
                }
            }

            private void SiftDown(int i)
            {
                int n = _data.Count;
                while (true)
                {
                    int l = 2 * i + 1;
                    int r = 2 * i + 2;
                    int smallest = i;

                    if (l < n && _data[l].Priority < _data[smallest].Priority)
                        smallest = l;
                    if (r < n && _data[r].Priority < _data[smallest].Priority)
                        smallest = r;

                    if (smallest == i)
                        break;

                    Swap(i, smallest);
                    i = smallest;
                }
            }

            private void Swap(int a, int b)
            {
                HeapItem tmp = _data[a];
                _data[a] = _data[b];
                _data[b] = tmp;
            }
        }



        public static HashSet<IRelation> PrunedRelationsToTargets(this DijkstraTreeResult tree, IEnumerable<Guid> targetGuids)
        {
            var keep = new HashSet<IRelation>();

            foreach (var target in targetGuids)
            {
                // Skip unreachable targets
                if (!tree.Prev.ContainsKey(target))
                    continue;

                Guid current = target;

                // Walk back to the source
                while (current != tree.Start)
                {
                    if (!tree.Prev.TryGetValue(current, out Guid parent))
                        break;

                    if (tree.PrevRelation.TryGetValue(current, out IRelation rel) && rel != null)
                        keep.Add(rel);

                    current = parent;
                }
            }

            return keep;
        }

    }
}