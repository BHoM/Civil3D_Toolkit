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

using BHG = BH.oM.Geometry;
using BHCE = BH.oM.Adapters.Civil3D.Elements;
using ACD = Autodesk.AutoCAD.DatabaseServices;
using ACG = Autodesk.AutoCAD.Geometry;

using BH.oM.Base.Attributes;

using System.ComponentModel;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Converts a Civil 3D MText to BH.oM.Adapters.Civil3D.Elements.TextNote.")]
        [Input("mtext", "A Civil 3D MText to convert.")]
        [Output("textNote", "The converted BH.oM.Adapters.Civil3D.Elements.TextNote.")]
        public static BHCE.TextNote FromCivil3D(this ACD.MText mtext)
        {
            if (mtext == null)
                return null;

            ACG.Vector3d x = mtext.Direction;
            ACG.Vector3d y = mtext.Normal.CrossProduct(x);

            return new BHCE.TextNote
            {
                Text = mtext.Text,
                Orientation = BH.Engine.Geometry.Create.CartesianCoordinateSystem(mtext.Location.FromCivil3D(), x.FromCivil3D(), y.FromCivil3D()),
                Height = mtext.TextHeight,
            };
        }


        /***************************************************/

        [Description("Converts a Civil 3D DBText to BH.oM.Adapters.Civil3D.Elements.TextNote.")]
        [Input("text", "A Civil 3D DBText to convert.")]
        [Output("textNote", "The converted BH.oM.Adapters.Civil3D.Elements.TextNote.")]
        public static BHCE.TextNote FromCivil3D(this ACD.DBText text)
        {
            if (text == null)
                return null;

            //Single-line text stores its rotation in its OCS, which AutoCAD derives from the normal
            ACG.Vector3d x = ACG.Vector3d.XAxis.TransformBy(ACG.Matrix3d.PlaneToWorld(text.Normal)).RotateBy(text.Rotation, text.Normal);
            ACG.Vector3d y = text.Normal.CrossProduct(x);

            return new BHCE.TextNote
            {
                Text = text.TextString,
                Orientation = BH.Engine.Geometry.Create.CartesianCoordinateSystem(text.Position.FromCivil3D(), x.FromCivil3D(), y.FromCivil3D()),
                Height = text.Height,
            };
        }
        /***************************************************/
    }
}
