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
using BH.oM.Civils.Fragments;
using BH.Engine.Base;

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
using Autodesk.AutoCAD.Geometry;

namespace BH.UI.Civil.Adapter
{

    public class C3DReader
    {

        public IEnumerable<IBHoMObject> ReadByType(
            Type requestedType,
            Database db,
            CivilDocument civdoc)
        {
            var results = new List<IBHoMObject>();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                results.AddRange(
                    ReadCAD(requestedType, db, tr)
                );

                if (civdoc != null)
                {
                    results.AddRange(
                        ReadC3DCAD(requestedType, db, tr, civdoc)
                    );
                }

                tr.Commit();
            }

            return results;
        }


        public IEnumerable<IBHoMObject> ReadCAD(
            Type requestedType,
            Database db,
            Transaction tr)
        {
            BlockTable bt =
                (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

            BlockTableRecord btr =
                (BlockTableRecord)tr.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(db),
                    OpenMode.ForRead);

            if (requestedType == typeof(BHC.CadTextNote))
                return ReadText(tr, btr);

            if (requestedType == typeof(BHG.Point))
                return ReadPoints(tr, btr);

            if (requestedType == typeof(BHG.ICurve))
                return ReadCurves(tr, btr);

            if (requestedType == typeof(BHC.BlockInstance))
                return ReadBlocks(tr, db, bt);

            if (requestedType == typeof(BHC.BlockDefinition))
                return ReadBlockDefs(tr, db, bt);

            return Enumerable.Empty<IBHoMObject>();
        }   

        public IEnumerable<IBHoMObject> ReadC3DCAD(
            Type requestedType,
            Database db,
            Transaction tr,
            CivilDocument civdoc)
        {
            if (civdoc == null)
                return Enumerable.Empty<IBHoMObject>();

            if (requestedType == typeof(BHC.CoGoPoint))
                return ReadCoGoPoints(civdoc);

            if (requestedType == typeof(BHC.Pipe))
                return ReadPipes(civdoc, tr);

            if (requestedType == typeof(BHC.ManholeChamber))
                return ReadPipeNetworkStructures(civdoc, tr);

            if (requestedType == typeof(BHC.PressureNetworkFitting))
                return ReadPressurePipeNetworkFitting(civdoc, tr);

            if (requestedType == typeof(BHC.PressureNetworkAppurtenance))
                return ReadPressurePipeNetworkAppurtenance(civdoc, tr);

            if (requestedType == typeof(BHC.CivSurface))
                return ReadTinSurface(civdoc, tr);

            return Enumerable.Empty<IBHoMObject>();
        }




        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/
        



        /***************************************************/
        /**** CAD Text ****/
        /***************************************************/


        private List<BHC.CadTextNote> ReadText(Transaction tr, BlockTableRecord btr)
        {
            List<BHC.CadTextNote> textNotes = new List<BHC.CadTextNote>();

            // Read mtext entities 
            foreach (ObjectId id in btr)
            {
                Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;

                if (ent is MText mtext)
                {
                    BHC.CadTextNote note = mtext.FromCivil3d();

                    if (note != null)
                    {
                        AttachCADDataFragment(mtext, note);
                        textNotes.Add(note);                           
                    }
                }
            }

        return textNotes;
        }


        /***************************************************/
        /**** CAD Points                         ****/
        /***************************************************/

        private List<IBHoMObject> ReadPoints(Transaction tr, BlockTableRecord btr)
        {
            List<IBHoMObject> points = new List<IBHoMObject>();

            // Read ALL point entities (DBPoint, etc.)

            foreach (ObjectId id in btr)
            {
                var pointEnt = tr.GetObject(id, OpenMode.ForRead) as AAD.DBPoint;
                if (pointEnt == null)
                    continue;

                var bhPoint = pointEnt.FromCivil3D();
                if (bhPoint == null)
                    continue;

                var bho = WrapGeometry(bhPoint);
                if (bho == null)
                    continue;

                AttachCADDataFragment(pointEnt, bho);
                points.Add(bho);
            }
            
            return points;
        }



        /***************************************************/
        /**** CAD Curves                         ****/
        /***************************************************/


        private List<IBHoMObject> ReadCurves(
            Transaction tr,
            BlockTableRecord btr,
            IEnumerable<AAD.Curve> sourceCurves = null)
            
