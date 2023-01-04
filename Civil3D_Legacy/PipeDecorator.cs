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

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using System;

namespace Civil3D_Toolkit
{
    public class PipeDecorator
    {
        private readonly Pipe _oPipe;
        private readonly Civil3DObjectRetriever _objectRetriever;

        public PipeDecorator(Pipe oPipe, Civil3DObjectRetriever objectRetriever)
        {
            _oPipe = oPipe;
            _objectRetriever = objectRetriever;
        }

        public Pipe Pipe
        {
            get
            {
                return _oPipe;
            }
        }

        public bool HasDownstreamStructure()
        {
            return !(_oPipe.EndStructureId.IsNull);
        }

        public Structure GetDownstreamStructure()
        {
            if (!HasDownstreamStructure())
            {
                throw new Exception("No downstream structure");
            }
            return _objectRetriever.GetStructure(_oPipe.EndStructureId);
        }

        private Structure GetUpstreamStructure()
        {
            return _objectRetriever.GetStructure(_oPipe.StartStructureId);
        }

        public string GetPipeInternalDimensions()
        {
            // Returns pipe diameter for circular pipes, or width x height for any other shape of pipe.
            if (_oPipe.CrossSectionalShape == SweptShapeType.Circular)
            {
                return (Convert.ToString(_oPipe.InnerDiameterOrWidth * 1000));
            }

            return (Convert.ToString(_oPipe.InnerDiameterOrWidth * 1000) + " x " + Convert.ToString(_oPipe.InnerHeight * 1000));
        }

        public string GetPipeGradient()
        {
            // Returns gradient X where gradient is represented by 1 in X.
            // If gradient is flatter than 1 in 1000, returns "Horizontal".
            double gradient = 1 / ((_oPipe.StartPoint.Z - _oPipe.EndPoint.Z) / _oPipe.Length2DCenterToCenter);

            if (gradient > 1000)
            {
                return "Horizontal";
            }

            return Convert.ToString(gradient);
        }
    }
}

