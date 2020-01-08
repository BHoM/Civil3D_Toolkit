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
//using Autodesk.AutoCAD.Geometry;

namespace BH.UI.Civil.Adapter
{
    public partial class GroundSnakeAdapter
    {

        /***************************************************/
        /**** Adapter Methods                           ****/
        /***************************************************/

        //General method called by the adapter when reading in data
        protected override IEnumerable<IBHoMObject> Read(Type type, IList ids)
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

                if (type == typeof(BHC.CivProfile))
                {
                    return ReadProfiles();
                }

                if (type == typeof(BHC.Parcel))
                {
                    return ReadParcels();
                }

                if (type == typeof(BHC.Alignment))
                {
                    return ReadAlignments();
                }
            }
            catch(Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                List<string> stack = e.StackTrace.Split(new char[] { '\n' })
                    .Where(x => x.Contains(" BH."))
                    .ToList();

                foreach(string s in stack)
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

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in doc.GetParcelTableIds())
                {
                    ADC.Parcel parcel = id.GetObject(OpenMode.ForRead) as ADC.Parcel;
                    parcels.Add(parcel.FromCivil3D());
                }
            }

            return parcels;
        }

        private List<BHC.CivProfile> ReadProfiles()
        {
            CivilDocument doc = CivilApplication.ActiveDocument;

            List<BHC.CivProfile> profiles = new List<BHC.CivProfile>();

            /*using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in doc.GetProfiles())
                {
                    ADC.Profile profile = id.GetObject(OpenMode.ForRead) as ADC.Profile;
                    profiles.Add(profile.FromCivil3D());
                }
            }*/

            return profiles;
        }

        private List<BHC.CoGoPoint> ReadCoGoPoints()
        {
            CivilDocument doc = CivilApplication.ActiveDocument;

            List<BHC.CoGoPoint> pnts = new List<BHC.CoGoPoint>();

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                foreach(ObjectId id in doc.CogoPoints)
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
                        bhPipe.CustomData[BH.Engine.Civil3D.Query.AdapterID] = pipeId.ToString();
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
                        bhManholeChamber.CustomData[BH.Engine.Civil3D.Query.AdapterID] = pipeId.ToString();
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
