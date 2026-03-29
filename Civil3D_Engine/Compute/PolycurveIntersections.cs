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

/*
 * Custom extension for post-processing PolyCurve intersections.
 * Culls intersection points that only occur once (i.e. intersect only
 * a single sub-curve pair).
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using BH.Engine.Geometry;

namespace BH.Engine.Adapters.Civil3D
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Computes PolyCurve intersection points and culls points that only occur once (i.e. intersect only a single sub-curve).")]
        [Input("curve1", "The first PolyCurve to intersect.")]
        [Input("curve2", "The second PolyCurve to intersect.")]
        [Input("tolerance", "Geometrical tolerance to be used in the method.")]
        [Output("intersections", "Intersection points shared by more than one sub-curve intersection.")]
        public static List<Point> CurveIntersectionsCulled(
            this PolyCurve curve1,
            PolyCurve curve2,
            double tolerance = Tolerance.Distance)
        {
            // Use existing engine logic
            List<Point> rawIntersections =
                BH.Engine.Geometry.Query.CurveIntersections(curve1, curve2, tolerance);

            if (rawIntersections == null || rawIntersections.Count == 0)
                return new List<Point>();

            // Cluster by tolerance and count occurrences
            List<Point> result = new List<Point>();
            bool[] used = new bool[rawIntersections.Count];

            for (int i = 0; i < rawIntersections.Count; i++)
            {
                if (used[i])
                    continue;

                Point p = rawIntersections[i];
                int count = 1;
                used[i] = true;

                for (int j = i + 1; j < rawIntersections.Count; j++)
                {
                    if (used[j])
                        continue;

                    if (p.Distance(rawIntersections[j]) <= tolerance)
                    {
                        count++;
                        used[j] = true;
                    }
                }

                // Cull points that only appear once
                if (count > 1)
                    result.Add(p);
            }

            return result;
        }
    }
}