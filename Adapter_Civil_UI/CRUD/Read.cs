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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Dimensional;
using BHC = BH.oM.Civils.Elements;
using BHG = BH.oM.Geometry;
using BH.UI.Civil.Engine;
//using BH.oM.Geometry;
using BH.Engine;

using BH.oM.Data.Requests;

using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.Runtime;
using ADC = Autodesk.Civil.DatabaseServices;
using AAD = Autodesk.AutoCAD.DatabaseServices;
using ACG = Autodesk.AutoCAD.Geometry;
using BCF = BH.oM.Civils.Fragments;

//using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Internal.DatabaseServices;
//using Autodesk.AutoCAD.Geometry;

using BH.oM.Adapter;

namespace BH.UI.Civil.Adapter
{
    public partial class CivilUIAdapter
    {

        /***************************************************/
        /**** Adapter Methods                           ****/
        /***************************************************/

        //General method called by the adapter when reading in data

        protected override IEnumerable<IBHoMObject> Read(FilterRequest request, ActionConfig actionConfig = null)
        {
            try
            {

                Log("=== Read(FilterRequest) ENTER ===");
                
                // Prove which DLL is actually running
                Log("Assembly: " + typeof(CivilUIAdapter).Assembly.Location);


                if (request == null)
                
                {
                    Log("Request is NULL");
                    return new List<BH.oM.Base.IBHoMObject>();
                }


                Type requestedType = request.Type;
                Log("RequestedType: " + (requestedType == null ? "NULL" : requestedType.FullName));

                
                if (requestedType == null)
                    return new List<BH.oM.Base.IBHoMObject>();






                // ---- Civils ----
                if (requestedType == typeof(BHC.CivSurface))
                    return ReadTinSurface();

                if (requestedType == typeof(BHC.Alignment))
                    return ReadAlignments();

                if (requestedType == typeof(BHC.FeatureLine))
                    return ReadFeatureLines();



                // ---- Geometry ----
                if (requestedType == typeof(BHG.Point))
                    return ReadPoints();

                if (requestedType == typeof(BHG.ICurve))
                    return ReadCurves();
                    
                // ---- CAD ----
                if (requestedType == typeof(BHC.Block))
                    return ReadBlocks();

                // //----- Annotation
                if (requestedType == typeof(BHC.CadTextNote))
                    return ReadText();

                // ---- Fallback ----
                return new List<IBHoMObject>();
            }
            
            catch (Exception ex)
            {
                Log("EXCEPTION in Read(FilterRequest): " + ex);
                BH.Engine.Base.Compute.RecordError("Read(FilterRequest) failed: " + ex.Message);
                return new List<IBHoMObject>();
            }
            finally
            {
                Log("=== Read(FilterRequest) EXIT ===");
            }

        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/
        
        
        private static IBHoMObject WrapGeometry(BHG.IGeometry geom)
        {
            return new BH.oM.Base.CustomObject
            {
                Name = geom.GetType().Name,
                CustomData = new Dictionary<string, object>
                {
                    { "Geometry", geom }
                }
            };
        }


        private static readonly string DebugLogPath =
            System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CivilUIAdapter_Debug.txt");

        private static void Log(string msg)
        {
            try
            {
                System.IO.File.AppendAllText(
                    DebugLogPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {msg}{Environment.NewLine}");
            }
            catch { /* ignore */ }

            try
            {
                BH.Engine.Base.Compute.RecordNote(msg);
            }
            catch { /* ignore */ }
        }

        
        BHG.PolyCurve ToPolyCurve(BHG.Polyline pl)
        {
            return new BHG.PolyCurve
            {
                Curves = pl.ControlPoints
                    .Zip(pl.ControlPoints.Skip(1),
                        (a, b) => (BHG.ICurve)new BHG.Line { Start = a, End = b })
                    .ToList()
            };
            
        }


        /***************************************************/
        /**** CAD Text ****/
        /***************************************************/


        private List<BHC.CadTextNote> ReadText()
        {
            List<BHC.CadTextNote> textNotes = new List<BHC.CadTextNote>();

            var doc = Application.DocumentManager.MdiActiveDocument;
            var db  = doc.Database;

            using (Transaction trans = db.TransactionManager.StartOpenCloseTransaction())
            {
                var btr = (BlockTableRecord)trans.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(db),
                    OpenMode.ForRead);

                // Read mtext entities 
                foreach (ObjectId id in btr)
                {
                    Entity ent = trans.GetObject(id, OpenMode.ForRead) as Entity;

                    if (ent is MText mtext)
                    {
                        BHC.CadTextNote note = mtext.FromCivil3d();

                        if (note != null)
                        {
                            AttachLayerFragment(mtext, note);
                            textNotes.Add(note);                           
                        }
                    }
                }
            trans.Commit();
            }
        return textNotes;
        }


        /***************************************************/
        /**** CAD Points                         ****/
        /***************************************************/

        private List<IBHoMObject> ReadPoints()
        {
            List<IBHoMObject> points = new List<IBHoMObject>();

            var doc = Application.DocumentManager.MdiActiveDocument;
            var db  = doc.Database;


            using (Transaction trans = db.TransactionManager.StartOpenCloseTransaction())
            {
                var btr = (BlockTableRecord)trans.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(db),
                    OpenMode.ForRead);
                // Read ALL point entities (DBPoint, etc.)
                foreach (ObjectId id in btr)
                {
                    if (!id.ObjectClass.IsDerivedFrom(
                        Autodesk.AutoCAD.Runtime.RXClass.GetClass(typeof(AAD.DBPoint))))
                        continue;

                    var pointEnt = trans.GetObject(id, OpenMode.ForRead) as AAD.DBPoint;
                    if (pointEnt == null)
                        continue;
                    BHG.Point bhPoint = pointEnt.FromCivil3D();

                    if (bhPoint != null)
                    {
                        var bho = WrapGeometry(bhPoint);
                        if (bho != null)
                        {
                            AttachLayerFragment(pointEnt, bho);
                            points.Add(bho);
                        }
                    }
                }
            }
            return points;
        }



