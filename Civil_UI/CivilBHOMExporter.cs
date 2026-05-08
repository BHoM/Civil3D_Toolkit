using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

using Autodesk.Civil.ApplicationServices;

using BH.oM.Base;
using BH.oM.Civils.Elements; // if needed for your Type list
using BH.UI.Civil.Adapter;   // your CivilUIAdapter namespace

public class CivilBHOMExporter : IExtensionApplication
{
    public void Initialize() { /* no UI init */ }
    public void Terminate() { }

    [CommandMethod("BHOM_EXPORT")]
    public void Export()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;

        var db = doc.Database;

        CivilDocument civdoc = null;
        try
        {
            civdoc = CivilApplication.ActiveDocument;
        }
        catch
        {
            civdoc = null;
        }

        // Safety: Civil doc must match this DWG database
        if (civdoc == null)
            return;

        // Where to write output (from .bat)
        var outPath = Environment.GetEnvironmentVariable("BHOM_EXPORT_OUT");
        if (string.IsNullOrWhiteSpace(outPath))
            outPath = Path.Combine(Path.GetDirectoryName(doc.Name) ?? "", Path.GetFileNameWithoutExtension(doc.Name) + ".json");

        // Choose which BHoM types to read (example – adjust to your needs)
        var requestedTypes = new List<Type>
        {
            typeof(BH.oM.Civils.Elements.CoGoPoint),
            typeof(BH.oM.Civils.Elements.Pipe),
            typeof(BH.oM.Civils.Elements.ManholeChamber),
            typeof(BH.oM.Civils.Elements.CivSurface),
            typeof(BH.oM.Civils.Elements.PressureNetworkFitting),
            typeof(BH.oM.Civils.Elements.PressureNetworkAppurtenance)
        };

        var adapter = new CivilUIAdapter();
        var results = new List<IBHoMObject>();

        using (var tr = db.TransactionManager.StartTransaction())
        {
            foreach (var t in requestedTypes)
            {
                // Calls your existing Civil read routing
                results.AddRange(adapter.ReadC3DCAD(t, db, tr, civdoc));
            }

            tr.Commit();
        }

        // Serialize. If you already have BHoM serialiser utilities in your solution, use them here.
        // As a safe fallback, write a simple line-based export (replace with real BHoM JSON in your codebase).
        Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? "");
        File.WriteAllLines(outPath,
            results.Select(o => o.GetType().FullName ?? "UNKNOWN"));

        // If you have a BHoM JSON serialiser available in your toolkit, swap the above for that.
    }
}