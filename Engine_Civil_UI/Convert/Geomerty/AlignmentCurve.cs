/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Geometry;
using BHG = BH.oM.Geometry;
using ACD = Autodesk.AutoCAD.DatabaseServices;
using ACG = Autodesk.AutoCAD.Geometry;

using ADC = Autodesk.Civil.DatabaseServices;

using BHC = BH.oM.Civils.Elements;
using BH.Engine.Geometry;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHC.AlignmentCurve FromCivil3D(ADC.AlignmentLine aLine)
        {
            BHG.Line line = new oM.Geometry.Line
            {
                Start = aLine.StartPoint.FromCivil3D(),
                End = aLine.EndPoint.FromCivil3D(),
            };

            return new BHC.AlignmentCurve
            {
                Curve = line,
            };
        }

        public static BHC.AlignmentCurve FromCivil3D(ADC.AlignmentArc aArc)
        {
            return new BHC.AlignmentCurve
            {
                Curve = BH.Engine.Geometry.Create.ArcByCentre(aArc.CenterPoint.FromCivil3D(), aArc.StartPoint.FromCivil3D(), aArc.EndPoint.FromCivil3D()),
            };
        }

        public static BHC.AlignmentCurve FromCivil3D(object o)
        {
            return null;
        }
    }
}


