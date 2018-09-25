using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;

// Basic functionality to add:
// 1. Fix Terminate method.

// Nice things to have:
// 1. Determine flow directions geometrically instead of using Autodesk method.
// 3. Split table after x rows.
// 4. Custom columns.

// Testing - what happens if..
// 1. Plugin is run in C3D 2017? 2015?
// 2. Other utilities are present?
// 3. No pipe networks are present?
// 4. Plugin is run on a project on Talon drive?

namespace Civil3D_Toolkit
{
    public class Plugins : IExtensionApplication
    {
        public ExcelServices xlServices = new ExcelServices();

        [CommandMethod("ASS")]
        public void AlignmentSettingOutPlugin()
        {
            AlignmentSettingOut alignmentSO = new AlignmentSettingOut();
            alignmentSO.countentities();
        }

        [CommandMethod("MHS")]
        public void ManholeSchedulePlugin()
        {
            ManholeSchedule manholeSchedule = new ManholeSchedule();
            manholeSchedule.exportToExcel();
        }

        #region IExtensionApplication Members

        public void Initialize()
        {
            // Do nothing
        }

        public void Terminate()
        {
            //Clean up all our Excel COM objects
            //This will close Excel without saving

            //if (xlWb != null)
            //{
            //    try
            //    {
            //        xlWb.Close(false, Type.Missing, Type.Missing);
            //        xlApp.Quit();
            //        Marshal.FinalReleaseComObject(xlWs);
            //        Marshal.FinalReleaseComObject(xlWb);
            //        Marshal.FinalReleaseComObject(xlApp);

            //        GC.Collect();
            //        GC.WaitForPendingFinalizers();

            //    }
            //    catch (System.Exception e)
            //    {
            //        Console.WriteLine(e.Message);
            //    }
            //}
        }
        #endregion
    }
}