        {
            List<IBHoMObject> curves = new List<IBHoMObject>();

            IEnumerable<AAD.Curve> acadCurves;

            // ✅ Use provided subset if available
            if (sourceCurves != null)
            {
                acadCurves = sourceCurves;
            }
            // ✅ Otherwise fall back to scanning the whole BTR (original behaviour)
            else
            {
                List<AAD.Curve> allCurves = new List<AAD.Curve>();

                foreach (ObjectId id in btr)
                {
                    if (!id.ObjectClass.IsDerivedFrom(
                        Autodesk.AutoCAD.Runtime.RXClass.GetClass(typeof(AAD.Curve))))
                        continue;

                    var curveEnt = tr.GetObject(id, OpenMode.ForRead) as AAD.Curve;
                    if (curveEnt != null)
                        allCurves.Add(curveEnt);
                }

                acadCurves = allCurves;
            }

            // ✅ Convert only the selected curves
            foreach (AAD.Curve curveEnt in acadCurves)
            {
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
                else if (curveEnt is AAD.Polyline2d polyline2d)
                {
                    bh = polyline2d.FromCivil3D();
                }
                else if (curveEnt is AAD.Polyline3d polyline3d)
                {
                    bh = polyline3d.FromCivil3D();
                }
                else if (curveEnt is AAD.Spline spline)
                {
                    bh = spline.FromCivil3D();
                }

                else
                {
                    Debugger.Log($"Curve Type not supported: {curveEnt.GetType().Name}");
                }

                if (bh != null)
                {
                    var bho = WrapGeometry(bh);
                    if (bho != null)
                    {
                        AttachCADDataFragment(curveEnt, bho);
                        curves.Add(bho);
                    }
                }
            }

            return curves;
        }


        /***************************************************/
        /**** CAD Blocks                         ****/
        /***************************************************/
        private List<BHC.BlockInstance> ReadBlocks(Transaction tr, Database db, BlockTable bt)
        {
            List<BHC.BlockInstance> blocks = new List<BHC.BlockInstance>();
              
            BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

            foreach (ObjectId id in ms)
            {
                if (!(tr.GetObject(id, OpenMode.ForRead) is BlockReference br))
                    continue;

                var bho = br.FromCivil3D(tr);
                if (bho == null)
                    continue;

                AttachCADDataFragment(br, bho);
                blocks.Add(bho);
            }
            return blocks;
        }

        private List<BHC.BlockDefinition> ReadBlockDefs(Transaction tr, Database db, BlockTable bt)
        {
            var defs = new List<BHC.BlockDefinition>();

            foreach (ObjectId btrId in bt)
            {
                var btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);

                if (btr.IsLayout || btr.Name.StartsWith("*"))
                    continue;

                var bhomDef = new BHC.BlockDefinition
                {
                    Name = btr.Name,
                    Geometry = new List<BHG.IGeometry>()
                };

                ReadBlockGeometryRecursive(
                    tr,
                    btr,
                    bhomDef.Geometry);

                defs.Add(bhomDef);
            }

