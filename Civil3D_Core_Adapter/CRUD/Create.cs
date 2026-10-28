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
using BHACC3d = BH.Civil3D.Adapter.Core;
using BH.Civil3D.Engine.Core;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

using BH.Engine.Geometry;

using BH.oM.Adapter;

namespace BH.Civil3D.Adapter.Core
{
    public partial class Civil3DListenerAdapter
    {

        /***************************************************/
        /****           Adapter Methods                 ****/
        /***************************************************/
        protected override bool ICreate<T>(IEnumerable<T> objects, ActionConfig actionConfig = null)
        {
            if (objects == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot push to Civil 3D: the list of objects is null.");
                return true;
            }

            int count = objects.Count();

            if (count == 0)
                return true;

            try
            {
                Document acDoc = Application.DocumentManager.MdiActiveDocument;

                CivilDocument civilDoc = CivilApplication.ActiveDocument;
                Database db = acDoc.Database;

                using (acDoc.LockDocument())
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);
                    var context =new BHACC3d.Civil3DRuntimeContext(acDoc, civilDoc, db, tr, bt, ms);

                    // ---- UNWRAP OBJECTWRAPPERS ----
                    var unwrapped = objects.Select(o => (o is BH.oM.Adapter.ObjectWrapper ow ? ow.WrappedObject : (object)o)).ToList();

                    // ---- DISPATCH ON UNWRAPPED TYPES ----
                    ProcessCustomObjectGeometry(unwrapped, context);
                    CreateCollection(unwrapped.OfType<BH.oM.Adapters.Civil3D.Elements.CoGoPoint>(), context);
                    
                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                BH.Engine.Base.Compute.RecordError($"Failed to push objects to Civil 3D: {ex.Message}");
            }

            return true;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/
        private void ProcessCustomObjectGeometry(IEnumerable<object> unwrapped, BHACC3d.Civil3DRuntimeContext context)
        {
            var geomCustomObjects = unwrapped
                .OfType<BH.oM.Base.CustomObject>()
                .Where(co => co?.CustomData?.ContainsKey("Geometry") ?? false)
                .ToList();

            if (!geomCustomObjects.Any())
                return;

            foreach (var co in geomCustomObjects)
            {
                var geometry = ExtractGeometry(co).Cast<object>().ToList();
                if (geometry.Any())
                {
                    GeometryMappingFunction(geometry, context, GetCadLayerName(co));
                }
            }
        }
        private void GeometryMappingFunction(IEnumerable<object> unwrapped, BHACC3d.Civil3DRuntimeContext context, string cadLayerName)
        {
            DispatchGeometry<BH.oM.Geometry.Point>(unwrapped, context, cadLayerName, CreateCollection);
            DispatchGeometry<BH.oM.Geometry.Line>(unwrapped, context, cadLayerName, CreateCollection);
            DispatchGeometry<BH.oM.Geometry.Arc>(unwrapped, context, cadLayerName, CreateCollection);
            DispatchGeometry<BH.oM.Geometry.Circle>(unwrapped, context, cadLayerName, CreateCollection);
            DispatchGeometry<BH.oM.Geometry.Ellipse>(unwrapped, context, cadLayerName, CreateCollection);
            DispatchGeometry<BH.oM.Geometry.Polyline>(unwrapped, context, cadLayerName, CreateCollection);
            DispatchGeometry<BH.oM.Geometry.PolyCurve>(unwrapped, context, cadLayerName, CreateCollection);
            DispatchGeometry<BH.oM.Geometry.NurbsCurve>(unwrapped, context, cadLayerName, CreateCollection);
        }
        private void DispatchGeometry<T>(IEnumerable<object> unwrapped, BHACC3d.Civil3DRuntimeContext context, string cadLayerName, Func<List<T>, BHACC3d.Civil3DRuntimeContext, string, bool> handler) where T : class
        {
            var items = unwrapped.OfType<T>().ToList();
            if (items.Any())
                _ = handler(items, context, cadLayerName);
        }

        private void DispatchCollection<T>(IEnumerable<object> unwrapped, BHACC3d.Civil3DRuntimeContext context, Func<List<T>, BHACC3d.Civil3DRuntimeContext, bool> handler) where T : class
        {
            var items = unwrapped.OfType<T>().ToList();
            if (items.Any())
                _ = handler(items, context);
        }

