/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024
 * License: LGPL-3.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using BHG = BH.oM.Geometry;
using BHC = BH.oM.Civils.Elements;
using ADC = Autodesk.Civil.DatabaseServices;
using BH.Engine.Geometry;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** New: Mesh-based Converters                ****/
        /***************************************************/

        // Convert a Civil 3D TIN surface to a BHoM Mesh
        public static BHC.CivSurface ToBHoM(this ADC.TinSurface acSurface)
        {
            if (acSurface == null)
                return null;
            // Tolerance for vertex welding (e.g. 1mm)
            double weldTolerance = 0.0005;

            // Vertex weld: map of rounded coordinate key -> index in vertices
            var vertexIndex = new Dictionary<(long X, long Y, long Z), int>();
            var vertices = new List<BHG.Point>();
            var facesIdx = new List<int[]>(); // each is {i0, i1, i2}

            foreach (ADC.TinSurfaceTriangle tri in acSurface.Triangles)
            {
                int i0 = AddVertex(tri.Vertex1.Location.FromCivil3D(), weldTolerance, vertexIndex, vertices);
                int i1 = AddVertex(tri.Vertex2.Location.FromCivil3D(), weldTolerance, vertexIndex, vertices);
                int i2 = AddVertex(tri.Vertex3.Location.FromCivil3D(), weldTolerance, vertexIndex, vertices);

                // Skip degenerate faces (identical indices)
                if (i0 != i1 && i1 != i2 && i2 != i0)
                    facesIdx.Add(new[] { i0, i1, i2, -1 }); // When face is triangular, set D = -1 per BHoM Face definitio
            }
 


            // Define bhomFaces and create each face via property initialiser (A,B,C,D)
            var bhomFaces = new List<BHG.Face>(facesIdx.Count);
            foreach (var idx in facesIdx)
            {
                var face = new BHG.Face
                {
                    A = idx[0],
                    B = idx[1],
                    C = idx[2],
                    D = idx[3] // -1 for triangles
                };
                bhomFaces.Add(face);
            }


            
            // Safest construction: assign Vertices/Faces directly

            var mesh = new BHG.Mesh
            {
                Vertices = vertices,
                Faces = bhomFaces
            };

            
            return new BHC.CivSurface
            {
                Mesh = mesh   // BHG.Mesh
            };

            
        }

        // Helper: quantised weld + append
        private static int AddVertex(BHG.Point p, double tol,
            Dictionary<(long X, long Y, long Z), int> map, List<BHG.Point> verts)
        {
            // Quantise by tolerance to avoid floating point key drift
            long qx = (long)Math.Round(p.X / tol);
            long qy = (long)Math.Round(p.Y / tol);
            long qz = (long)Math.Round(p.Z / tol);

            var key = (qx, qy, qz);
            if (map.TryGetValue(key, out int idx))
                return idx;

            idx = verts.Count;
            verts.Add(p);
            map[key] = idx;
            return idx;
        }

        /***************************************************/
        /**** Existing Methods (kept for compatibility) ****/
        /***************************************************/

        // public static BHC.CivSurface ToBHoM(this ADC.TinSurface acSurface)
        // {
            // Existing behaviour: polyline triangles
        //     var polylines = new List<BHG.Polyline>();
        //     foreach (ADC.TinSurfaceTriangle triangle in acSurface.Triangles)
        //         polylines.Add(triangle.ToBHoM());

        //     return new BHC.CivSurface
        //     {
        //         Triangles = polylines,
        //     };
        // }

        public static BHG.Polyline ToBHoM(this ADC.TinSurfaceTriangle triangle)
        {
            var pts = new List<BHG.Point>
            {
                triangle.Vertex1.Location.FromCivil3D(),
                triangle.Vertex2.Location.FromCivil3D(),
                triangle.Vertex3.Location.FromCivil3D(),
            };
            // close
            pts.Add(pts[0]);
            return BH.Engine.Geometry.Create.Polyline(pts);
        }

    }
}