            return defs;
        }


        private void ReadBlockGeometryRecursive(
            Transaction tr,
            BlockTableRecord btr,
            IList<BHG.IGeometry> output)
        {
            foreach (ObjectId id in btr)
            {
                if (!(tr.GetObject(id, OpenMode.ForRead) is Entity ent))
                    continue;

                // Nested blocks stay as BlockReferences
                if (ent is BlockReference)
                    continue;

                var bho = ent.FromCivil3D();
                if (bho is BHG.IGeometry geom)
                    output.Add(geom);
            }
        }



        /***************************************************/
        /**** Civils 3d Objects                         ****/
        /***************************************************/

        private List<BHC.FeatureLine> ReadFeatureLines(Transaction tr, BlockTableRecord btr)
        {
            List<BHC.FeatureLine> featureLines = new List<BHC.FeatureLine>();

            var blockIDs =
            from ObjectId id in btr
            where id.ObjectClass.IsDerivedFrom(Autodesk.AutoCAD.Runtime.RXClass.GetClass(typeof(ADC.FeatureLine)))
            select id;

            foreach (ObjectId id in blockIDs)
            {
                ADC.FeatureLine l = (tr.GetObject(id, OpenMode.ForRead) as ADC.FeatureLine);
                if (l != null)
                    featureLines.Add(l.FromCivil3D());
            }

            return featureLines;
        }

        private List<BHC.Alignment> ReadAlignments(CivilDocument civdoc)
        {
            List<BHC.Alignment> alignments = new List<BHC.Alignment>();

                foreach (ObjectId id in civdoc.GetAlignmentIds())
                {
                    ADC.Alignment alignment = id.GetObject(OpenMode.ForRead) as ADC.Alignment;
                    alignments.Add(alignment.FromCivil3D());
                }
            return alignments;
        }

        private List<BHC.Parcel> ReadParcels(Transaction tr, BlockTableRecord btr)
        {
            List<BHC.Parcel> parcels = new List<BHC.Parcel>();

            var parcelIDs =
            from ObjectId id in btr
            where id.ObjectClass.IsDerivedFrom(Autodesk.AutoCAD.Runtime.RXClass.GetClass(typeof(ADC.Parcel)))
            select id;

            foreach (ObjectId id in parcelIDs)
            {
                ADC.Parcel p = (tr.GetObject(id, OpenMode.ForRead) as ADC.Parcel);
                if (p != null)
                    parcels.Add(p.FromCivil3D());
            }
            return parcels;
        }

        private List<BHC.CivProfile> ReadProfiles(Transaction tr, BlockTableRecord btr)
        {
            List<BHC.CivProfile> profiles = new List<BHC.CivProfile>();

            var featureIDs =
            from ObjectId id in btr
            where id.ObjectClass.IsDerivedFrom(Autodesk.AutoCAD.Runtime.RXClass.GetClass(typeof(ADC.Profile)))
            select id;

            foreach (ObjectId id in featureIDs)
            {
                ADC.Profile p = (tr.GetObject(id, OpenMode.ForRead) as ADC.Profile);
                if (p != null)
                    profiles.Add(p.FromCivil3D());
            }
            return profiles;
        }

        private List<BHC.CoGoPoint> ReadCoGoPoints(CivilDocument civdoc)
        {
            List<BHC.CoGoPoint> pnts = new List<BHC.CoGoPoint>();

            foreach (ObjectId id in civdoc.GetAllPointIds())
            {
                ADC.CogoPoint pnt = id.GetObject(OpenMode.ForRead) as ADC.CogoPoint;
                pnts.Add(pnt.FromCivil3D());
            }
            return pnts;
        }

        private List<BHC.Pipe> ReadPipes(CivilDocument civdoc, Transaction tr)
        {
            List<BHC.Pipe> pipeList = new List<BHC.Pipe>();

            // --------------------------------------------------
            // Gravity Pipe Networks (with structures)
            // --------------------------------------------------
            foreach (ObjectId id in civdoc.GetPipeNetworkIds())
            {
                ADC.Network network =
                    tr.GetObject(id, OpenMode.ForRead) as ADC.Network;

                if (network == null)
                    continue;

                foreach (ObjectId pipeId in network.GetPipeIds())
                {
                    ADC.Pipe pipe =
                        tr.GetObject(pipeId, OpenMode.ForRead) as ADC.Pipe;

                    if (pipe == null)
                        continue;

                    BHC.Pipe bhPipe = pipe.ToBHoM(tr);

                    bhPipe = AddPipeConnectivityFragments(bhPipe, pipe, tr);

                    pipeList.Add(bhPipe);
                }
            }

            // --------------------------------------------------
            // Pressure Pipe Networks (no structures)
            // --------------------------------------------------
            foreach (ObjectId id in civdoc.GetPressurePipeNetworkIds())
            {
                ADC.PressurePipeNetwork network =
                    tr.GetObject(id, OpenMode.ForRead) as ADC.PressurePipeNetwork;

                if (network == null)
                    continue;

                foreach (ObjectId pipeId in network.GetPipeIds())
                {
                    ADC.PressurePipe pipe =
                        tr.GetObject(pipeId, OpenMode.ForRead) as ADC.PressurePipe;

                    if (pipe == null)
                        continue;

                    BHC.Pipe bhPipe = pipe.ToBHoM(tr);

                    pipeList.Add(bhPipe);
                }
            }

            return pipeList;
        }


        private BHC.Pipe AddPipeConnectivityFragments(
            BHC.Pipe bhPipe,
            ADC.Pipe pipe,
            Transaction tr)
        {
            if (bhPipe == null || pipe == null || tr == null)
                return bhPipe;

            ADC.Structure startStructure = null;
            ADC.Structure endStructure   = null;

            // Safely resolve structures (pipes may be partially connected)
            if (!pipe.StartStructureId.IsNull)
            {
                startStructure =
                    tr.GetObject(pipe.StartStructureId, OpenMode.ForRead, false)
                        as ADC.Structure;
            }

            if (!pipe.EndStructureId.IsNull)
            {
                endStructure =
                    tr.GetObject(pipe.EndStructureId, OpenMode.ForRead, false)
                        as ADC.Structure;
            }

            double startInvert = double.NaN;
            double endInvert   = double.NaN;

            // Use InnerDiameterOrWidth (as requested)
            if (pipe.InnerDiameterOrWidth > 0)
            {
                startInvert =
                    pipe.StartPoint.Z - pipe.InnerDiameterOrWidth / 2.0;

                endInvert =
                    pipe.EndPoint.Z - pipe.InnerDiameterOrWidth / 2.0;
            }

            ADC.Structure upstreamStruct   = null;
            ADC.Structure downstreamStruct = null;

            if (!double.IsNaN(startInvert) && !double.IsNaN(endInvert))
            {
                if (startInvert > endInvert)
                {
                    upstreamStruct   = startStructure;
                    downstreamStruct = endStructure;
                }
                else if (endInvert > startInvert)
                {
                    upstreamStruct   = endStructure;
                    downstreamStruct = startStructure;
                }
                else
                {
                    // Flat pipe – retain declared order
                    upstreamStruct   = startStructure;
                    downstreamStruct = endStructure;
                }
            }
            else
            {
                // Fallback: infer from availability only
                upstreamStruct   = startStructure;
                downstreamStruct = endStructure;
            }

            // ✅ Always add a fragment once evaluated
            bhPipe.AddFragment(
                new PipeConnectivityFragment
                {
                    UpstreamStructureName   = upstreamStruct?.Name,
                    DownstreamStructureName = downstreamStruct?.Name
                });

            return bhPipe;
        }

        private List<BHC.ManholeChamber> ReadPipeNetworkStructures(CivilDocument civdoc, Transaction tr)
        {
            List<BHC.ManholeChamber> manholeChamberList = new List<BHC.ManholeChamber>();

            foreach (ObjectId id in civdoc.GetPipeNetworkIds())
            {
                ADC.Network network = tr.GetObject(id, OpenMode.ForRead) as ADC.Network;
                foreach (ObjectId pipeId in network.GetStructureIds())
                {
                    ADC.Structure manholeChamber = tr.GetObject(pipeId, OpenMode.ForRead) as ADC.Structure;
                    BHC.ManholeChamber bhManholeChamber = manholeChamber.ToBHoM();
                    manholeChamberList.Add(bhManholeChamber);
                }
            }
            return manholeChamberList;
        }

        private List<BHC.PressureNetworkFitting> ReadPressurePipeNetworkFitting(CivilDocument civdoc, Transaction tr)
        {
            List<BHC.PressureNetworkFitting> fittingList = new List<BHC.PressureNetworkFitting>();

            foreach (ObjectId id in civdoc.GetPressurePipeNetworkIds())
            {
                ADC.PressurePipeNetwork network = tr.GetObject(id, OpenMode.ForRead) as ADC.PressurePipeNetwork;
                foreach (ObjectId fitId in network.GetFittingIds())
                {
                    ADC.PressureFitting pressureFitting = tr.GetObject(fitId, OpenMode.ForRead) as ADC.PressureFitting;
                    BHC.PressureNetworkFitting bhpressureFitting = pressureFitting.ToBHoM();
                    fittingList.Add(bhpressureFitting);
                }
            }
            return fittingList;
        }

        private List<BHC.PressureNetworkAppurtenance> ReadPressurePipeNetworkAppurtenance(CivilDocument civdoc, Transaction tr)
        {
            List<BHC.PressureNetworkAppurtenance> appurtenanceList = new List<BHC.PressureNetworkAppurtenance>();

            foreach (ObjectId id in civdoc.GetPressurePipeNetworkIds())
            {
                ADC.PressurePipeNetwork network = tr.GetObject(id, OpenMode.ForRead) as ADC.PressurePipeNetwork;
                foreach (ObjectId fitId in network.GetAppurtenanceIds())
                {
                    ADC.PressureAppurtenance pressureAppurtenance = tr.GetObject(fitId, OpenMode.ForRead) as ADC.PressureAppurtenance;
                    BHC.PressureNetworkAppurtenance bhpressureAppurtenance = pressureAppurtenance.ToBHoM();
                    appurtenanceList.Add(bhpressureAppurtenance);
                }
            }
            return appurtenanceList;
        }

        private List<BHC.CivSurface> ReadTinSurface(CivilDocument civdoc, Transaction tr)
        {   
            var tinSurfaceList = new List<BHC.CivSurface>();

            foreach (ObjectId id in civdoc.GetSurfaceIds())
            {
                var tinSurface = tr.GetObject(id, OpenMode.ForRead) as ADC.TinSurface;
                if (tinSurface != null)
                {
                    tinSurfaceList.Add(tinSurface.ToBHoM()); // returns BHC.CivSurface
                }
            }
            return tinSurfaceList;
        }






        /***************************************************/
        // Helper methods
        /***************************************************/
    
        private static void AttachCADDataFragment(AAD.Entity entity, IBHoMObject bhObj)
        {
            if (entity == null || bhObj == null)
                return;

            var layerName = entity.Layer;
            var handle = entity.Handle.ToString();

            if (!string.IsNullOrWhiteSpace(layerName))
            {
                bhObj.Fragments.Add(new BCF.CADDataFragment
                {
                    CADLayerName = layerName,
                    CADObjectHandle = handle
                });
            }
        }


        
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
    }
}