        private void DispatchCollection<T>(IEnumerable<object> unwrapped, BHACC3d.Civil3DRuntimeContext context, string layer, Func<List<T>, BHACC3d.Civil3DRuntimeContext, string, bool> handler) where T : class
        {
            var items = unwrapped.OfType<T>().ToList();
            if (items.Any())
                _ = handler(items, context, layer);
        }

        private bool CreateCollection(IEnumerable<BH.oM.Geometry.Point> points, BHACC3d.Civil3DRuntimeContext context, string layer = "0")
            => ProcessEntityCollection(points, context, layer, p => p.ToCivil3D(), "Could not convert a Point to a Civil 3D entity, so it was not created.");

        private bool CreateCollection(IEnumerable<BH.oM.Geometry.Line> lines, BHACC3d.Civil3DRuntimeContext context, string layer = "0")
            => ProcessEntityCollection(lines, context, layer, l => l.ToCivil3D(), "Could not convert a Line to a Civil 3D entity, so it was not created.");

        private bool CreateCollection(IEnumerable<BH.oM.Geometry.Arc> arcs, BHACC3d.Civil3DRuntimeContext context, string layer = "0")
            => ProcessEntityCollection(arcs, context, layer, a => a.ToCivil3D(), "Could not convert an Arc to a Civil 3D entity, so it was not created.");

        private bool CreateCollection(IEnumerable<BH.oM.Geometry.Circle> circles, BHACC3d.Civil3DRuntimeContext context, string layer = "0")
            => ProcessEntityCollection(circles, context, layer, c => c.ToCivil3D(), "Could not convert a Circle to a Civil 3D entity, so it was not created.");

        private bool CreateCollection(IEnumerable<BH.oM.Geometry.Ellipse> ellipses, BHACC3d.Civil3DRuntimeContext context, string layer = "0")
            => ProcessEntityCollection(ellipses, context, layer, e => e.ToCivil3D(), "Could not convert an Ellipse to a Civil 3D entity, so it was not created.");

        private bool CreateCollection(IEnumerable<BH.oM.Geometry.Polyline> polylines, BHACC3d.Civil3DRuntimeContext context, string layer = "0")
            => ProcessPolylineCollection(polylines, context, layer, 
                p => p.ControlPoints != null && p.ControlPoints.Any(pt => Math.Abs(pt.Z) > 1e-3) 
                    ? (object)p.ToPolyline3D() 
                    : (object)p.ToLWPolyline(), 
                "Could not convert a Polyline to a Civil 3D entity, so it was not created.");

        private bool CreateCollection(IEnumerable<BH.oM.Geometry.PolyCurve> polyCurves, BHACC3d.Civil3DRuntimeContext context, string layer = "0")
            => ProcessPolylineCollection(polyCurves, context, layer,
                pc => HasNonZeroZ(pc)
                    ? (object)pc.ToPolyline3D()
                    : (object)pc.ToLWPolyline(),
                "Could not convert a PolyCurve to a Civil 3D entity, so it was not created.");

        private bool CreateCollection(IEnumerable<BH.oM.Geometry.NurbsCurve> splines, BHACC3d.Civil3DRuntimeContext context, string layer = "0")
            => ProcessEntityCollection(splines, context, layer, s => s.ToCivil3D(), "Could not convert a NurbsCurve to a Civil 3D spline, so it was not created.");
        
