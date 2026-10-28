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

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.Settings;

using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets the factor that converts lengths in the drawing's units to metres. Uses the Civil 3D drawing settings when a Civil 3D document is given, otherwise the drawing's INSUNITS.")]
        [Input("database", "Drawing database.")]
        [Input("civilDocument", "Civil 3D document of the drawing, if there is one.")]
        [Output("factor", "Multiply drawing lengths by this to get metres.")]
        public static double LengthToSI(this Database database, CivilDocument civilDocument = null)
        {
            if (civilDocument != null)
            {
                DrawingUnitType units = civilDocument.Settings.DrawingSettings.UnitZoneSettings.DrawingUnits;
                if (units == DrawingUnitType.Feet)
                    return 0.3048;
                if (units == DrawingUnitType.Meters)
                    return 1.0;
            }

            if (database == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot get the units of a null drawing database.");
                return 1.0;
            }

            if (database.Insunits == UnitsValue.Undefined)
            {
                BH.Engine.Base.Compute.RecordWarning("Drawing units are not set (INSUNITS = 0). Assuming metres.");
                return 1.0;
            }

            return UnitsConverter.GetConversionFactor(database.Insunits, UnitsValue.Meters);
        }

        /***************************************************/
    }
}
