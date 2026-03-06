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
using BHC = BH.oM.Civils.Elements;
using BH.UI.Civil.Engine;
//using BH.oM.Geometry;

using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.Runtime;
using ADC = Autodesk.Civil.DatabaseServices;


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

        /***************************************************/
        /**** Private Methods                           ****/
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
            CivilDocument doc = CivilApplication.ActiveDocument;

            List<BHC.CivSurface> tinSurfaceList = new List<BHC.CivSurface>();

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in doc.GetSurfaceIds())
                {
                    ADC.TinSurface tinSurface = trans.GetObject(id, OpenMode.ForRead) as ADC.TinSurface;
                    if (tinSurface != null)
                    {
                        tinSurfaceList.Add(tinSurface.ToBHoM());
                    }
                }

                trans.Commit();
            }

            return tinSurfaceList;
        }
        /***************************************************/

    }
}