        private bool CreateCollection(
            IEnumerable<BH.oM.Adapters.Civil3D.Elements.CoGoPoint> cogos,
            BHACC3d.Civil3DRuntimeContext context,
            string layer = "0",
            string desc = "",
            bool useDescriptionKey = true,
            bool matchOnParams = false,
            bool useNextPointNumSetting = true)
        {
            try
            {
                if (cogos == null)
                {
                    BH.Engine.Base.Compute.RecordError("Cannot create CoGo points: the list of CoGo points is null.");
                    return true;
                }

                if (context == null)
                {
                    BH.Engine.Base.Compute.RecordError("Cannot create CoGo points: the Civil 3D drawing is not available.");
                    return false;
                }

                if (context.CivilDocument == null)
                {
                    BH.Engine.Base.Compute.RecordError("Cannot create CoGo points: the active drawing is not a Civil 3D drawing.");
                    return false;
                }

                if (context.Transaction == null)
                {
                    BH.Engine.Base.Compute.RecordError("Cannot create CoGo points: no transaction is open on the active drawing.");
                    return false;
                }

                // Materialise once
                var input = cogos.Where(c => c != null).ToList();
                if (input.Count == 0)
                    return true;

                // Validate points before building locations
                var valid = new List<BH.oM.Adapters.Civil3D.Elements.CoGoPoint>();
                var locations = new Autodesk.AutoCAD.Geometry.Point3dCollection();

                for (int i = 0; i < input.Count; i++)
                {
                    var cp = input[i];
                    var p = cp.Location;

                    if (p == null)
                    {
                        BH.Engine.Base.Compute.RecordWarning($"CoGo point at index {i} was skipped because its location is null.");
                        continue;
                    }

                    // Guard against NaN/Infinity (these can crash Point3d / Add)
                    if (double.IsNaN(p.X) || double.IsNaN(p.Y) || double.IsNaN(p.Z) ||
                        double.IsInfinity(p.X) || double.IsInfinity(p.Y) || double.IsInfinity(p.Z))
                    {
                        BH.Engine.Base.Compute.RecordWarning($"CoGo point at index {i} was skipped because its location has invalid coordinates ({p.X}, {p.Y}, {p.Z}).");
                        continue;
                    }

                    locations.Add(new Autodesk.AutoCAD.Geometry.Point3d(p.X, p.Y, p.Z));
                    valid.Add(cp);
                }

                if (valid.Count == 0)
                {
                    BH.Engine.Base.Compute.RecordWarning("No CoGo points were created because none had a valid location.");
                    return true;
                }

                // Ensure layer once (not per point)
                EnsureLayerPresent(context, layer);

                ObjectIdCollection ids = context.CivilDocument.CogoPoints.Add(
                    locations,
                    desc ?? string.Empty,
                    useDescriptionKey,
                    matchOnParams,
                    useNextPointNumSetting);

                if (ids == null || ids.Count == 0)
                {
                    BH.Engine.Base.Compute.RecordError("Civil 3D did not create any CoGo points.");
                    return false;
                }

                // Assumption: ids returned in same order as locations
                int idx = 0;
                foreach (Autodesk.AutoCAD.DatabaseServices.ObjectId id in ids)
                {
                    if (idx >= valid.Count)
                        break;

                    var bhPoint = valid[idx];
                    idx++;

                    try
                    {
                        var dbObj = context.Transaction.GetObject(id, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite, false);
                        var c3dPoint = dbObj as ADC.CogoPoint;

                        if (c3dPoint == null)
                        {
                            BH.Engine.Base.Compute.RecordWarning("A created CoGo point could not be opened for editing, so its name and layer were not set.");
                            continue;
                        }

                        // Apply properties + layer
                        // bhPoint.ToCivil3D(c3dPoint);
                        
                        if (!string.IsNullOrWhiteSpace(bhPoint.Name))
                        {
                            // Only assign if you're absolutely sure you're not duplicating
                            c3dPoint.PointName = bhPoint.Name;
                        }

                        c3dPoint.Layer = layer;
                    }
                    catch (System.Exception exPoint)
                    {
                        BH.Engine.Base.Compute.RecordError($"Could not set the name or layer of a CoGo point: {exPoint.Message}");
                    }
                }

                if (ids.Count != valid.Count)
                {
                    BH.Engine.Base.Compute.RecordWarning($"Civil 3D created {ids.Count} CoGo points from {valid.Count} valid inputs.");
                }

                return true;
            }
            catch (System.Exception ex)
            {
                BH.Engine.Base.Compute.RecordError($"Failed to create CoGo points: {ex.Message}");
                return false;
            }
        }

        /***************************************************/
        /**** Private Helpers                           ****/
        /***************************************************/

        private bool ProcessEntityCollection<T, TEntity>(IEnumerable<T> items, BHACC3d.Civil3DRuntimeContext context, string layer, Func<T, TEntity> converter, string errorMessage) where TEntity : Entity
        {
            foreach (var item in items)
            {
                if (item == null)
                    continue;

                var acEntity = converter(item);
                if (acEntity == null)
                {
                    BH.Engine.Base.Compute.RecordError(errorMessage);
                    continue;
                }

                acEntity.SetDatabaseDefaults();
                EnsureLayerPresent(context, layer);
                acEntity.Layer = layer;
                context.BlockTableRecord.AppendEntity(acEntity);
                context.Transaction.AddNewlyCreatedDBObject(acEntity, true);
            }

            return true;
        }

