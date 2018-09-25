using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Adapter;
using BH.oM.Adapters.Civil3D;
using BH.oM.Base;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;

namespace BH.Adapter.Civil3D
{
    public partial class Civil3DAdapter : BHoMAdapter
    {
        /***************************************************/
        /****           Public  Fields                  ****/
        /***************************************************/

        public Civil3DConfig Civil3DConfig { get; set; } = new Civil3DConfig();

        

        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        public Civil3DAdapter(string filePath = "", Civil3DConfig civil3DConfig = null, bool Active = false)
        {
            if (Active)
            {
                if (filePath == "")
                {
                    m_Document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                    m_TransactionManager = m_Document.Database.TransactionManager;
                    m_Editor = m_Document.Editor;
                }
            }
        }

        protected override bool Create<T>(IEnumerable<T> objects, bool replaceAll = false)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<IBHoMObject> Read(Type type, IList ids)
        {
            throw new NotImplementedException();
        }

        /***************************************************/
        /****           Private  Fields                 ****/
        /***************************************************/

        private Autodesk.AutoCAD.DatabaseServices.TransactionManager m_TransactionManager;
        private CivilDocument m_CivilDocument;
        private Document m_Document;
        private Editor m_Editor;
    }
}
