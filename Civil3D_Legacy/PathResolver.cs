using System;
using System.Text.RegularExpressions;

namespace Civil3D_Toolkit
{
    public class PathResolver
    {
        public string GetScheduleFilePath(string fileName)
        {
            // Returns a path to use for saving/opening a schedule.
            string dwgPrefix = Convert.ToString(Autodesk.AutoCAD.ApplicationServices.Application.GetSystemVariable("DWGPREFIX"));

            Regex rgx = new Regex(@".*?[0-9]{5,6}[^\\]*(?=\\)");
            Match match = rgx.Match(dwgPrefix);

            return match + @"\F08 Civils - Infrastructure\Data\" + fileName;
        }

        public string GetDataLinkPath(string fileName, string worksheetName, string name)
        {
            // Returns a path to a schedule to be datalinked into a drawing. 
            ExcelServices xlServices = new ExcelServices();
            string path = GetScheduleFilePath(fileName) + @".xlsx!" + worksheetName + @"!'" + worksheetName + @"'!" + xlServices.SanitisedRangeName(name);
            return path;
        }
    }
}
