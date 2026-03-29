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
using BH.oM.Geometry;
using BH.Engine.Geometry;



namespace BH.Engine.Adapters.Civil3D
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        
        public static List<ICurve> ToCurveTree(this DijkstraTreeResult tree,Graph graph, IEnumerable<Guid> targetGuids)
        {
            var curves = new List<ICurve>();

            var keep = tree.PrunedRelationsToTargets(targetGuids);

            foreach (var rel in keep)
            {
                if (rel == null)
                    continue;

                // CASE A: Relation directly carries geometry (best case)
                var geomFragment = rel.Curve;
                
                if (geomFragment != null && rel.Curve is ICurve curve)
                {
                    curves.Add(curve);
                    continue;
                }

                // CASE B: Build curve from node positions
                var a = graph.Entities[rel.Source] as INode;
                var b = graph.Entities[rel.Target] as INode;
                

                if (a != null && b != null)
                {

                var l = new BH.oM.Geometry.Line
                    {
                        Start = a.Position,
                        End   = b.Position
                    };
                    curves.Add(l);
                }
            }

            return curves;
        }

        public static Graph ToSubgraph(this DijkstraTreeResult tree, Graph sourceGraph)
        {
            if (tree == null || sourceGraph == null)
                return null;

            // Create a new graph to hold the subgraph
            var subgraph = new Graph
            {
                Entities = new Dictionary<Guid, IBHoMObject>(),
                Relations = new List<IRelation>()
            };

            // Add all visited nodes from the source graph
            if (tree.Visited != null && sourceGraph.Entities != null)
            {
                foreach (var nodeId in tree.Visited)
                {
                    if (sourceGraph.Entities.TryGetValue(nodeId, out var entity))
                    {
                        subgraph.Entities[nodeId] = entity;
                    }
                }
            }

            // Add all relations that form the shortest path tree
            if (tree.PrevRelation != null)
            {
                foreach (var relation in tree.PrevRelation.Values)
                {
                    if (relation != null)
                    {
                        subgraph.Relations.Add(relation);
                    }
                }
            }

            return subgraph;
        }

    }
}