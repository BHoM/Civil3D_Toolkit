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
using Autodesk.AutoCAD.DatabaseServices;

using BH.oM.Base;

namespace BH.oM.Civils.Fragments
{
    public class CADDataFragment : IFragment
    {       
        public string CADLayerName { get; set; }
        public string CADObjectHandle { get; set; }
    }

    // public class CadTextFragment : IFragment
    // {
    //     public double TextHeight { get; set; }
    //     public double Width { get; set; }
    //     public string TextStyle { get; set; }
    //     public AttachmentPoint Attachment { get; set; }  // This is the AutoCAD enum for text attachment, which is a combination of horizontal and vertical alignment. It may need to be converted to BHoM's own alignment system if necessary.
    //     public bool Annotative { get; set; }
    // }

}

