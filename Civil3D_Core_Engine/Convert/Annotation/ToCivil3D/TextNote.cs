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

using BHCE = BH.oM.Adapters.Civil3D.Elements;
using BH.oM.Base.Attributes;

using ACD = Autodesk.AutoCAD.DatabaseServices;

using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Converts BH.oM.Adapters.Civil3D.Elements.TextNote to a Civil 3D MText.")]
        [Input("textNote", "BH.oM.Adapters.Civil3D.Elements.TextNote to convert.")]
        [Output("mText", "The converted Civil 3D MText.")]
        public static ACD.MText ToCivil3D(this BHCE.TextNote textNote)
        {
            if (textNote == null)
                return null;

            if (textNote.Orientation == null)
            {
                BH.Engine.Base.Compute.RecordError("A TextNote has no Orientation, so it could not be converted to Civil 3D text.");
                return null;
            }

            ACD.MText mtext = new ACD.MText
            {
                Contents = textNote.Text,
                Location = textNote.Orientation.Origin.ToCivil3D(),
                Normal = textNote.Orientation.Z.ToCivil3D(),
                Direction = textNote.Orientation.X.ToCivil3D(),
            };

            if (textNote.Height > 0)
                mtext.TextHeight = textNote.Height;

            return mtext;
        }

        /***************************************************/
    }
}
