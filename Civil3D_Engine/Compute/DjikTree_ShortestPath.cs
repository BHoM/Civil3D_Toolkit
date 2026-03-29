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
using BH.oM.Analytical.Elements;
using BH.oM.Analytical.Graph;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Civils.Elements;
using BH.Engine.Adapters.Civil3D;
using BH.oM.Geometry;

namespace BH.Engine.Adapters.Civil3D
{
    public static partial class Query
    {
        [Description("Gets a ShortestPathResult to a specific end node using a precomputed DijkstraShortestPathTree.")]
        [Input("tree", "The reusable shortest-path tree result.")]
        [Input("graph", "The graph that the tree was computed from.")]
        [Input("end", "The Guid entity used for the end of the path.")]
        [Output("shortest path result", "The ShortestPathResult.")]
        public static ShortestPathResult PathTo(this DijkstraTreeResult tree, Graph graph, Guid end)
        {
            if (tree == null || graph == null)
            {
                BH.Engine.Base.Compute.RecordError("Tree or graph is null.");
                return null;
            }

            if (tree.GraphId != graph.BHoM_Guid)
            {
                BH.Engine.Base.Compute.RecordError("Tree was computed for a different graph.");
                return null;
            }

            if (!graph.Entities.ContainsKey(end))
            {
                BH.Engine.Base.Compute.RecordError("End node not present in graph.Entities.");
                return null;
            }

            double endCost;
            if (!tree.Dist.TryGetValue(end, out endCost))
            {
                BH.Engine.Base.Compute.RecordError("End node is not reachable from start.");
                return null;
            }

            // Reconstruct nodes and relations by walking Prev pointers backwards
            List<Guid> nodes = new List<Guid>();
            List<IRelation> rels = new List<IRelation>();

            Guid current = end;
            nodes.Add(current);

            while (current != tree.Start)
            {
                Guid p;
                if (!tree.Prev.TryGetValue(current, out p))
                    break; // Should not happen if reachable

                IRelation pr;
                if (tree.PrevRelation.TryGetValue(current, out pr) && pr != null)
                    rels.Add(pr);

                current = p;
                nodes.Add(current);
            }

            nodes.Reverse();
            rels.Reverse();

            // Convert nodes to objects
            List<IBHoMObject> objPath = new List<IBHoMObject>();
            for (int i = 0; i < nodes.Count; i++)
                objPath.Add(graph.Entities[nodes[i]]);

            // Entities visited in the Dijkstra run
            List<IBHoMObject> visited = new List<IBHoMObject>();
            foreach (Guid g in tree.Visited)
                visited.Add(graph.Entities[g]);

            // "length" here as hop-count, mirroring original (but you could redefine)
            double length = rels.Count;
            double cost = endCost;

            return new ShortestPathResult(
                graph.BHoM_Guid,
                "DijkstraShortestPathTree",
                -1,
                objPath,
                length,
                cost,
                visited,
                rels
            );
        }



        [Description("Gets a ShortestPathResult to a specific end object using a precomputed DijkstraShortestPathTree.")]
        [Input("tree", "The reusable shortest-path tree result.")]
        [Input("graph", "The graph that the tree was computed from.")]
        [Input("end", "The IBHoMObject entity used for the end of the path.")]
        [Output("shortest path result", "The ShortestPathResult.")]
        public static ShortestPathResult PathTo(
            this DijkstraTreeResult tree,
            Graph graph,
            IBHoMObject end)
        {
            if (end == null)
            {
                BH.Engine.Base.Compute.RecordError("End object is null.");
                return null;
            }

            return PathTo(tree, graph, end.BHoM_Guid);
        }

        [Description("Returns the unique set of relations used by the paths from the tree start to multiple target nodes.")]
        [Input("tree", "The reusable shortest-path tree result.")]
        [Input("targets", "Target node Guids.")]
        [Output("relations", "Unique relations used across all reconstructed paths.")]
        public static List<IRelation> UnionRelationsToTargets(this DijkstraTreeResult tree, IEnumerable<Guid> targets)
        {
            if (tree == null || targets == null)
                return new List<IRelation>();

            HashSet<EdgeKey> seen = new HashSet<EdgeKey>();
            List<IRelation> result = new List<IRelation>();

            foreach (Guid t in targets)
            {
                if (!tree.Dist.ContainsKey(t))
                    continue; // unreachable or unknown

                Guid current = t;

                while (current != tree.Start)
                {
                    Guid p;
                    if (!tree.Prev.TryGetValue(current, out p))
                        break;

                    IRelation r;
                    if (tree.PrevRelation.TryGetValue(current, out r) && r != null)
                    {
                        EdgeKey k = new EdgeKey(r.Source, r.Target);
                        if (!seen.Contains(k))
                        {
                            seen.Add(k);
                            result.Add(r);
                        }
                    }

                    current = p;
                }
            }

            return result;
        }

        private struct EdgeKey : IEquatable<EdgeKey>
        {
            public Guid A;
            public Guid B;

            public EdgeKey(Guid a, Guid b)
            {
                A = a;
                B = b;
            }

            public bool Equals(EdgeKey other)
            {
                return A.Equals(other.A) && B.Equals(other.B);
            }

            public override bool Equals(object obj)
            {
                return obj is EdgeKey && Equals((EdgeKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (A.GetHashCode() * 397) ^ B.GetHashCode();
                }
            }
        }
    }
}