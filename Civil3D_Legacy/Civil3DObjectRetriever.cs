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
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using System.Collections.Generic;

namespace Civil3D_Toolkit
{
    public class Civil3DObjectRetriever
    {
        //Test comment
        private readonly TransactionManager _transactionManager;
        private static IComparer<Structure> _comparer = new CustomStuctureComparer();

        public Civil3DObjectRetriever(TransactionManager transactionManager)
        {
            _transactionManager = transactionManager;
        }

        public Alignment GetAlignment(ObjectId alignmentId, OpenMode openMode = OpenMode.ForRead)
        {
            return _transactionManager.GetObject(alignmentId, openMode) as Alignment;
        }

        public Structure GetStructure(ObjectId structureId, OpenMode openMode = OpenMode.ForRead)
        {
            return _transactionManager.GetObject(structureId, openMode) as Structure;
        }

        public Pipe GetPipe(ObjectId pipeId, OpenMode openMode = OpenMode.ForRead)
        {
            return _transactionManager.GetObject(pipeId, openMode) as Pipe;
        }

        public Network GetPipeNetwork(ObjectId networkId, OpenMode openMode = OpenMode.ForRead)
        {
            return _transactionManager.GetObject(networkId, openMode) as Network;
        }

        public ObjectIdCollection GetStructureIdCollection(Network oNetwork)
        {
            return oNetwork.GetStructureIds();
        }

        public List<Structure> GetSortedStructureList(Network oNetwork, IComparer<Structure> comparer = null)
        {
            comparer = comparer ?? _comparer;

            List<Structure> structureList = new List<Structure>();

            foreach (ObjectId structureId in GetStructureIdCollection(oNetwork))
            {
                structureList.Add(GetStructure(structureId));
            }

            structureList.Sort(comparer);

            return structureList;
        }

        public ObjectIdCollection GetPipeIdCollection(Network oNetwork)
        {
            return oNetwork.GetPipeIds();
        }

        public ObjectIdCollection GetPipeNetworkIdCollection()
        {
            return CivilApplication.ActiveDocument.GetPipeNetworkIds();
        }

        public DataLink GetDataLink(ObjectId dlId, OpenMode openMode = OpenMode.ForRead)
        {
            return _transactionManager.GetObject(dlId, openMode) as DataLink;
        }
    }
}
