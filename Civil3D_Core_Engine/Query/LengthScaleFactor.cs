/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

using BH.oM.Base.Attributes;
using BH.oM.Units;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;

using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets the factor that scales lengths between the drawing's units and metres, converted with Units_Engine. Geometry is scaled by this factor in one go.")]
        [Input("database", "Drawing database.")]
        [Input("civilDocument", "Civil 3D document of the drawing, if there is one.")]
        [Input("toSI", "If true, the factor converts drawing units to metres; if false, it converts metres to drawing units.")]
        [Output("factor", "Multiply lengths by this factor to convert them.")]
        public static double LengthScaleFactor(this Database database, CivilDocument civilDocument = null, bool toSI = true)
        {
            LengthUnit unit = database.DrawingLengthUnit(civilDocument);
            return toSI ? BH.Engine.Units.Convert.FromLength(1.0, unit) : BH.Engine.Units.Convert.ToLength(1.0, unit);
        }

        /***************************************************/
    }
}