using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BH.UI.Civil.Adapter;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

// BHoM base (adjust namespaces if your build differs)
using BH.oM.Base;

namespace BH.UI.Civil.Adapter // adjust to your engine namespace
{
    public static class Debugger
    {
        public static readonly string DebugLogPath =
            System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CivilUIAdapter_Debug.txt");

        public static void Log(string msg)
        {
            try
            {
                System.IO.File.AppendAllText(
                    DebugLogPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {msg}{Environment.NewLine}");
            }
            catch { /* ignore */ }

            try
            {
                BH.Engine.Base.Compute.RecordNote(msg);
            }
            catch { /* ignore */ }
        }
    }
}