        private bool ProcessPolylineCollection<T>(IEnumerable<T> items, BHACC3d.Civil3DRuntimeContext context, string layer, Func<T, object> converter, string errorMessage) where T : class
        {
            foreach (var item in items)
            {
                if (item == null)
                    continue;

                var acPolyline = converter(item);
                if (acPolyline == null)
                {
                    BH.Engine.Base.Compute.RecordError(errorMessage);
                    continue;
                }

                if (acPolyline is AAD.Polyline3d poly3d)
                {
                    poly3d.SetDatabaseDefaults();
                    EnsureLayerPresent(context, layer);
                    poly3d.Layer = layer;
                    context.BlockTableRecord.AppendEntity(poly3d);
                    context.Transaction.AddNewlyCreatedDBObject(poly3d, true);
                }
                else if (acPolyline is AAD.Polyline polyLW)
                {
                    polyLW.SetDatabaseDefaults();
                    EnsureLayerPresent(context, layer);
                    polyLW.Layer = layer;
                    context.BlockTableRecord.AppendEntity(polyLW);
                    context.Transaction.AddNewlyCreatedDBObject(polyLW, true);
                }
            }

            return true;
        }

        // Helper: check if PolyCurve has any non-zero Z values
        private bool HasNonZeroZ(BH.oM.Geometry.PolyCurve polyCurve)
        {
            if (polyCurve == null || polyCurve.Curves == null)
                return false;

            foreach (var curve in polyCurve.Curves)
            {
                if (curve is BH.oM.Geometry.Line line)
                {
                    if (Math.Abs(line.Start.Z) > 1e-10 || Math.Abs(line.End.Z) > 1e-3)
                        return true;
                }
                else if (curve is BH.oM.Geometry.Arc arc)
                {
                    // Check arc coordinate system origin for Z
                    if (Math.Abs(arc.CoordinateSystem.Origin.Z) > 1e-3)
                        return true;
                }
                else if (curve is BH.oM.Geometry.Polyline polyline)
                {
                    if (polyline.ControlPoints != null && polyline.ControlPoints.Any(pt => Math.Abs(pt.Z) > 1e-3))
                        return true;
                }
            }

            return false;
        }

        // Helper: extract geometry (single or enumerable)
        private IEnumerable<BH.oM.Geometry.IGeometry> ExtractGeometry(BH.oM.Base.CustomObject co)
        {
            if (co == null || co.CustomData == null)
                yield break;

            object geomObj;
            if (!co.CustomData.TryGetValue("Geometry", out geomObj) || geomObj == null)
                yield break;

            // Single geometry
            var single = geomObj as BH.oM.Geometry.IGeometry;
            if (single != null)
            {
                yield return single;
                yield break;
            }

            // Collection of geometry
            var enumerable = geomObj as IEnumerable<BH.oM.Geometry.IGeometry>;
            if (enumerable != null)
            {
                foreach (var g in enumerable)
                {
                    var geom = g as BH.oM.Geometry.IGeometry;
                    if (geom != null)
                        yield return geom;
                }
            }
        }


        private string GetCadLayerName(BH.oM.Base.CustomObject co)
        {
            if (co == null || co.Fragments == null)
                return null;

            var identifiers = co.Fragments.OfType<BH.oM.Adapters.Civil3D.Fragments.DrawingIdentifiers>().FirstOrDefault();

            if (identifiers == null || string.IsNullOrWhiteSpace(identifiers.Layer))
                return null;

            return identifiers.Layer;
        }

        private static bool EnsureLayerPresent(
            BHACC3d.Civil3DRuntimeContext context,
            string layerName
        )
        {
            if (string.IsNullOrWhiteSpace(layerName))
                return false;

            Database db = context.Database;
            Transaction tr = context.Transaction;

            LayerTable lt =
                (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);

            // Layer already exists
            if (lt.Has(layerName))
                return true;

            // Create new layer using database defaults
            LayerTableRecord layer = new LayerTableRecord
            {
                Name = layerName
            };

            lt.UpgradeOpen();
            lt.Add(layer);
            tr.AddNewlyCreatedDBObject(layer, true);

            return true;
        }

        public static void ApplyPointNumberIfValid(
            ADC.CogoPoint c3dPoint,
            int bhPointNumber,
            bool useNextPointNumSetting
        )
        {
            if (useNextPointNumSetting)
                return;

            // Civil 3D requires PointNumber >= 1; 0 or less keeps the number Civil 3D assigned
            if (bhPointNumber < 1)
                return;

            c3dPoint.PointNumber = (uint)bhPointNumber;
        }




    }
}


