/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
    public class PipePrinter : IObjectPrinter<Pipe>
    {
        private readonly PipeDecoratorFactory _pipeDecoratorFactory;

        public PipePrinter(PipeDecoratorFactory pipeDecoratorFactory)
        {
            _pipeDecoratorFactory = pipeDecoratorFactory;
        }

        public string[] Print(Pipe oPipe)
        {
            PipeDecorator oDecoratedPipe = _pipeDecoratorFactory.Create(oPipe);
            return new[]
            {
                oDecoratedPipe.GetPipeInternalDimensions(),
                Convert.ToString(oPipe.Length3DToInsideEdge),
                oDecoratedPipe.GetPipeGradient(),
                Convert.ToString(Math.Round(oPipe.EndPoint.Z - (oPipe.InnerDiameterOrWidth / 2), 2)),
                oDecoratedPipe.HasDownstreamStructure() ? oDecoratedPipe.GetDownstreamStructure().Name : ""
            };
        }
    }
}

