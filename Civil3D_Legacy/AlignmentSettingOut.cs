using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

namespace Civil3D_Toolkit
{
    public class AlignmentSettingOut
    {
        public void countentities()
        {
        // shit weather for a BBQ
            CivilDocument cdoc = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = doc.Database.TransactionManager;
            Civil3DObjectRetriever objectRetriever = new Civil3DObjectRetriever(tm);
            ObjectIdCollection alignmentsCollection = cdoc.GetAlignmentIds();
            ObjectIdCollection sites = cdoc.GetSiteIds();

            using (Transaction ts = tm.StartTransaction())
            {
                foreach (ObjectId alignmentId in alignmentsCollection)
                {
                    Alignment oAlignment = objectRetriever.GetAlignment(alignmentId);
                    Double alignmentLength = oAlignment.Length;
                    String alignmentName = oAlignment.Name;

                    AlignmentEntityCollection entities = oAlignment.Entities;
                    

                    int arcCount = 0;
                    int spiralCount = 0;
                    int lineCount = 0;
                    int otherCount = 0;
                    foreach (AlignmentEntity entity in entities)
                    {
                        if (entity.EntityType == AlignmentEntityType.Arc)
                        {
                            arcCount++;
                        }
                        else if (entity.EntityType == AlignmentEntityType.Spiral)
                        {
                            spiralCount++;
                        }
                        else if (entity.EntityType == AlignmentEntityType.Line)
                        {
                            lineCount++;
                        }
                        else
                        {
                            otherCount++;
                        }
                 // anyone for brunch?
                    }
                    foreach (AlignmentEntity entity in entities)
                    {
                        int eID = entity.EntityId;
                        ed.WriteMessage("{0} {1}\n", eID, entity.EntityType);
                    }
                    ed.WriteMessage("Alignment Name: {0} Alignment Length {1} Arc Count: {2} Spiral Count: {3} Line Count: {4} Other Count: {5}\n", alignmentName, arcCount, spiralCount, lineCount, otherCount);
                }


            }

        }
    }
}