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
using ACD = Autodesk.AutoCAD.DatabaseServices;
using ACG = Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Geometry;
using BH.Engine.Geometry;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHG.PolyCurve FromCivil3D(this ACD.Polyline acadPolyline)
        {
            List<BHG.ICurve> segments = new List<BHG.ICurve>();
            int numVerts = acadPolyline.NumberOfVertices;
            for (int i = 0; i < numVerts; i++)
            {
                int next = (i + 1) % numVerts;
                // Handle open polyline
                if (!acadPolyline.Closed && i == numVerts - 1)
                    break;
                ACG.Point3d start = acadPolyline.GetPoint3dAt(i);
                ACG.Point3d end = acadPolyline.GetPoint3dAt(next);
                // Duplicate vertices give zero-length segments, which make the whole curve invalid in Rhino
                // (it then cannot be pulled to, measured or joined). Skip them; the shape is unchanged.
                if (start.DistanceTo(end) < ZeroLength)
                    continue;
                double bulge = acadPolyline.GetBulgeAt(i);
                if (bulge == 0)
                {
                    // Straight segment → Line
                    segments.Add(new BHG.Line
                    {
                        Start = start.FromCivil3D(),
                        End = end.FromCivil3D()
                    });
                }
                else
                {
                    // Arc segment
                    BHG.Arc arc = BulgeToArc(start, end, bulge);
                    segments.Add(arc);
                }
            }
            return new BHG.PolyCurve { Curves = segments };
        }

        /***************************************************/

        public static BHG.PolyCurve FromCivil3D(this ACD.Polyline2d polyline2d)
        {
            var segments = new List<BHG.ICurve>();
            var vertices = new List<ACD.Vertex2d>();

            foreach (ACD.ObjectId id in polyline2d)
            {
                var v = id.GetObject(ACD.OpenMode.ForRead) as ACD.Vertex2d;
                if (v != null)
                    vertices.Add(v);
            }

            int count = vertices.Count;

            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;

                if (!polyline2d.Closed && i == count - 1)
                    break;

                var vStart = vertices[i];
                var vEnd   = vertices[next];

                ACG.Point3d start = new ACG.Point3d(vStart.Position.X, vStart.Position.Y, 0);
                ACG.Point3d end   = new ACG.Point3d(vEnd.Position.X,   vEnd.Position.Y,   0);
                double bulge      = vStart.Bulge;

                AddSegment(segments, start, end, bulge);
            }

            return new BHG.PolyCurve { Curves = segments };
        }

        /***************************************************/

        public static BHG.PolyCurve FromCivil3D(this ACD.Polyline3d polyline3d)
        {
            var segments = new List<BHG.ICurve>();
            var vertices = new List<ACD.PolylineVertex3d>();

            // Collect vertices
            foreach (ACD.ObjectId id in polyline3d)
            {
                var v = id.GetObject(ACD.OpenMode.ForRead) as ACD.PolylineVertex3d;
                if (v != null)
                    vertices.Add(v);
            }

            int count = vertices.Count;

            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;

                if (!polyline3d.Closed && i == count - 1)
                    break;

                ACG.Point3d start = vertices[i].Position;
                ACG.Point3d end   = vertices[next].Position;

                // Polyline3d only supports straight segments
                segments.Add(new BHG.Line
                {
                    Start = start.FromCivil3D(),
                    End   = end.FromCivil3D()
                });
            }

            return new BHG.PolyCurve { Curves = segments };
        }

        /***************************************************/
    }
}