        /***************************************************/
        /**** CAD Curves                         ****/
        /***************************************************/


        private List<IBHoMObject> ReadCurves()
        {
            List<IBHoMObject> curves = new List<IBHoMObject>();

            var doc = Application.DocumentManager.MdiActiveDocument;
            var db  = doc.Database;

            using (Transaction trans = db.TransactionManager.StartOpenCloseTransaction())
            {
                var btr = (BlockTableRecord)trans.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(db),
                    OpenMode.ForRead);

                // Read ALL curve entities (Arc, Ellipse, etc.)
                foreach (ObjectId id in btr)
                {
                    if (!id.ObjectClass.IsDerivedFrom(
                        Autodesk.AutoCAD.Runtime.RXClass.GetClass(typeof(AAD.Curve))))
                        continue;

                    var curveEnt = trans.GetObject(id, OpenMode.ForRead) as AAD.Curve;
                    if (curveEnt == null)
                        continue;

                    BHG.IGeometry bh = null;

                    if (curveEnt is AAD.Line line)
                    {
                        bh = line.FromCivil3D();
                    }
                    else if (curveEnt is AAD.Arc arc)
                    {
                        bh = arc.FromCivil3D();
                    }
                    else if (curveEnt is AAD.Circle circle)
                    {
                        bh = circle.FromCivil3D();
                    }
                    else if (curveEnt is AAD.Ellipse ellipse)
                    {
                        bh = ellipse.FromCivil3D();
                    }
                    else if (curveEnt is AAD.Polyline polyline)
                    {
                        bh = polyline.FromCivil3D();
                    }

                    // else if (curveEnt is AAD.Spline spline)
                    // {
                    //     bh = spline.FromCivil3D();
                    // }
                    else
                    {

                        Log("Curve Type not supported");
                        
                        // Fallback to geometry curve for unsupported types
                        // using (var ge = curveEnt.GetGeCurve())
                        // {
                        //     if (ge is ACG.CompositeCurve3d cc)
                        //         bh = cc.FromCivil3D();
                        //     else if (ge is ACG.PolylineCurve3d plc)
                        //         bh = ToPolyCurve(plc.FromCivil3D());
                        //     else if (ge is ACG.CircularArc3d circ)
                        //         bh = circ.FromCivil3D();
                        //     else if (ge is ACG.EllipticalArc3d ell)
                        //         bh = ell.FromCivil3D();
                        //     else if (ge is ACG.NurbCurve3d nurb)
                        //         bh = nurb.FromCivil3D();
                        // }
                    }

                    if (bh != null)
                    {
                        var bho = WrapGeometry(bh);
                        if (bho != null)
                        {
                            AttachLayerFragment(curveEnt, bho);
                            curves.Add(bho);
                        }
                    }
                }

                trans.Commit();
            }

