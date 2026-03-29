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
using BHG = BH.oM.Geometry;
using ACD = Autodesk.AutoCAD.DatabaseServices;
using ACG = Autodesk.AutoCAD.Geometry;
using BHCE = BH.oM.Civils.Elements;

using BH.Engine.Base;
using BHB = BH.oM.Base;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        ///////////////////
        // To Civils 3d //
        /////////////////
        /***************   ********************/
        




        /////////////////////
        // From Civils 3d //
        ///////////////////   
        /// 
        /// 

        public static BHCE.CadTextNote FromCivil3d(this ACD.MText mtext)
        {
            if (mtext == null)
                return null;


            BHCE.CadTextNote note = new BHCE.CadTextNote
            {
                Text = mtext.Text,
                Position = new BHG.Point
                {
                    X = mtext.Location.X,
                    Y = mtext.Location.Y,
                    Z = mtext.Location.Z
                },
                Rotation = mtext.Rotation
            };

            // Attach CAD metadata as fragments
            // note.Fragments.Add(mtext.ToCadTextFragment());

            return note;
        }


        // internal static BH.oM.Civils.Fragments.CadTextFragment ToCadTextFragment(this ACD.MText mtext)
        // {
        //     return new BH.oM.Civils.Fragments.CadTextFragment
        //     {
        //         TextHeight = mtext.TextHeight,
        //         Width = mtext.Width,
        //         TextStyle = mtext.TextStyleName,
        //         Attachment = mtext.Attachment,
        //         Annotative = mtext.Annotative == ACD.AnnotativeStates.True,
        //     };
        // }

    }
}


