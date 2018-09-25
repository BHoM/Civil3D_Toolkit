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
