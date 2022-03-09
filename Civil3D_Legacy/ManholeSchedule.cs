/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;

namespace Civil3D_Toolkit
{
    public class ManholeSchedule
    {
        public ExcelServices xlServices = new ExcelServices();

        public void exportToExcel()
        {
            // This is executed when "MHS" is run in the C3D console.
            CivilDocument cdoc = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = doc.Database.TransactionManager;
            Civil3DObjectRetriever objectRetriever = new Civil3DObjectRetriever(tm);
            DataLinkDecorator dataLinkDecorator = new DataLinkDecorator(objectRetriever, doc.Database.DataLinkManager);

            // Check that there's a pipe network to parse:
            if (cdoc.GetPipeNetworkIds() == null)
            {
                ed.WriteMessage("There are no pipe networks to export. Open a document that contains at least one pipe network");
                return;
            }

            Microsoft.Office.Interop.Excel.Application xlApp = xlServices.GetNewExcelApp();
            Workbook xlWb = xlServices.GetExcelWorkbook(xlApp, @"L:\Civil3D\General\Manhole Schedule\MH SCH Template.xlsx");

            // Get pipe network, structures, and pipes:
            using (Transaction ts = tm.StartTransaction())
            {
                ObjectIdCollection oNetworkIds = objectRetriever.GetPipeNetworkIdCollection();

                StructureDecoratorFactory oStructureDecoratorFactory = new StructureDecoratorFactory(objectRetriever);
                PipeDecoratorFactory oPipeDecoratorFactory = new PipeDecoratorFactory(objectRetriever);
                StructurePrinter oStructurePrinter = new StructurePrinter(oStructureDecoratorFactory);
                PipePrinter oPipePrinter = new PipePrinter(oPipeDecoratorFactory);
                ExcelPrinter oSchedulePrinter = new ExcelPrinter();
                PathResolver oPathResolver = new PathResolver();

                foreach (ObjectId oNetworkId in oNetworkIds)
                {
                    int rowIndex = 5;
                    Network oNetwork = objectRetriever.GetPipeNetwork(oNetworkId);
                    Worksheet xlWs = xlServices.GetNewWorksheetFromFirstWorksheet(xlWb, oNetwork.Name);
                    oSchedulePrinter.PrintToCell(xlWs, "B1", "MANHOLE SCHEDULE: " + oNetwork.Name);
                    List<Structure> sortedStructureList = objectRetriever.GetSortedStructureList(oNetwork);

                    foreach (Structure oStructure in sortedStructureList)
                    {
                        char colIndex = 'B';
                        StructureDecorator oDecoratedStructure = oStructureDecoratorFactory.Create(oStructure);

                        oSchedulePrinter.Print(xlWs, rowIndex, colIndex, oStructure, oStructurePrinter);

                        if (oDecoratedStructure.HasOutgoingPipe())
                        {
                            colIndex = 'M';
                            oSchedulePrinter.Print(xlWs, rowIndex, colIndex, oDecoratedStructure.GetOutgoingPipe(), oPipePrinter);
                        }

                        ++rowIndex;
                    }
                    xlServices.NameRange(xlWs, "B1", "Q" + Convert.ToString(rowIndex - 1), oNetwork.Name.Replace(" ", "_"));
                }
                xlServices.SaveWorkbook(xlWb, oPathResolver.GetScheduleFilePath("MH SCH"));
                xlServices.CloseExcel(xlApp, xlWb);

                foreach (ObjectId oNetworkId in oNetworkIds)
                {
                    Network oNetwork = objectRetriever.GetPipeNetwork(oNetworkId);
                    if (dataLinkDecorator.DataLinkNameExistsInDrawing(oNetwork.Name))
                    {
                        dataLinkDecorator.UpdateDataLink(oNetwork.Name);
                        ed.WriteMessage(oNetwork.Name + " datalink updated." + "\n");
                    }
                    else
                    {
                        DataLink dl = dataLinkDecorator.CreateDataLink(oNetwork.Name, oPathResolver.GetDataLinkPath("MH SCH", oNetwork.Name, oNetwork.Name.Replace(" ", "_")));
                        ts.AddNewlyCreatedDBObject(dl, true);
                        ed.WriteMessage(oNetwork.Name + " datalinked successfully." + "\n");
                    }
                }
                ts.Commit();
            }
            doc.SendStringToExecute("TABLE ", true, false, true);

            ed.WriteMessage("MHS plugin finished successfully.");
        }
    }
}
