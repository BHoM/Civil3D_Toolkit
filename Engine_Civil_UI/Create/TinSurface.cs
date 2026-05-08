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


using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.Runtime;
using ADC = Autodesk.Civil.DatabaseServices;
using BH.UI.Civil.Engine;
using BH.oM.Adapters.Civil3D;

using Autodesk.AutoCAD.EditorInput;


namespace BH.UI.Civil.Engine
{
    public static partial class Create
    {
        public static bool InCivil3D(this BHC.CivSurface s, Civil3DRuntimeContext context, string layer = "0")
        {
            {
                CivilDocument civDoc = context.CivilDocument;
                Transaction acTrans = context.Transaction;
                BlockTableRecord acBlkTblRec = context.BlockTableRecord;

                var pts  = s.Mesh.Vertices;
                var fcs  = s.Mesh.Faces;
                var name = string.IsNullOrEmpty(s.Name) ? "CivSurface_Mesh" : s.Name;

                string styleName = "Standard";
                
                // Resolve surface style
                ObjectId styleId = ObjectId.Null;

                try
                {
                    styleId = civDoc.Styles.SurfaceStyles[styleName];
                }
                catch
                {
                    throw new System.Exception(
                        $"Surface style '{styleName}' was not found in this drawing.");
                }

                // ----------------------------------------------------------
                // 1) Create new TIN Surface
                // ----------------------------------------------------------
                ObjectId id = ADC.TinSurface.Create(name, styleId);
                ADC.TinSurface c3dSurface = (ADC.TinSurface)acTrans.GetObject(id, OpenMode.ForWrite);

                // ----------------------------------------------------------
                // 2) Add vertices
                // ----------------------------------------------------------
                foreach (var p in pts)
                    c3dSurface.AddVertex(p.ToACGPoint2d());
                
                // // ----------------------------------------------------------
                // // Create one 3D polyline per face
                // // ----------------------------------------------------------
                var breaklineIds = new ObjectIdCollection();

                foreach (var f in fcs)
                {
                    // Validate indices
                    if (f.A < 0 || f.B < 0 || f.C < 0 || f.A >= pts.Count || f.B >= pts.Count || f.C >= pts.Count)
                        continue;

                    var facepts = BH.UI.Civil.Engine.Convert.PolylineFromFace(f, pts).ControlPoints;
                    var C3Dfacepts = new Point3dCollection();

                    foreach (var pt in facepts)
                    {
                        C3Dfacepts.Add(BH.UI.Civil.Engine.Convert.ToACGPoint3d(pt));
                    }

                    var pl3d = new Polyline3d(Poly3dType.SimplePoly, C3Dfacepts, /* closed */ true);

                    acBlkTblRec.AppendEntity(pl3d);
                    acTrans.AddNewlyCreatedDBObject(pl3d, true);
                    breaklineIds.Add(pl3d.ObjectId);
                }

                // ----------------------------------------------------------
                // 5) Add breaklines + rebuild
                // ----------------------------------------------------------
                c3dSurface.BreaklinesDefinition.AddStandardBreaklines(breaklineIds, 5.0, 0.25, 0.25, 0.087);
                c3dSurface.Rebuild();

                // ----------------------------------------------------------
                // 6) Assign to layer
                // ----------------------------------------------------------
                c3dSurface.Layer = layer;
            }

            return true;
        }
    }
}




