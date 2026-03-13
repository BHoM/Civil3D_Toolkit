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

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using AAD = Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.Runtime;
using ADC = Autodesk.Civil.DatabaseServices;


using BH.UI.Civil.Engine;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

using BH.Engine.Geometry;

using BH.oM.Adapter;

namespace BH.UI.Civil.Adapter
{
    public partial class CivilUIAdapter
    {

        /***************************************************/
        /****           Adapter Methods                 ****/
        /***************************************************/

        //General method called by the adapter for push
        protected override bool ICreate<T>(IEnumerable<T> objects, ActionConfig actionConfig = null)
        {
            return CreateCollection(objects as dynamic);
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private bool CreateCollection(IEnumerable<BH.oM.Civils.Elements.Pipe> pipes)
        {
            List<BH.oM.Civils.Elements.Pipe> p = pipes.ToList();

            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (DocumentLock acLckDoc = acDoc.LockDocument())
            {
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    try
                    {
                        BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (BH.oM.Civils.Elements.Pipe pipe in p)
                        {
                            Line crv = ((pipe.CentreLine as BH.oM.Geometry.Line).ToCivil3D());
                            crv.SetDatabaseDefaults();

                            acBlkTblRec.AppendEntity(crv);

                            acTrans.AddNewlyCreatedDBObject(crv, true);
                        }
                    }
                    catch (System.Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.ToString());
                        List<string> stack = e.StackTrace.Split(new char[] { '\n' })
                            .ToList();

                        foreach (string s in stack)
                        {
                            System.Windows.Forms.MessageBox.Show(s);
                        }

                        System.Windows.Forms.MessageBox.Show("END");
                    }

                    acTrans.Commit();
                }
            }

            return true;
        }

        private bool CreateCollection(IEnumerable<BH.oM.Civils.Elements.CivSurface> srfs)
        {
            List<BH.oM.Civils.Elements.CivSurface> srf = srfs.ToList();

            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (DocumentLock acLckDoc = acDoc.LockDocument())
            {
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    try
                    {
                        BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (BH.oM.Civils.Elements.CivSurface s in srf)
                        {
                            if (s?.Mesh == null || s.Mesh.Vertices == null || s.Mesh.Faces == null)
                                continue;

                            var pts = s.Mesh.Vertices;   // List<BH.oM.Geometry.Point>
                            var fcs = s.Mesh.Faces;      // List<BH.oM.Geometry.Face> (A,B,C,D; D == -1 => triangle)
                            var name = string.IsNullOrEmpty(s.Name) ? "CivSurface_Mesh" : s.Name;

                            // Define the style name
                            string styleName = "Standard";

                            // Get the Civil 3D document
                            CivilDocument civDoc = CivilApplication.ActiveDocument;

                            // Retrieve the style ObjectId from the name
                            ObjectId styleId = ObjectId.Null;

                            try
                            {
                                // SurfaceStyles[] indexer returns the ObjectId for the style
                                styleId = civDoc.Styles.SurfaceStyles[styleName];
                            }
                            catch
                            {
                                throw new System.Exception(
                                    $"Surface style '{styleName}' was not found in this drawing."
                                );
                            }

                            // -----------------------------------------------------------------
                            // 1) Create a new Civil 3D TIN surface in the drawing
                            // -----------------------------------------------------------------
                            ObjectId id = ADC.TinSurface.Create(name, styleId);

                            // 2) Get the surface object
                            ADC.TinSurface c3dSurface =
                                acTrans.GetObject(id, OpenMode.ForWrite) as ADC.TinSurface;

                            // -----------------------------------------------------------------
                            // 2) Add vertices first (2024+: one-by-one; bulk overloads removed)
                            // -----------------------------------------------------------------
                            foreach (var p in pts)
                                c3dSurface.AddVertex(p.ToCivil3D());

                            // -----------------------------------------------------------------
                            // 3) Build unique edges from triangles (triangles only; throw on quads)
                            // -----------------------------------------------------------------
                            var edges = new HashSet<(int u, int v)>();
                            foreach (var f in fcs)
                            {
                                if (f.D >= 0)
                                    throw new InvalidOperationException("Quad face encountered; only triangles allowed.");

                                AddEdge(f.A, f.B);
                                AddEdge(f.B, f.C);
                                AddEdge(f.C, f.A);
                            }

                            // -----------------------------------------------------------------
                            // 4) Create 3D polylines for each unique edge and collect their Ids
                            // -----------------------------------------------------------------
                            var breaklineIds = new ObjectIdCollection();

                            foreach (var (u, v) in edges)
                            {
                                // Guard index validity (skip bad faces quietly; or throw if you prefer)
                                if (u < 0 || v < 0 || u >= pts.Count || v >= pts.Count)
                                    continue;

                                Point3d pa = pts[u].ToCivil3D();
                                Point3d pb = pts[v].ToCivil3D();

                                // Create a simple 3D polyline with two vertices (A -> B)
                                var coll = new Point3dCollection();
                                coll.Add(pa);
                                coll.Add(pb);

                                var pl3d = new Polyline3d(Poly3dType.SimplePoly, coll, /*closed*/ false);
                                acBlkTblRec.AppendEntity(pl3d);
                                acTrans.AddNewlyCreatedDBObject(pl3d, true);

                                breaklineIds.Add(pl3d.ObjectId);
                            }

                            // -----------------------------------------------------------------
                            // 5) Register breaklines on the surface and rebuild
                            //    Signature variations exist; the 3-parameter distances call is widely supported.
                            //    
                            // -----------------------------------------------------------------
                            c3dSurface.BreaklinesDefinition.AddStandardBreaklines(
                                breaklineIds, 5.0, 0.25, 0.25, 0.087
                            );

                            c3dSurface.Rebuild();

                            // --- local helper: add undirected edge once ---
                            void AddEdge(int i, int j)
                            {
                                int a = Math.Min(i, j);
                                int b = Math.Max(i, j);
                                edges.Add((a, b)); // HashSet prevents duplicates
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.ToString());
                        List<string> stack = e.StackTrace.Split(new char[] { '\n' })
                            .ToList();

                        foreach (string s in stack)
                        {
                            System.Windows.Forms.MessageBox.Show(s);
                        }

                        System.Windows.Forms.MessageBox.Show("END");
                    }

                    acTrans.Commit();
                }
            }

            return true;
        }

        /***************************************************/


    }
}


