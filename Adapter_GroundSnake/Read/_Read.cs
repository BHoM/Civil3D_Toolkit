using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Structure.Elements;


using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace BH.UI.GroundSnake.Adapter
{
    public partial class GroundSnakeAdapter
    {

        /***************************************************/
        /****           Adapter Methods                 ****/
        /***************************************************/

        protected override IEnumerable<IBHoMObject> Read(Type type, IList ids)
        {

            if (type == typeof(Bar))
            {
                return ReadBars();
            }

            return new List<IBHoMObject>();

            //throw new NotImplementedException();
        }

        /***************************************************/


        private List<Bar> ReadBars(List<string> ids = null)
        {
            var editor = Application.DocumentManager.MdiActiveDocument.Editor;
            var per = editor.GetEntity("\nSelect line");
            if (per.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK) return new List<Bar>();

            using (var transaction = editor.Document.Database.TransactionManager.StartTransaction())
            {
                var line = transaction.GetObject(per.ObjectId, OpenMode.ForRead) as Line;
                if (line != null)
                {
                    var lineWeight = line.LineWeight;
                    var weight = (int)lineWeight / 100.0; // in millimeters
                    var weightInInches = (weight / 100.0) / 0.254;
                }
                transaction.Commit();
            }
            return new List<Bar>();
        }
    }
}
