// References you typically need (exact DLL names vary by install):
// - AcCoreMgd.dll
// - AcDbMgd.dll
// - (optional) AcMgd.dll
// - BHoM.dll, BHoM_Engine.dll, BH.oM.Base.dll (and BH.oM.Geometry.dll if you want real geometry)
//
// This code reads a DWG using a *side database* (no UI open), filters entity types based on requested BHoM types,
// and returns either real BHoM geometry (if BH.Engine.Geometry.Create is available) or a BHoM wrapper object.

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
    public static class ReadFromDWG
    {
        public static List<IBHoMObject> ReadFromDWGList(List<string> dwgPaths,List<Type> bhomTypes,bool active = true)
        {
            var results = new List<IBHoMObject>();

            if (!active)
                return results;

            if (dwgPaths == null || dwgPaths.Count == 0)
                return results;

            if (bhomTypes == null || bhomTypes.Count == 0)
                return results;

            foreach (var path in dwgPaths)
            {
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                if (!File.Exists(path))
                    continue;

                using (var db = new Database(false, true))
                {
                    try
                    {
                        db.ReadDwgFile(path, FileShare.ReadWrite, false, string.Empty);
                        db.CloseInput(true);

                        using (var tr = db.TransactionManager.StartTransaction())
                        {
                            var adapter = new CivilUIAdapter();

                            foreach (var requestedType in bhomTypes)
                            {
                                results.AddRange(
                                    adapter.ReadCAD(requestedType, db, tr)
                                );
                            }

                            tr.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debugger.Log($"Failed to read DWG '{path}': {ex}");
                        continue;
                    }
                }
            }

            return results;
        }

    }
}