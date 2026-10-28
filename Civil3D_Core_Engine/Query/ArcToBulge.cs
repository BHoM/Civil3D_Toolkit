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
using BHG = BH.oM.Geometry;

using System;
using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Converts an arc into the bulge of a polyline segment: the tangent of a quarter of the included angle, positive when the arc turns anticlockwise in plan.")]
        [Input("arc", "Arc to convert.")]
        [Output("bulge", "Bulge of the equivalent polyline segment.")]
        public static double ArcToBulge(this BHG.Arc arc)
        {
            if (arc == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot get the bulge of a null arc.");
                return 0.0;
            }

            double sweep = arc.EndAngle - arc.StartAngle;
            if (sweep < 0) sweep += 2 * Math.PI;

            // A BHoM arc runs counter-clockwise about its own CoordinateSystem.Z, while an AutoCAD bulge is positive
            // for counter-clockwise in plan (WCS Z up) and negative for clockwise. So the sign follows which way the
            // arc's Z axis points: an arc whose Z points down is clockwise in plan.
            double sign = arc.CoordinateSystem.Z.Z < 0 ? -1.0 : 1.0;
            return sign * Math.Tan(sweep / 4.0);
        }

        /***************************************************/
    }
}
