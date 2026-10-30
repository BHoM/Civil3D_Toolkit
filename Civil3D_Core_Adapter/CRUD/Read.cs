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
using BH.oM.Adapter;
using BHC = BH.oM.Adapters.Civil3D.Elements;
using BH.oM.Adapters.Civil3D.Fragments;
using BH.oM.Base;
using BH.Engine.Geometry;
using BH.Engine.Units;
using BHG = BH.oM.Geometry;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using ADC = Autodesk.Civil.DatabaseServices;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BH.Civil3D.Adapter.Core
{
    public partial class Civil3DListenerAdapter
    {
        /***************************************************/
        /**** Adapter Methods                           ****/
        /***************************************************/

        protected override IEnumerable<IBHoMObject> IRead(Type type, IList ids, ActionConfig actionConfig = null)
        {
            List<IBHoMObject> result = new List<IBHoMObject>();

            if (type == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot pull from Civil 3D without a type.");
                return result;
            }

            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                BH.Engine.Base.Compute.RecordError("No drawing is open in Civil 3D.");
                return result;
            }

            Database db = doc.Database;
            CivilDocument civdoc = CivilApplication.ActiveDocument;
            double toSI = 1.0.FromLength(db.DrawingLengthUnit(civdoc));

            List<ReaderEntry> readers = m_Readers.Where(x => x.Produces.Any(p => type.IsAssignableFrom(p))).ToList();
            if (readers.Count == 0)
            {
                BH.Engine.Base.Compute.RecordWarning($"Pulling {type.Name} from Civil 3D is not supported.");
                return result;
            }

            if (civdoc == null && readers.Any(x => x.NeedsCivil))
            {
                BH.Engine.Base.Compute.RecordWarning("This drawing is not a Civil 3D drawing; Civil 3D objects cannot be read.");
                readers = readers.Where(x => !x.NeedsCivil).ToList();
            }

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                List<Type> modelSpaceTypes = readers.SelectMany(x => x.ModelSpaceEntities).Distinct().ToList();
                if (modelSpaceTypes.Count > 0)
                    result.AddRange(ReadEntities(ms.Cast<ObjectId>(), modelSpaceTypes, type, tr, toSI));

                if (readers.Any(x => x.Produces.Contains(typeof(BHC.BlockDefinition))))
                    result.AddRange(ReadBlockDefinitions(bt, db, tr, toSI));

                if (civdoc != null)
                {
                    if (readers.Any(x => x.Produces.Contains(typeof(BHC.CoGoPoint))))
                        result.AddRange(ReadEntities(civdoc.CogoPoints.Cast<ObjectId>(), new List<Type> { typeof(ADC.CogoPoint) }, type, tr, toSI));

                    if (readers.Any(x => x.Produces.Contains(typeof(BHC.Alignment))))
                        result.AddRange(ReadEntities(civdoc.GetAlignmentIds().Cast<ObjectId>(), new List<Type> { typeof(ADC.Alignment) }, type, tr, toSI));

                    if (readers.Any(x => x.Produces.Contains(typeof(BHC.Parcel))))
                        result.AddRange(ReadEntities(ParcelIds(civdoc, tr), new List<Type> { typeof(ADC.Parcel) }, type, tr, toSI));
                }

                tr.Commit();
            }

            return result;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        //Converts the drawing objects of the given classes, keeps results of the requested type, scales them to SI units
        //and attaches their drawing identifiers. Geometry is wrapped, because IRead must return IBHoMObjects and plain
        //geometry cannot carry the identifiers; Push reads the layer back from the same wrapper.
        private List<IBHoMObject> ReadEntities(IEnumerable<ObjectId> ids, List<Type> entityTypes, Type type, Transaction tr, double toSI)
        {
            List<IBHoMObject> result = new List<IBHoMObject>();

            foreach (ObjectId id in ids)
            {
                Entity entity = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (entity == null || !entityTypes.Any(x => x.IsInstanceOfType(entity)))
                    continue;

                try
                {
                    object converted = entity is ADC.Alignment ? ((ADC.Alignment)entity).FromCivil3D() : entity.IFromCivil3D(tr);
                    if (converted == null || !type.IsAssignableFrom(converted.GetType()))
                        continue;

                    IBHoMObject obj;
                    if (converted is BHG.IGeometry geometry)
                    {
                        obj = new ObjectWrapper
                        {
                            WrappedObject = Scaled(geometry, toSI)
                        };
                    }
                    else
                    {
                        obj = converted as IBHoMObject;
                        if (obj == null)
                            continue;

                        ToSI(obj as dynamic, toSI);
                    }

                    obj.SetIdentifiers(entity);

                    double width = entity.GlobalWidth();
                    if (width > 0)
                        obj.Fragments.AddOrReplace(new PolylineWidth(width * toSI));

                    result.Add(obj);
                }
                catch (Exception e)
                {
                    BH.Engine.Base.Compute.RecordWarning($"Could not read {entity.GetType().Name} {entity.Handle}: {e.Message}");
                }
            }

            return result;
        }

        /***************************************************/

        private List<IBHoMObject> ReadBlockDefinitions(BlockTable bt, Database db, Transaction tr, double toSI)
        {
            List<IBHoMObject> result = new List<IBHoMObject>();
            Dictionary<ObjectId, BHC.BlockDefinition> cache = new Dictionary<ObjectId, BHC.BlockDefinition>();

            foreach (ObjectId id in bt)
            {
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
                if (btr.IsLayout || btr.IsAnonymous || btr.IsFromExternalReference)
                    continue;

                try
                {
                    BHC.BlockDefinition definition = btr.FromCivil3D(tr, db, cache);
                    ToSI(definition, toSI);
                    result.Add(definition);
                }
                catch (Exception e)
                {
                    BH.Engine.Base.Compute.RecordWarning($"Could not read block definition {btr.Name}: {e.Message}");
                }
            }

            return result;
        }

        /***************************************************/

        //Parcels belong to sites, so they are found through the drawing's sites.
        private static List<ObjectId> ParcelIds(CivilDocument civdoc, Transaction tr)
        {
            List<ObjectId> ids = new List<ObjectId>();
            foreach (ObjectId siteId in civdoc.GetSiteIds())
            {
                ADC.Site site = tr.GetObject(siteId, OpenMode.ForRead) as ADC.Site;
                if (site != null)
                    ids.AddRange(site.GetParcelIds().Cast<ObjectId>());
            }

            return ids;
        }


        /***************************************************/
        /**** Private Methods - Units                   ****/
        /***************************************************/

        //Each method scales the geometric properties and [Length] values of a freshly read object from drawing units to metres.
        private static void ToSI(IBHoMObject obj, double f)
        {
        }

        /***************************************************/

        private static void ToSI(BHC.CoGoPoint obj, double f)
        {
            obj.Location = Scaled(obj.Location, f);
        }

        /***************************************************/

        private static void ToSI(BHC.FeatureLine obj, double f)
        {
            obj.Curve = Scaled(obj.Curve, f);
        }

        /***************************************************/

        private static void ToSI(BHC.Alignment obj, double f)
        {
            obj.Curve = Scaled(obj.Curve, f);
            obj.ReferencePoint = Scaled(obj.ReferencePoint, f);
            obj.StartingStation *= f;
            obj.ReferencePointStation *= f;
        }

        /***************************************************/

        private static void ToSI(BHC.Parcel obj, double f)
        {
            obj.Boundary = Scaled(obj.Boundary, f);
            obj.AreaLocation = Scaled(obj.AreaLocation, f);
        }

        /***************************************************/

        private static void ToSI(BHC.TextNote obj, double f)
        {
            obj.Orientation = ScaleOrigin(obj.Orientation, f);
            obj.Height *= f;
        }

        /***************************************************/

        private static void ToSI(BHC.Block obj, double f)
        {
            obj.Orientation = ScaleOrigin(obj.Orientation, f);
        }

        /***************************************************/

        private static void ToSI(BHC.BlockDefinition obj, double f)
        {
            obj.BasePoint = Scaled(obj.BasePoint, f);
            obj.Geometry = obj.Geometry.Select(x => Scaled(x, f)).ToList();
            obj.NestedBlocks.ForEach(x => ToSI(x, f));
        }

        /***************************************************/

        /***************************************************/

        //Scales geometry uniformly about the origin. Null stays null, and a factor of 1 returns the input unchanged.
        private static T Scaled<T>(T geometry, double f) where T : class, BHG.IGeometry
        {
            if (geometry == null || f == 1.0)
                return geometry;

            return geometry.IScale(new BHG.Point(), new BHG.Vector { X = f, Y = f, Z = f }) as T;
        }

        /***************************************************/

        //Scales only the origin: the axes of an orientation are unit vectors.
        private static BHG.CoordinateSystem.Cartesian ScaleOrigin(BHG.CoordinateSystem.Cartesian orientation, double f)
        {
            if (orientation == null || f == 1.0)
                return orientation;

            return BH.Engine.Geometry.Create.CartesianCoordinateSystem(Scaled(orientation.Origin, f), orientation.X, orientation.Y);
        }


        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        //What each kind of pull produces, which drawing objects in model space it reads, and whether it needs Civil 3D.
        private class ReaderEntry
        {
            public List<Type> Produces;
            public List<Type> ModelSpaceEntities;
            public bool NeedsCivil;
        }

        private static readonly List<ReaderEntry> m_Readers = new List<ReaderEntry>
        {
            new ReaderEntry { Produces = new List<Type> { typeof(BHG.Point) }, ModelSpaceEntities = new List<Type> { typeof(DBPoint) } },
            new ReaderEntry { Produces = new List<Type> { typeof(BHG.Line), typeof(BHG.Arc), typeof(BHG.Circle), typeof(BHG.Ellipse), typeof(BHG.Polyline), typeof(BHG.PolyCurve), typeof(BHG.NurbsCurve) },
                              ModelSpaceEntities = new List<Type> { typeof(Line), typeof(Arc), typeof(Circle), typeof(Ellipse), typeof(Polyline), typeof(Polyline2d), typeof(Polyline3d), typeof(Spline) } },
            new ReaderEntry { Produces = new List<Type> { typeof(BHC.TextNote) }, ModelSpaceEntities = new List<Type> { typeof(MText), typeof(DBText) } },
            new ReaderEntry { Produces = new List<Type> { typeof(BHC.Block) }, ModelSpaceEntities = new List<Type> { typeof(BlockReference) } },
            new ReaderEntry { Produces = new List<Type> { typeof(BHC.BlockDefinition) }, ModelSpaceEntities = new List<Type>() },
            new ReaderEntry { Produces = new List<Type> { typeof(BHC.CoGoPoint) }, ModelSpaceEntities = new List<Type>(), NeedsCivil = true },
            new ReaderEntry { Produces = new List<Type> { typeof(BHC.FeatureLine) }, ModelSpaceEntities = new List<Type> { typeof(ADC.FeatureLine) }, NeedsCivil = true },
            new ReaderEntry { Produces = new List<Type> { typeof(BHC.Alignment) }, ModelSpaceEntities = new List<Type>(), NeedsCivil = true },
            new ReaderEntry { Produces = new List<Type> { typeof(BHC.Parcel) }, ModelSpaceEntities = new List<Type>(), NeedsCivil = true },
        };

        /***************************************************/
    }
}
