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

using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Civil3D_Toolkit
{
    public class CustomStuctureComparer: IComparer<string>, IComparer<Structure>
    {
        public int Compare(string x, string y)
        {
            if (x == y || x == null || y == null)
            {
                return 0;
            }

            Regex rgx = new Regex(@"\d+(?:\.\d+)?");

            if (rgx.IsMatch(x) && rgx.IsMatch(y))
            {
                decimal xNumber = Convert.ToDecimal(rgx.Match(x).Value);
                decimal yNumber = Convert.ToDecimal(rgx.Match(y).Value);

                if (xNumber > yNumber)
                {
                    return 1;
                }

                if (xNumber < yNumber)
                {
                    return -1;
                }
            }

            return 0;
        }
        public int Compare(Structure x, Structure y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            return Compare(x.Name, y.Name);
        }
    }
}

