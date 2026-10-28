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
using System.ComponentModel;
using BHG = BH.oM.Geometry;

using ACG = Autodesk.AutoCAD.Geometry;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Converts a polyline segment given by its start point, end point and bulge into an arc. The bulge is the tangent of a quarter of the included angle, positive when the arc turns anticlockwise in plan.")]
        [Input("start", "Start point of the segment.")]
        [Input("end", "End point of the segment.")]
        [Input("bulge", "Bulge of the segment. Zero means a straight segment.")]
        [Output("arc", "The arc, or null if the segment is straight or has zero length.")]
        public static BHG.Arc BulgeToArc(this ACG.Point3d start, ACG.Point3d end, double bulge)
        {
            double chord = start.DistanceTo(end);
            if (System.Math.Abs(bulge) < 1e-10 || chord < 1e-10)
                return null;

            // Included angle (signed). bulge = tan(theta/4)  [2](https://lee-mac.com/bulgeconversion.html)
            double theta = 4.0 * System.Math.Atan(bulge);
            double half = System.Math.Abs(theta) / 2.0;

            double sinHalf = System.Math.Sin(half);
            if (System.Math.Abs(sinHalf) < 1e-10)
                return null;

            // Positive radius
            double radius = chord / (2.0 * sinHalf);

            // Midpoint of chord
            ACG.Point3d mid = new ACG.Point3d(
                (start.X + end.X) / 2.0,
                (start.Y + end.Y) / 2.0,
                (start.Z + end.Z) / 2.0
            );

            // Perpendicular unit (left of chord, in XY)
            ACG.Vector3d chordVec = end - start;
            ACG.Vector3d perpDir = new ACG.Vector3d(-chordVec.Y, chordVec.X, 0.0);
            if (perpDir.Length < 1e-10)
                perpDir = new ACG.Vector3d(1, 0, 0);
            perpDir = perpDir.GetNormal();

            // Signed distance from the chord midpoint to the centre, along the left normal: halfChord * (1 - b^2) / (2b).
            // For an arc of less than 180 degrees (|b| < 1) the centre is on the bulge's side; past 180 degrees (|b| > 1,
            // e.g. a cul-de-sac loop) it crosses to the other side. Always putting it on the bulge's side mirrored those
            // arcs, so they no longer met their neighbours and the polyline failed to convert in Grasshopper.
            double halfChord = chord / 2.0;
            double offset = halfChord * (1.0 - bulge * bulge) / (2.0 * bulge);

            ACG.Point3d center = mid + perpDir * offset;

            // Local axes
            ACG.Vector3d centerToStart = (start - center).GetNormal();
            BHG.Vector xAxis = centerToStart.FromCivil3D();

            // Flip handedness for negative bulge so positive sweep goes the "right" way
            BHG.Vector zAxis = new BHG.Vector { X = 0, Y = 0, Z = (bulge >= 0.0) ? 1.0 : -1.0 };
            BHG.Vector yAxis = BH.Engine.Geometry.Query.CrossProduct(zAxis, xAxis);
            yAxis = BH.Engine.Geometry.Modify.Normalise(yAxis);

            var system = BH.Engine.Geometry.Create.CartesianCoordinateSystem(
                center.FromCivil3D(),
                xAxis,
                yAxis
            );

            // Use magnitude only; DO NOT reflex-convert (the flipped system handles CW)
            double sweep = System.Math.Abs(theta);

            return BH.Engine.Geometry.Create.Arc(system, radius, 0.0, sweep);
        }

        /***************************************************/
    }
}
