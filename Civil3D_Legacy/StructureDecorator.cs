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
using System.Collections.Generic;

namespace Civil3D_Toolkit
{
    public class StructureDecorator
    {
        private readonly Structure _oStructure;
        private readonly Civil3DObjectRetriever _objectRetriever;

        public StructureDecorator(Structure oStructure, Civil3DObjectRetriever objectRetriever)
        {
            _oStructure = oStructure;
            _objectRetriever = objectRetriever;
        }

        public Structure Structure
        {
            get
            {
                return _oStructure;
            }
        }

        public string GetManholeInternalDimensions()
        {
            // Returns a string of either length * width or diameter for structure.
            if (_oStructure.BoundingShape == BoundingShapeType.Box)
            {
                return (Convert.ToString(_oStructure.InnerLength * 1000) + " x " + Convert.ToString(_oStructure.InnerDiameterOrWidth * 1000));
            }  
            else if (_oStructure.BoundingShape == BoundingShapeType.Cylinder)
            {
                return (Convert.ToString(_oStructure.InnerDiameterOrWidth * 1000) + " DIA.");
            }
            throw new Exception("Structure " + _oStructure.Name + " is not a valid shape. Structures must be Box or Cylinder");  
        }

        public string GetManholeClearOpeningDimensions()
        {
            // Returns a string of either frame length * width or just diameter for clear opening.
            if (_oStructure.PartData.GetDataFieldBy(PartContextType.StructFrameDiameter).Value.ToString() == "0")
            {
                return (_oStructure.PartData.GetDataFieldBy(PartContextType.StructFrameWidth).Value + " x " + _oStructure.PartData.GetDataFieldBy(PartContextType.StructFrameLength).Value);
            }
                
            else
                return (_oStructure.PartData.GetDataFieldBy(PartContextType.StructFrameDiameter).Value + " DIA.");
        }

        public List<Pipe> GetConnectedPipes()
        {
            // Returns a list of the pipes connected to the structure.
            List<Pipe> connectedPipesList = new List<Pipe>();
            Dictionary<string, Pipe> dictPipeNames = new Dictionary<string, Pipe>();

            foreach (ObjectId pipeId in _objectRetriever.GetPipeIdCollection(_objectRetriever.GetPipeNetwork(_oStructure.NetworkId)))
            {
                Pipe oPipe = _objectRetriever.GetPipe(pipeId);
                dictPipeNames.Add(oPipe.Name, oPipe);
            }

            string[] connectedPipeArray = _oStructure.GetConnectedPipeNames();

            foreach (string pipeName in connectedPipeArray)
            {
                connectedPipesList.Add(dictPipeNames[pipeName]);
            }

            return connectedPipesList;
        }

        public Pipe GetOutgoingPipe()
        {
            // Returns the outgoing pipe from a structure.
            if (!HasOutgoingPipe())
                throw new Exception("No outgoing pipe");

            int index = 0;
            foreach (Pipe oPipe in GetConnectedPipes())
            {
                if (_oStructure.IsConnectedPipeFlowingOut(index))
                {
                    return oPipe;
                }
                ++index;
            }
            throw new Exception("No outgoing pipe.");
        }

        public bool HasOutgoingPipe()
        {
            // Returns true if a structure has an outgoing pipe, otherwise false.
            int index = 0;
            foreach (Pipe oPipe in GetConnectedPipes())
            {
                if (_oStructure.IsConnectedPipeFlowingOut(index))
                {
                    return true;
                }
                ++index;
            }
            return false;
        }

        public bool IsNull()
        {
            // Returns true if a structure is a null structure
            if (_oStructure.PartType == PartType.StructNull)
            {
                return true;
            }
            return false;
        }

        public bool IsInletOutlet()
        {
            // Returns true if a structure is an inlet-outlet structure e.g. a headwall
            if (_oStructure.PartType == PartType.StructInletOutlet)
            {
                return true;
            }
            return false;
        }
        public string GetCoverType()
        {

            // Returns the Cover Type from the manhole parameter SF - Frame 

            return _oStructure.Frame;
        }
        public string GetLoadingClass()
        {
            // Returns the Loading Class from the manhole parameter SC_Cover 
            return _oStructure.Cover;
        }
    }
}

