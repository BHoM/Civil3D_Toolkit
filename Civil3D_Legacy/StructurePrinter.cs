/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using Autodesk.Civil.DatabaseServices;
using System;

namespace Civil3D_Toolkit
{
    public class StructurePrinter : IObjectPrinter<Structure>
    {
        private readonly StructureDecoratorFactory _structureDecoratorFactory;

        public StructurePrinter(StructureDecoratorFactory structureDecoratorFactory)
        {
            _structureDecoratorFactory = structureDecoratorFactory;
        }

        public string[] Print(Structure oStructure)
        {
            StructureDecorator oDecoratedStructure = _structureDecoratorFactory.Create(oStructure);

            if (oDecoratedStructure.IsNull() || oDecoratedStructure.IsInletOutlet())
            {
                return new[]
                {
                oStructure.Name,
                Convert.ToString(Math.Round(oStructure.Position.X, 3)),
                Convert.ToString(Math.Round(oStructure.Position.Y, 3)),
                Convert.ToString(Math.Round(oStructure.Position.Z, 2)),
                "",
                "",
                oStructure.PartDescription
                };
            }

            return new[]
            {
                oStructure.Name,
                Convert.ToString(Math.Round(oStructure.Position.X, 3)),
                Convert.ToString(Math.Round(oStructure.Position.Y, 3)),
                Convert.ToString(Math.Round(oStructure.Position.Z, 2)),
                Convert.ToString(Math.Round(oStructure.SumpElevation, 2)),
                Convert.ToString(Math.Round(oStructure.RimToSumpHeight, 2)),
                oStructure.PartDescription,
                oDecoratedStructure.GetManholeInternalDimensions(),
                oDecoratedStructure.GetManholeClearOpeningDimensions(),
                oDecoratedStructure.GetCoverType(),
                oDecoratedStructure.GetLoadingClass(),
            };
        }
    }
}