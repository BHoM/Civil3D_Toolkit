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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHG = BH.oM.Geometry;
using BHC = BH.oM.Civils.Elements;
using Autodesk.Civil.DatabaseServices;

using Autodesk.AutoCAD.DatabaseServices;


using Autodesk.Civil.ApplicationServices;
namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHC.Pipe ToBHoM(this Pipe acPipe, Transaction tr)
        {

            ObjectId networkId = acPipe.NetworkId;
            Network network = tr.GetObject(networkId, OpenMode.ForRead) as Network;
            string networkName = network?.Name;

            BHG.ICurve centreLine = ExtractPipeCentreLine(acPipe);

            return new BHC.Pipe
            {          
                Name = acPipe.Name,
                CentreLine = centreLine,
                Diameter = acPipe.InnerDiameterOrWidth,
                Thickness = acPipe.WallThickness,
                FlowDirection = acPipe.FlowDirectionMethod.ToBHoM(),
                PipeNetworkName = networkName
            };

        }


        //public static BHC.Pipe ToBHoM(this PressurePipe acPipe, Transaction tr)
        //{
        //    ObjectId networkId = acPipe.NetworkId;
        //    PressurePipeNetwork network = tr.GetObject(networkId, OpenMode.ForRead) as PressurePipeNetwork;
        //    string networkName = network?.Name;

        //    // BHG.ICurve centreLine = ExtractPipeCentreLine(acPipe);

        //    return new BHC.Pipe
        //    {
                
        //        Name = acPipe.Name,
        //        // CentreLine = centreLine,
        //        Diameter = acPipe.InnerDiameter,
        //        Thickness = acPipe.WallThickness,
        //        PipeNetworkName = networkName
        //    };

        //}        

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static BHG.ICurve ExtractPipeCentreLine(Pipe acPipe)
        {
            // Try to extract the actual centreline geometry from the pipe
            // If the pipe has intermediate structures, connect through them
            // Otherwise, default to a straight line between start and end points
            
            try
            {               
                // If we only have start and end, create a line; otherwise extract the actual curve
                if (acPipe.Curve2d == null)
                {
                    return new BHG.Line { Start = acPipe.StartPoint.FromCivil3D(), End = acPipe.EndPoint.FromCivil3D() };
                }
                else
                {
                    return ElevateCurve2dTo3d(acPipe.Curve2d, acPipe.StartPoint.FromCivil3D(), acPipe.EndPoint.FromCivil3D());
                }
            }
            catch
            {
                // Fallback to simple line if anything goes wrong
                return new BHG.Line { Start = acPipe.StartPoint.FromCivil3D(), End = acPipe.EndPoint.FromCivil3D() };
            }
        }

        private static BHG.ICurve ElevateCurve2dTo3d(object curve2d, BHG.Point startPoint, BHG.Point endPoint)
        {
            // Convert 2D curve to BHoM curve
            BHG.ICurve baseCurve = ((dynamic)curve2d).FromCivil3D();
            
            double zDifference = endPoint.Z - startPoint.Z;
            
            // Estimate curve length by calculating distance between start and end
            double dx = endPoint.X - startPoint.X;
            double dy = endPoint.Y - startPoint.Y;
            double curveLength = Math.Sqrt(dx * dx + dy * dy);
            
            // If curve is an Arc, calculate its arc length
            if (baseCurve is BHG.Arc arc)
            {
                // Arc length = radius * angle (angle should be absolute sweep)
                double sweepAngle = Math.Abs(arc.EndAngle - arc.StartAngle);
                curveLength = arc.Radius * sweepAngle;
            }
            
            double gradient = zDifference / curveLength; // Z change per unit length
            
            // Get 3D points at start, middle, and end of arc
            BHG.Point pt0 = startPoint;
            
            // Get midpoint - for Arc curves use the engine method, otherwise interpolate
            BHG.Point ptMid2d;
            if (baseCurve is BHG.Arc)
            {
                ptMid2d = BH.Engine.Geometry.Query.PointAtParameter(baseCurve as BHG.Arc, 0.5);
            }
            else
            {
                // Interpolate midpoint for other curve types
                ptMid2d = new BHG.Point 
                { 
                    X = (startPoint.X + endPoint.X) / 2, 
                    Y = (startPoint.Y + endPoint.Y) / 2, 
                    Z = (startPoint.Z + endPoint.Z) / 2 
                };
            }
            
            double distanceToMid = curveLength * 0.5;
            // Create 3D point by adjusting Z coordinate on 2D mid point
            BHG.Point ptMid = ptMid2d + new BHG.Vector { X = 0, Y = 0, Z = startPoint.Z + (gradient * distanceToMid) - ptMid2d.Z };
            
            BHG.Point pt1 = endPoint;
            
            // Create a 3D arc from the three points
            return CreateArcFrom3Points(pt0, ptMid, pt1);
        }

        private static BHG.ICurve CreateArcFrom3Points(BHG.Point pt0, BHG.Point ptMid, BHG.Point pt1)
        {
            // Find the center and radius from 3 points
            // Calculate two vectors
            BHG.Vector v1 = ptMid - pt0;
            BHG.Vector v2 = pt1 - ptMid;
            
            // Find perpendicular bisectors to locate center
            BHG.Point mid1 = pt0 + v1 * 0.5;
            BHG.Point mid2 = ptMid + v2 * 0.5;
            
            // Create perpendicular vectors (rotate 90 degrees in XY plane)
            BHG.Vector perp1 = new BHG.Vector { X = -v1.Y, Y = v1.X, Z = 0 };
            BHG.Vector perp2 = new BHG.Vector { X = -v2.Y, Y = v2.X, Z = 0 };
            
            // Normalize by dividing by magnitude
            double mag1 = Math.Sqrt(perp1.X * perp1.X + perp1.Y * perp1.Y + perp1.Z * perp1.Z);
            double mag2 = Math.Sqrt(perp2.X * perp2.X + perp2.Y * perp2.Y + perp2.Z * perp2.Z);
            
            if (mag1 > 1e-10)
                perp1 = new BHG.Vector { X = perp1.X / mag1, Y = perp1.Y / mag1, Z = perp1.Z / mag1 };
            if (mag2 > 1e-10)
                perp2 = new BHG.Vector { X = perp2.X / mag2, Y = perp2.Y / mag2, Z = perp2.Z / mag2 };
            
            // Find intersection of perpendicular bisectors (arc center)
            // Using parametric line equations: mid1 + t1*perp1 = mid2 + t2*perp2
            double denom = perp1.X * perp2.Y - perp1.Y * perp2.X;
            if (Math.Abs(denom) < 1e-10)
            {
                // Points are collinear, return line instead
                return new BHG.Line { Start = pt0, End = pt1 };
            }
            
            BHG.Vector dm = mid2 - mid1;
            double t1 = (dm.X * perp2.Y - dm.Y * perp2.X) / denom;
            
            BHG.Point centre = mid1 + perp1 * t1;
            
            // Calculate radius as distance from centre to pt0
            double dx = pt0.X - centre.X;
            double dy = pt0.Y - centre.Y;
            double dz = pt0.Z - centre.Z;
            double radius = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            
            // Create local coordinate system for the arc
            BHG.Vector xAxisRaw = pt0 - centre;
            double xAxisMag = Math.Sqrt(xAxisRaw.X * xAxisRaw.X + xAxisRaw.Y * xAxisRaw.Y + xAxisRaw.Z * xAxisRaw.Z);
            BHG.Vector xAxis = new BHG.Vector { X = xAxisRaw.X / xAxisMag, Y = xAxisRaw.Y / xAxisMag, Z = xAxisRaw.Z / xAxisMag };
            
            // Cross product: v1 x v2
            BHG.Vector normalRaw = new BHG.Vector 
            { 
                X = v1.Y * v2.Z - v1.Z * v2.Y,
                Y = v1.Z * v2.X - v1.X * v2.Z,
                Z = v1.X * v2.Y - v1.Y * v2.X
            };
            double normalMag = Math.Sqrt(normalRaw.X * normalRaw.X + normalRaw.Y * normalRaw.Y + normalRaw.Z * normalRaw.Z);
            BHG.Vector normal = new BHG.Vector { X = normalRaw.X / normalMag, Y = normalRaw.Y / normalMag, Z = normalRaw.Z / normalMag };
            
            // Cross product: normal x xAxis
            BHG.Vector yAxisRaw = new BHG.Vector
            {
                X = normal.Y * xAxis.Z - normal.Z * xAxis.Y,
                Y = normal.Z * xAxis.X - normal.X * xAxis.Z,
                Z = normal.X * xAxis.Y - normal.Y * xAxis.X
            };
            double yAxisMag = Math.Sqrt(yAxisRaw.X * yAxisRaw.X + yAxisRaw.Y * yAxisRaw.Y + yAxisRaw.Z * yAxisRaw.Z);
            BHG.Vector yAxis = new BHG.Vector { X = yAxisRaw.X / yAxisMag, Y = yAxisRaw.Y / yAxisMag, Z = yAxisRaw.Z / yAxisMag };
                        
            // Calculate angles from center to points - using dot product manually
            BHG.Vector toP0 = pt0 - centre;
            BHG.Vector toPMid = ptMid - centre;
            BHG.Vector toP1 = pt1 - centre;
            
            double angle0 = Math.Atan2(toP0.X * yAxis.X + toP0.Y * yAxis.Y + toP0.Z * yAxis.Z, toP0.X * xAxis.X + toP0.Y * xAxis.Y + toP0.Z * xAxis.Z);
            double angleMid = Math.Atan2(toPMid.X * yAxis.X + toPMid.Y * yAxis.Y + toPMid.Z * yAxis.Z, toPMid.X * xAxis.X + toPMid.Y * xAxis.Y + toPMid.Z * xAxis.Z);
            double angle1 = Math.Atan2(toP1.X * yAxis.X + toP1.Y * yAxis.Y + toP1.Z * yAxis.Z, toP1.X * xAxis.X + toP1.Y * xAxis.Y + toP1.Z * xAxis.Z);
            
            // Determine direction and normalize angles
            double startAngle = angle0;
            double endAngle = angle1;
            
            if (angleMid < startAngle && startAngle < endAngle)
            {
                // Adjust if mid is outside the range
            }
            else if (startAngle > endAngle)
            {
                endAngle += Math.PI * 2;
            }
            
            var cs = BH.Engine.Geometry.Create.CartesianCoordinateSystem(centre, xAxis, yAxis);
            double sweep = endAngle - startAngle;
            return (BHG.ICurve)BH.Engine.Geometry.Create.Arc(cs, radius, startAngle, sweep);
        }
    }
}

