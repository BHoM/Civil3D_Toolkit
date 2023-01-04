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

namespace Civil3D_Toolkit
{
    public class ExcelPrinter
    {
        public void PrintToCell(Worksheet ws, string cellIndex, string sValue)
        {
            // Prints a string to a given cell.
            Range aRange = ws.get_Range(cellIndex, Type.Missing);
            aRange.Value2 = sValue;
        }

        private void PrintArray(Worksheet ws, int rowIndex, char colIndex, string[] arr)
        {
            // Prints an array across multiple cells horizontally.
            foreach (string item in arr)
            {
                PrintToCell(ws, "" + colIndex + rowIndex, item);
                ++colIndex;
            }
        }

        public void Print<T>(Worksheet ws, int rowIndex, char colIndex, T oObject, IObjectPrinter<T> printer)
        {
            // Prints an object if it is printable.
            string[] arr = printer.Print(oObject);
            PrintArray(ws, rowIndex, colIndex, arr);
        }
    }
}

