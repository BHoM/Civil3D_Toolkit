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

using BH.Engine.Base;
using BH.oM.Adapters.Civil3D.Fragments;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.ComponentModel;

using Autodesk.AutoCAD.DatabaseServices;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Attaches the drawing object's identifiers to a BHoM object as a DrawingIdentifiers fragment, replacing any existing one. " +
                     "This modifies the input object, so use it only on objects the adapter has just created.")]
        [Input("obj", "BHoM object converted from the drawing object.")]
        [Input("entity", "Drawing object the BHoM object was converted from.")]
        public static void SetIdentifiers(this IBHoMObject obj, Entity entity)
        {
            if (obj == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot set identifiers on a null BHoM object.");
                return;
            }

            DrawingIdentifiers identifiers = entity.Identifiers();
            if (identifiers != null)
                obj.Fragments.AddOrReplace(identifiers);
        }

        /***************************************************/
    }
}
