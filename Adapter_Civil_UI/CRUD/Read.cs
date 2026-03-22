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




        protected override IEnumerable<IBHoMObject> IRead(Type type, IList ids, ActionConfig actionConfig = null)
        {

            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
                ed.WriteMessage($"\n[IRead] Requested type: {type.FullName}");

            try
            {

                if (type == typeof(BHC.Pipe))
                {
                    return ReadPipes();
                }

                if (type == typeof(BHC.ManholeChamber))
                {
                    return ReadHoles();
                }

                if (type == typeof(BHC.CivSurface))
                {
                    return ReadTinSurface();
                }

                if (type == typeof(BHC.CoGoPoint))
                {
                    return ReadCoGoPoints();
                }

                /* if (type == typeof(BHC.CivProfile))
                  {
                      return ReadProfiles();
                  } */

                if (type == typeof(BHC.Parcel))
                {
                    return ReadParcels();
                }

                if (type == typeof(BHC.Alignment))
                {
                    return ReadAlignments();
                }

                if (type == typeof(BHC.Block))
                    return ReadBlocks();

                if (type == typeof(BHC.FeatureLine))
                    return ReadFeatureLines();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                List<string> stack = e.StackTrace.Split(new char[] { '\n' })
                    .Where(x => x.Contains(" BH."))
                    .ToList();

                foreach (string s in stack)
                {
                    System.Windows.Forms.MessageBox.Show(s);
                }

                System.Windows.Forms.MessageBox.Show("END");

                throw e;
            }

            return new List<IBHoMObject>();
        }




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
                {
                    Log("Branch: CivSurface");
                    var r = ReadTinSurface().Cast<IBHoMObject>().ToList();
                    Log("Returned CivSurface count: " + r.Count);
                    return r;
                }
                if (requestedType == typeof(BHC.Alignment))
                    return ReadAlignments().Cast<IBHoMObject>().ToList();

                if (requestedType == typeof(BHC.FeatureLine))
                    return ReadFeatureLines().Cast<IBHoMObject>().ToList();

                // ---- Geometry ----

                if (requestedType == typeof(BHG.Line))
                {
                    Log("Branch: Line");

                    var raw = ReadLines();
                    Log("Returned Line count (raw): " + raw.Count);

                    var result = new List<IBHoMObject>();

                    foreach (var ln in raw)
                    {
                        result.Add(WrapGeometry(ln));
                    }

                    Log("Returned Line count (wrapped): " + result.Count);
                    return result;
                }

                if (requestedType == typeof(BHG.PolyCurve) ||
                    requestedType == typeof(BHG.Polyline))
                {
                    Log("Branch: PolyCurve / Polyline");

                    var raw = ReadPolyLines();
                    Log("Returned PolyCurve count (raw): " + raw.Count);

                    var result = new List<IBHoMObject>();

                    foreach (var ln in raw)
                    {
                        result.Add(WrapGeometry(ln));
                    }

                    Log("Returned PolyCurve count (wrapped): " + result.Count);
                    return result;
                }

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


        /**** Geometry 3d Objects                       ****/
        private List<BHG.Line> ReadLines(List<string> ids = null)
        {

            Log("Read lines initiated...");

            List<BHG.Line> lines = new List<BHG.Line>();

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartOpenCloseTransaction())
            {
                var btr =
                     (BlockTableRecord)trans.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(Application.DocumentManager.MdiActiveDocument.Database),
                    OpenMode.ForRead
                     );

                var blockIDs =
                from ObjectId id in btr
                where id.ObjectClass.IsDerivedFrom(Autodesk.AutoCAD.Runtime.RXClass.GetClass(typeof(AAD.Line)))
                select id;

                var ed = Application.DocumentManager.MdiActiveDocument.Editor;


                var idsList = blockIDs.ToList();
                Log($"[ReadLines] Found {idsList.Count} AAD.Line entities");

                foreach (ObjectId id in blockIDs)
                {
                    // Get the AutoCAD Line entity
                    var l = trans.GetObject(id, OpenMode.ForRead) as AAD.Line;
                    if (l == null)
                        continue;

                    // Get and dispose of the geometric curve properly
                    using (var ge = l.GetGeCurve())
                    {
                        Log($"GeCurve type: {ge?.GetType().FullName}");

                        // Normal AutoCAD lines return LineSegment3d
  
                        var geomLine = ge as ACG.LineSegment3d;
                        if (geomLine == null)
                            continue;

                        // Convert to BHoM geometry
                        var bhLine = geomLine.FromCivil3D();
                        if (bhLine == null)
                        {
                            Log("FromCivil3D returned null");
                            continue;
                        }
                        lines.Add(bhLine);
                    }
                }

                trans.Commit();
            }
            return lines;
        }
        


        
        private List<BHG.PolyCurve> ReadPolyLines()
        {
            List<BHG.PolyCurve> polyCurves = new List<BHG.PolyCurve>();

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartOpenCloseTransaction())
            {
                var btr =
                     (BlockTableRecord)trans.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(Application.DocumentManager.MdiActiveDocument.Database),
                    OpenMode.ForRead
                     );

                var blockIDs =
                from ObjectId id in btr
                where id.ObjectClass.IsDerivedFrom(Autodesk.AutoCAD.Runtime.RXClass.GetClass(typeof(AAD.Polyline)))
                select id;

                foreach (ObjectId id in blockIDs)
                {
                    var pl = trans.GetObject(id, OpenMode.ForRead) as Polyline;
                        if (pl == null)
                            continue;

                    ACG.Curve3d acCurve = pl.GetGeCurve();

                    // ---------- ROUTE BY AUTOCAD GEOMETRY TYPE ----------

                    // 1. Composite curve (most common when arcs are present)
                    if (acCurve is ACG.CompositeCurve3d cc)
                    {
                        var bh = cc.FromCivil3D() as BHG.PolyCurve;
                        if (bh != null)
                            polyCurves.Add(bh);
                    }

                    // 2. PolylineCurve3d (purely linear)
                    else if (acCurve is ACG.PolylineCurve3d plc)
                    {
                        var bhPl = plc.FromCivil3D() as BHG.Polyline;
                        var bhPc = ToPolyCurve(bhPl);
                        if (bhPc != null)
                            polyCurves.Add(bhPc);
                    }

                    // 3. Fallback: sample any other Curve3d
                    else
                    {
                        continue;
                    }
                }
                trans.Commit();
            }
            return polyCurves;
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


        /**** Civils 3d Objects                         ****/
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

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartOpenCloseTransaction())
            {
                var btr =
                     (BlockTableRecord)trans.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(Application.DocumentManager.MdiActiveDocument.Database),
                    OpenMode.ForRead
                     );
                var blockIDs =
                from ObjectId id in btr
                where id.ObjectClass.IsDerivedFrom(Autodesk.AutoCAD.Runtime.RXClass.GetClass(typeof(BlockReference)))
                select id;

                foreach (ObjectId id in blockIDs)
                {
                    BlockReference b = (trans.GetObject(id, OpenMode.ForRead) as BlockReference);
                    if (b != null)
                        blocks.Add(b.FromCivil3D());
                }

                trans.Commit();
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

        private List<BHC.Pipe> ReadPipes(List<string> ids = null)
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

        private List<BHC.ManholeChamber> ReadHoles(List<string> ids = null)
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

        private List<BHC.CivSurface> ReadTinSurface(List<string> ids = null)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage($"\n ReadTinSurface initiated...");
            
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

    }
}


