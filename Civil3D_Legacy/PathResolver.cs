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

