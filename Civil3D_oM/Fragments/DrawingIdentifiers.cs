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

using BH.oM.Base;
using System.ComponentModel;

namespace BH.oM.Adapters.Civil3D.Fragments
{
    [Description("Identifiers of the drawing object that the BHoM object carrying this fragment was pulled from.")]
    public class DrawingIdentifiers : IPersistentAdapterId, IImmutable
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        [Description("Handle of the drawing object, as a hexadecimal string. It is unique within a drawing and does not change between sessions.")]
        public virtual object PersistentId { get; } = "";

        [Description("Name of the layer the drawing object is on.")]
        public virtual string Layer { get; } = "";

        [Description("Name of the drawing object's class, e.g. AcDbLine or AeccDbCogoPoint.")]
        public virtual string ObjectType { get; } = "";

        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        public DrawingIdentifiers(string persistentId = "", string layer = "", string objectType = "")
        {
            PersistentId = persistentId;
            Layer = layer;
            ObjectType = objectType;
        }

        /***************************************************/
    }
}
