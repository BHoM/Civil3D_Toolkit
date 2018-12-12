using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BHC = BH.oM.Civils.Elements;
using BH.Engine.GroundSnake;
//using BH.oM.Geometry;

using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.Runtime;
using ADC = Autodesk.Civil.DatabaseServices;


//using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.Geometry;

namespace BH.UI.GroundSnake.Adapter
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

            return new List<IBHoMObject>();
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

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
                        bhPipe.CustomData[Engine.Civil3D.Query.AdapterID] = pipeId.ToString();
                        pipeList.Add(bhPipe);
                    }

                }

                trans.Commit();
            }

            return pipeList;
        }


        /***************************************************/
        
    }
}
