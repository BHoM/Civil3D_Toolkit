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
using BH.oM.Analytical.Graph;


namespace BH.Engine.Adapters.Civil3D
{
    // Lightweight result object (engine-side DTO).
    // If you want this in oM later, move it to BH.oM.Analytical.* and make it a BHoMObject.
    public class DijkstraTreeResult
    {
        public Guid GraphId { get; internal set; }
        public Guid Start { get; internal set; }

        // Distances from Start (cost)
        public Dictionary<Guid, double> Dist { get; internal set; }

        // Predecessor pointers: Prev[v] = u
        public Dictionary<Guid, Guid> Prev { get; internal set; }

        // The relation used to reach v from Prev[v]
        public Dictionary<Guid, IRelation> PrevRelation { get; internal set; }

        // Visited nodes (for debugging / diagnostics)
        public HashSet<Guid> Visited { get; internal set; }

        // Adjacency cached for this run (optional, but handy if you want to reuse it)
        internal Dictionary<Guid, List<IRelation>> Adjacency { get; set; }
    }
}