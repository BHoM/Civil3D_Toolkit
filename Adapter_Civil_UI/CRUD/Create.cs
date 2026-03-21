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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using AAD = Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.Runtime;
using ADC = Autodesk.Civil.DatabaseServices;
using BHACC3d = BH.oM.Adapters.Civil3D;
using BH.UI.Civil.Engine;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

using BH.Engine.Geometry;

using BH.oM.Adapter;

namespace BH.UI.Civil.Adapter
{
    public partial class CivilUIAdapter
    {

        /***************************************************/
        /****           Adapter Methods                 ****/
        /***************************************************/

        //General method called by the adapter for push
        protected override bool ICreate<T>(IEnumerable<T> objects, ActionConfig actionConfig = null)
        {
            
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            Database db = acDoc.Database;
            
            using (DocumentLock acLckDoc = acDoc.LockDocument())
            {    
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    var context = new BHACC3d.Civil3DRuntimeContext(acDoc, civilDoc, db, tr, acBlkTbl, acBlkTblRec);
                    //foreach (var obj in objects)
                    //{
                        CreateCollection(objects as dynamic, context);
                    //}
                    tr.Commit();
                }
            }
            return true;
        }




        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private bool CreateCollection(IEnumerable<BH.oM.Civils.Elements.Pipe> pipes, BHACC3d.Civil3DRuntimeContext context)
        {
            CivilDocument civDoc = context.CivilDocument;
            Transaction acTrans = context.Transaction;
            BlockTableRecord acBlkTblRec = context.BlockTableRecord;

            List<BH.oM.Civils.Elements.Pipe> p = pipes.ToList();

            foreach (BH.oM.Civils.Elements.Pipe pipe in p)
            {
                Line crv = ((pipe.CentreLine as BH.oM.Geometry.Line).ToCivil3D());
                crv.SetDatabaseDefaults();
                acBlkTblRec.AppendEntity(crv);
                acTrans.AddNewlyCreatedDBObject(crv, true);
            }
            return true;
        }

        private bool CreateCollection(IEnumerable<BH.oM.Civils.Elements.CivSurface> srfs, BHACC3d.Civil3DRuntimeContext context)
        {
            // CivilDocument civDoc = context.CivilDocument;
            // Transaction acTrans = context.Transaction;
            // BlockTableRecord acBlkTblRec = context.BlockTableRecord;
            List<BH.oM.Civils.Elements.CivSurface> srf = srfs.ToList();
            
            foreach (BH.oM.Civils.Elements.CivSurface s in srfs)
            {               
                BH.UI.Civil.Engine.Create.InCivil3D(s, context);
            }
            return true;
        }









    }
}


