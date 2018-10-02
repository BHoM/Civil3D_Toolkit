using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Structure.Elements;
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
        /****           Adapter Methods                 ****/
        /***************************************************/

        protected override IEnumerable<IBHoMObject> Read(Type type, IList ids)
        {

            if (type == typeof(BHC.Pipe))
            {
                return ReadPipes();
            }

            return new List<IBHoMObject>();

            //throw new NotImplementedException();
        }

        /***************************************************/

        public List<BHC.Pipe> ReadPipes(List<string> ids = null)
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
                        pipeList.Add(pipe.ToBHoM());
                    }

                }

                trans.Commit();
            }

            return pipeList;
        }


        /***************************************************/

        //    //Test method
        //private List<Bar> ReadBars(List<string> ids = null)
        //{
        //    // Get the current document and database
        //    Document acDoc = Application.DocumentManager.MdiActiveDocument;
        //    Database acCurDb = acDoc.Database;

        //    List<Bar> barList = new List<Bar>();

        //    // Start a transaction
        //    using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
        //    {
        //        // Open the Block table for read
        //        BlockTable acBlkTbl;
        //        acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
        //                                     OpenMode.ForRead) as BlockTable;

        //        // Open the Block table record Model space for read
        //        BlockTableRecord acBlkTblRec;
        //        acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
        //                                        OpenMode.ForRead) as BlockTableRecord;

        //        // Step through the Block table record
        //        foreach (ObjectId asObjId in acBlkTblRec)
        //        {
        //            if (asObjId.ObjectClass.DxfName == "LINE")
        //            {
        //                Line line = acTrans.GetObject(asObjId, OpenMode.ForRead) as Line;
        //                Bar bar = new Bar
        //                {
        //                    StartNode = new Node { Position = new oM.Geometry.Point { X = line.StartPoint.X, Y = line.StartPoint.Y, Z = line.StartPoint.Z } },
        //                    EndNode = new Node { Position = new oM.Geometry.Point { X = line.EndPoint.X, Y = line.EndPoint.Y, Z = line.EndPoint.Z } }
        //                };

        //                barList.Add(bar);
        //            }
        //            //acDoc.Editor.WriteMessage("\nDXF name: " + asObjId.ObjectClass.DxfName);
        //            //acDoc.Editor.WriteMessage("\nObjectID: " + asObjId.ToString());
        //            //acDoc.Editor.WriteMessage("\nHandle: " + asObjId.Handle.ToString());
        //            //acDoc.Editor.WriteMessage("\n");
        //        }

        //        acTrans.Commit();
                
        //        // Dispose of the transaction
        //    }
        //    return barList;
        //}
    }
}
