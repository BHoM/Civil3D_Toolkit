/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Civil3D_Toolkit
{
    public class ExcelServices
    {
        public List<object> ExcelObjectsList = new List<object>();

        public Application GetNewExcelApp()
        {
            // Opens a new Excel Application and returns it.
            Application app = new Application();

            if (app == null)
            {
                throw new Exception(@"Excel could not be started. Check that your office installation and project references are correct.");
            }

            app.Visible = false;
            app.DisplayAlerts = false;

            ExcelObjectsList.Add(app);
            return app;
        }

        public Workbook GetExcelWorkbook(Application app, string filePath)
        {
            // Opens an Excel Workbook and returns it.
            Workbook wb = app.Workbooks.Open(filePath);
            ExcelObjectsList.Add(app);
            return wb;
        }

        public Worksheet GetNewWorksheetFromFirstWorksheet(Workbook wb, string worksheetName)
        {
            // Copies first Worksheet to a new Worksheet and returns that.
            ((Worksheet)wb.Worksheets[1]).Copy(Type.Missing, (Worksheet)wb.Worksheets[1]);
            Worksheet ws = ((Worksheet)wb.Worksheets[2]);
            ws.Name = worksheetName;
            ExcelObjectsList.Add(ws);
            return (Worksheet)wb.Worksheets[2];
        }

        public void SaveWorkbook(Workbook wb, string filePath)
        {
            // Saves an Excel Workbook.
            wb.SaveAs(filePath);
        }

        public void NameRange(Worksheet ws, string firstCellIndex, string finalCellIndex, string name)
        {
            // Names a range of cells from firstCellIndex to finalCellIndex e.g. A1:Q5
            // Note: name must not contain any spaces.
            ws.Names.Add(SanitisedRangeName(name), ws.get_Range(firstCellIndex, finalCellIndex));
        }

        public string SanitisedRangeName(string name)
        {
            Regex rgx = new Regex(@"[^0-9a-z\.]", RegexOptions.IgnoreCase);
            string sanitisedName = rgx.Replace(name, "");
            return sanitisedName.Substring(0, Math.Min(255, sanitisedName.Length));
        }

        public void CloseExcel(Application app, Workbook wb)
        {
            wb.Close(false, Type.Missing, Type.Missing);
            app.Quit();
            //Marshal.FinalReleaseComObject(ws);
            Marshal.FinalReleaseComObject(wb);
            Marshal.FinalReleaseComObject(app);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
