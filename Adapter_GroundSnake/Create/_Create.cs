using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using BH.oM.Structure.Elements;

namespace BH.UI.GroundSnake.Adapter
{
    public partial class GroundSnakeAdapter
    {

        /***************************************************/
        /****           Adapter Methods                 ****/
        /***************************************************/

        protected override bool Create<T>(IEnumerable<T> objects, bool replaceAll = false)
        {
            if (objects.Count() > 0)
            {
                if (objects.First() is Bar)
                {
                    CreateCollection(objects as IEnumerable<Bar>);
                }
            }

            return true;

        }

        /***************************************************/


        private void CreateCollection(IEnumerable<Bar> enumerable)
        {

            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;


            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (Line acLine = new Line(new Point3d(5, 5, 0),
                                              new Point3d(12, 3, 0)))
                {

                    // Add the new object to the block table record and the transaction
                    acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                }

                // Save the new object to the database
                acTrans.Commit();
            }

            
        }
    }
}
