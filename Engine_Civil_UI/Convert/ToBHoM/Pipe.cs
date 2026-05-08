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
using BHC = BH.oM.Civils.Elements;
using Autodesk.Civil.DatabaseServices;

using Autodesk.AutoCAD.DatabaseServices;


using Autodesk.Civil.ApplicationServices;
namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHC.Pipe ToBHoM(this Pipe acPipe, Transaction tr)
        {

            ObjectId networkId = acPipe.NetworkId;
            Network network = tr.GetObject(networkId, OpenMode.ForRead) as Network;
            string networkName = network?.Name;

            return new BHC.Pipe
            {          
                Name = acPipe.Name,
                CentreLine = new BHG.Line { Start = acPipe.StartPoint.FromCivil3D(), End = acPipe.EndPoint.FromCivil3D() },
                Diameter = acPipe.InnerDiameterOrWidth,
                Thickness = acPipe.WallThickness,
                FlowDirection = acPipe.FlowDirectionMethod.ToBHoM(),
                PipeNetworkName = networkName
            };

        }


        public static BHC.Pipe ToBHoM(this PressurePipe acPipe, Transaction tr)
        {
            ObjectId networkId = acPipe.NetworkId;
            PressurePipeNetwork network = tr.GetObject(networkId, OpenMode.ForRead) as PressurePipeNetwork;
            string networkName = network?.Name;

            return new BHC.Pipe
            {
                
                Name = acPipe.Name,
                CentreLine = new BHG.Line { Start = acPipe.StartPoint.FromCivil3D(), End = acPipe.EndPoint.FromCivil3D() },
                Diameter = acPipe.InnerDiameter,
                Thickness = acPipe.WallThickness,
                PipeNetworkName = networkName
            };

        }        

        /***************************************************/
    }
}


