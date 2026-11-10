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

using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Converts a Civil 3D TinSurface to BH.oM.Adapters.Civil3D.Elements.TinSurface, with its visible triangles as a mesh. " +
                     "Triangles hidden by the surface's boundaries are left out, and each vertex is used once by every triangle that shares it.")]
        [Input("civSurface", "A Civil 3D TinSurface to convert.")]
        [Output("surface", "The converted BH.oM.Adapters.Civil3D.Elements.TinSurface.")]
        public static BHC.TinSurface FromCivil3D(this ADC.TinSurface civSurface)
        {
            List<BHG.Point> vertices = new List<BHG.Point>();
            List<BHG.Face> faces = new List<BHG.Face>();

            //Triangles share their vertex objects, so equal locations are the same vertex and need no tolerance.
            Dictionary<(double X, double Y, double Z), int> vertexIndices = new Dictionary<(double X, double Y, double Z), int>();

            foreach (ADC.TinSurfaceTriangle triangle in civSurface.GetTriangles(false))
            {
                faces.Add(new BHG.Face
                {
                    A = VertexIndex(triangle.Vertex1, vertexIndices, vertices),
                    B = VertexIndex(triangle.Vertex2, vertexIndices, vertices),
                    C = VertexIndex(triangle.Vertex3, vertexIndices, vertices),
                    D = -1,
                });
            }

            if (faces.Count == 0)
                BH.Engine.Base.Compute.RecordWarning($"Surface {civSurface.Name} has no visible triangles, so its mesh is empty.");

            return new BHC.TinSurface
            {
                Description = civSurface.Description,
                Mesh = new BHG.Mesh { Vertices = vertices, Faces = faces },
                Name = civSurface.Name,
            };
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static int VertexIndex(ADC.TinSurfaceVertex vertex, Dictionary<(double X, double Y, double Z), int> vertexIndices, List<BHG.Point> vertices)
        {
            ACG.Point3d location = vertex.Location;

            int index;
            if (vertexIndices.TryGetValue((location.X, location.Y, location.Z), out index))
                return index;

            index = vertices.Count;
            vertices.Add(location.FromCivil3D());
            vertexIndices[(location.X, location.Y, location.Z)] = index;
            return index;
        }

        /***************************************************/
    }
}