            return curves;
        }


        /***************************************************/
        /**** Civils 3d Objects                         ****/
        /***************************************************/

        private List<BHC.FeatureLine> ReadFeatureLines()
        {
            List<BHC.FeatureLine> featureLines = new List<BHC.FeatureLine>();

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartOpenCloseTransaction())
            {
                var btr =
                     (BlockTableRecord)trans.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(Application.DocumentManager.MdiActiveDocument.Database),
                    OpenMode.ForRead
                     );

                var blockIDs =
                from ObjectId id in btr
                where id.ObjectClass.IsDerivedFrom(Autodesk.AutoCAD.Runtime.RXClass.GetClass(typeof(ADC.FeatureLine)))
                select id;

                foreach (ObjectId id in blockIDs)
                {
                    ADC.FeatureLine l = (trans.GetObject(id, OpenMode.ForRead) as ADC.FeatureLine);
                    if (l != null)
                        featureLines.Add(l.FromCivil3D());
                }

                trans.Commit();
            }

            return featureLines;
        }

        private List<BHC.Block> ReadBlocks()
        {
            List<BHC.Block> blocks = new List<BHC.Block>();

            var doc = Application.DocumentManager.MdiActiveDocument;
            var db  = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                foreach (ObjectId id in ms)
                {                 
                    if (!id.ObjectClass.IsDerivedFrom(
                            Autodesk.AutoCAD.Runtime.RXClass.GetClass(typeof(BlockReference))))
                        continue;

                    BlockReference br =
                        tr.GetObject(id, OpenMode.ForRead) as BlockReference;

                    BHC.Block bho = null;
                    bho = br.FromCivil3D();

                    if (bho != null)
                    {
                        AttachLayerFragment(br, bho);
                        blocks.Add(bho);
                    }

                }
            }
            return blocks;
        }

        private List<BHC.Alignment> ReadAlignments()
        {
            CivilDocument doc = CivilApplication.ActiveDocument;

            List<BHC.Alignment> alignments = new List<BHC.Alignment>();

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in doc.GetAlignmentIds())
                {
                    ADC.Alignment alignment = id.GetObject(OpenMode.ForRead) as ADC.Alignment;
                    alignments.Add(alignment.FromCivil3D());
                }
            }

            return alignments;
        }

        private List<BHC.Parcel> ReadParcels()
        {
            CivilDocument doc = CivilApplication.ActiveDocument;

            List<BHC.Parcel> parcels = new List<BHC.Parcel>();

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartOpenCloseTransaction())
            {
                var btr =
                     (BlockTableRecord)trans.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(Application.DocumentManager.MdiActiveDocument.Database),
                    OpenMode.ForRead
                     );
                var parcelIDs =
                from ObjectId id in btr
                where id.ObjectClass.IsDerivedFrom(Autodesk.AutoCAD.Runtime.RXClass.GetClass(typeof(ADC.Parcel)))
                select id;

                foreach (ObjectId id in parcelIDs)
                {
                    ADC.Parcel p = (trans.GetObject(id, OpenMode.ForRead) as ADC.Parcel);
                    if (p != null)
                        parcels.Add(p.FromCivil3D());
                }

                trans.Commit();
            }

            return parcels;
        }

        private List<BHC.CivProfile> ReadProfiles()
        {
            CivilDocument doc = CivilApplication.ActiveDocument;

            List<BHC.CivProfile> profiles = new List<BHC.CivProfile>();

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartOpenCloseTransaction())
            {
                var btr =
                     (BlockTableRecord)trans.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(Application.DocumentManager.MdiActiveDocument.Database),
                    OpenMode.ForRead
                     );
                var featureIDs =
                from ObjectId id in btr
                where id.ObjectClass.IsDerivedFrom(Autodesk.AutoCAD.Runtime.RXClass.GetClass(typeof(ADC.Profile)))
                select id;

                foreach (ObjectId id in featureIDs)
                {
                    ADC.Profile p = (trans.GetObject(id, OpenMode.ForRead) as ADC.Profile);
                    if (p != null)
                        profiles.Add(p.FromCivil3D());
                }

                trans.Commit();
            }

            return profiles;
        }

        private List<BHC.CoGoPoint> ReadCoGoPoints()
        {
            CivilDocument doc = CivilApplication.ActiveDocument;

            List<BHC.CoGoPoint> pnts = new List<BHC.CoGoPoint>();

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in doc.GetAllPointIds())
                {
                    ADC.CogoPoint pnt = id.GetObject(OpenMode.ForRead) as ADC.CogoPoint;
                    pnts.Add(pnt.FromCivil3D());
                }
            }

            return pnts;
        }

        private List<BHC.Pipe> ReadPipes()
        {
            CivilDocument doc = CivilApplication.ActiveDocument;

            List<BHC.Pipe> pipeList = new List<BHC.Pipe>();

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in doc.GetPipeNetworkIds())
                {
                    ADC.Network network = trans.GetObject(id, OpenMode.ForRead) as ADC.Network;
                    foreach (ObjectId pipeId in network.GetPipeIds())
                    {
                        ADC.Pipe pipe = trans.GetObject(pipeId, OpenMode.ForRead) as ADC.Pipe;
                        BHC.Pipe bhPipe = pipe.ToBHoM();
                        pipeList.Add(bhPipe);
                    }

                }

                trans.Commit();
            }

            return pipeList;
        }

        private List<BHC.ManholeChamber> ReadHoles()
        {
            CivilDocument doc = CivilApplication.ActiveDocument;

            List<BHC.ManholeChamber> manholeChamberList = new List<BHC.ManholeChamber>();

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in doc.GetPipeNetworkIds())
                {
                    ADC.Network network = trans.GetObject(id, OpenMode.ForRead) as ADC.Network;
                    foreach (ObjectId pipeId in network.GetStructureIds())
                    {
                        ADC.Structure manholeChamber = trans.GetObject(pipeId, OpenMode.ForRead) as ADC.Structure;
                        BHC.ManholeChamber bhManholeChamber = manholeChamber.ToBHoM();
                        manholeChamberList.Add(bhManholeChamber);
                    }

                }

                trans.Commit();
            }

            return manholeChamberList;
        }

        private List<BHC.CivSurface> ReadTinSurface()
        {   
            CivilDocument doc = CivilApplication.ActiveDocument;

            var tinSurfaceList = new List<BHC.CivSurface>();

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in doc.GetSurfaceIds())
                {
                    var tinSurface = trans.GetObject(id, OpenMode.ForRead) as ADC.TinSurface;
                    if (tinSurface != null)
                    {
                        tinSurfaceList.Add(tinSurface.ToBHoM()); // returns BHC.CivSurface
                    }
                }
                trans.Commit();
            }
            return tinSurfaceList;
        }

        /***************************************************/
        // Helper method to attach layer information as a fragment to the BHoM object
        /***************************************************/
    
        private static void AttachLayerFragment(AAD.Entity entity, IBHoMObject bhObj)
        {
            if (entity == null || bhObj == null)
                return;

            var layerName = entity.Layer;

            if (!string.IsNullOrWhiteSpace(layerName))
            {
                bhObj.Fragments.Add(new BCF.CADLayerFragment
                {
                    CADLayerName = layerName
                });
            }
        }

    }
}


