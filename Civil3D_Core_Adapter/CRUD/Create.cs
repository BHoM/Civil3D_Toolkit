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

using BH.Civil3D.Engine.Core;
using BH.Engine.Base;
using BH.oM.Adapter;
using BHC = BH.oM.Adapters.Civil3D.Elements;
using BH.oM.Adapters.Civil3D.Fragments;
using BH.oM.Base;
using BH.Engine.Units;
using BHG = BH.oM.Geometry;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using ACG = Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using ADC = Autodesk.Civil.DatabaseServices;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Civil3D.Adapter.Core
{
    public partial class Civil3DListenerAdapter
    {
        /***************************************************/
        /**** Adapter Methods                           ****/
        /***************************************************/

        protected override bool ICreate<T>(IEnumerable<T> objects, ActionConfig actionConfig = null)
        {
            if (objects == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot push a null list of objects to Civil 3D.");
                return false;
            }

            List<T> items = objects.ToList();
            if (items.Count == 0)
                return true;

            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                BH.Engine.Base.Compute.RecordError("No drawing is open in Civil 3D.");
                return false;
            }

            Database db = doc.Database;
            CivilDocument civdoc = CivilApplication.ActiveDocument;
            double fromSI = 1.0.ToLength(db.DrawingLengthUnit(civdoc));
            int created = 0;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);

                foreach (T item in items)
                {
                    object obj = item is ObjectWrapper wrapper ? wrapper.WrappedObject : item;

                    object scaled = FromSI(obj, fromSI);
                    Entity entity = WidePolyline(scaled, (item as IBHoMObject)?.Fragments?.OfType<PolylineWidth>().FirstOrDefault(), fromSI)
                        ?? ToEntity(scaled, bt, tr, civdoc);
                    if (entity == null)
                        continue;

                    if (entity.ObjectId.IsNull)
                    {
                        ms.AppendEntity(entity);
                        tr.AddNewlyCreatedDBObject(entity, true);
                    }

                    entity.LayerId = LayerId(item as IBHoMObject, lt, db);
                    created++;
                }

                if (created > 0)
                    tr.Commit();
            }

            return created == items.Count;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static Entity ToEntity(object obj, BlockTable bt, Transaction tr, CivilDocument civdoc)
        {
            switch (obj)
            {
                case BHG.Point point:
                    return point.ToCivil3DDBPoint();
                case BHG.Line line:
                    return line.ToCivil3D();
                case BHG.Arc arc:
                    return arc.ToCivil3D();
                case BHG.Circle circle:
                    return circle.ToCivil3D();
                case BHG.Ellipse ellipse:
                    return ellipse.ToCivil3D();
                case BHG.Polyline polyline:
                    return polyline.ToCivil3D();
                case BHG.PolyCurve polyCurve:
                    return polyCurve.ToCivil3D();
                case BHG.NurbsCurve nurbsCurve:
                    return nurbsCurve.ToCivil3D();
                case BHC.TextNote textNote:
                    return textNote.ToCivil3D();
                case BHC.Block block:
                    return block.ToCivil3D(bt, tr);
                case BHC.SurveyPoint surveyPoint:
                    return CreateSurveyPoint(surveyPoint, civdoc, tr);
                default:
                    BH.Engine.Base.Compute.RecordError($"Pushing {obj?.GetType().Name} to Civil 3D is not supported.");
                    return null;
            }
        }

        /***************************************************/

        //A curve carrying a width greater than 0 is drawn as one lightweight polyline of that constant width. Returns null,
        //so that the curve is pushed as usual, when there is no width or the curve cannot be drawn as such a polyline.
        private static Entity WidePolyline(object obj, PolylineWidth width, double fromSI)
        {
            if (width == null || width.GlobalWidth <= 0 || !(obj is BHG.ICurve))
                return null;

            Polyline polyline = ((BHG.ICurve)obj).ToCivil3DPolyline();
            if (polyline == null)
            {
                BH.Engine.Base.Compute.RecordWarning($"The width of a pushed {obj.GetType().Name} has been ignored because it cannot be drawn as a lightweight polyline.");
                return null;
            }

            polyline.ConstantWidth = width.GlobalWidth * fromSI;
            return polyline;
        }

        /***************************************************/

        //Survey points are created through the Civil 3D point collection, so they are already in the drawing when returned.
        private static Entity CreateSurveyPoint(BHC.SurveyPoint point, CivilDocument civdoc, Transaction tr)
        {
            if (civdoc == null)
            {
                BH.Engine.Base.Compute.RecordError("Survey points can only be pushed to a Civil 3D drawing.");
                return null;
            }

            if (point.Location == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot push a survey point without a Location.");
                return null;
            }

            //A point number already used in the drawing cannot be applied, so the new point takes Civil 3D's next number instead.
            if (point.PointNumber > 0 && civdoc.CogoPoints.Contains((uint)point.PointNumber))
            {
                BH.Engine.Base.Compute.RecordWarning($"Point number {point.PointNumber} is already used in the drawing, so Civil 3D's next number was used instead.");
                point = point.ShallowClone();
                point.PointNumber = 0;
            }

            ACG.Point3dCollection locations = new ACG.Point3dCollection { point.Location.ToCivil3D() };
            ObjectIdCollection ids = civdoc.CogoPoints.Add(locations, point.RawDescription ?? "", true, false, true);
            if (ids == null || ids.Count == 0)
            {
                BH.Engine.Base.Compute.RecordError("Civil 3D did not create the survey point.");
                return null;
            }

            ADC.CogoPoint c3dPoint = (ADC.CogoPoint)tr.GetObject(ids[0], OpenMode.ForWrite);
            point.ToCivil3D(c3dPoint);
            return c3dPoint;
        }

        /***************************************************/

        //Uses the layer recorded on the object when it was pulled, if that layer exists; otherwise the current layer. Layers are never created.
        private static ObjectId LayerId(IBHoMObject obj, LayerTable lt, Database db)
        {
            DrawingIdentifiers identifiers = obj?.Fragments.OfType<DrawingIdentifiers>().FirstOrDefault();
            if (identifiers == null || string.IsNullOrWhiteSpace(identifiers.Layer))
                return db.Clayer;

            if (lt.Has(identifiers.Layer))
                return lt[identifiers.Layer];

            BH.Engine.Base.Compute.RecordWarning($"Layer '{identifiers.Layer}' does not exist; using the current layer.");
            return db.Clayer;
        }


        /***************************************************/
        /**** Private Methods - Units                   ****/
        /***************************************************/

        //Returns the object scaled from metres to drawing units. BHoM objects are shallow-cloned so the input is never changed.
        private static object FromSI(object obj, double f)
        {
            if (f == 1.0)
                return obj;

            switch (obj)
            {
                case BHG.IGeometry geometry:
                    return Scaled(geometry, f);
                case BHC.TextNote textNote:
                    BHC.TextNote text = textNote.ShallowClone();
                    text.Orientation = ScaleOrigin(text.Orientation, f);
                    text.Height *= f;
                    return text;
                case BHC.Block block:
                    BHC.Block instance = block.ShallowClone();
                    instance.Orientation = ScaleOrigin(instance.Orientation, f);
                    return instance;
                case BHC.SurveyPoint surveyPoint:
                    BHC.SurveyPoint point = surveyPoint.ShallowClone();
                    point.Location = Scaled(point.Location, f);
                    return point;
                default:
                    return obj;
            }
        }

        /***************************************************/
    }
}
