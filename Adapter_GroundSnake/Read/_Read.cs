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

            return new List<IBHoMObject>();
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        /*private List<BH.oM.Geometry.Point> ReadPoints()
        {
            CivilDocument doc = CivilApplication.ActiveDocument;

            List<BH.oM.Geometry.Point> pnts = new List<oM.Geometry.Point>();

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                foreach(ObjectId id in doc.GetAllPointIds())
                {
                    DBObject pnt = trans.GetObject(id, OpenMode.ForRead);
                    ADC.Point p = (ADC.Point)pnt;
                }
            }

                return pnts;
        }*/

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
