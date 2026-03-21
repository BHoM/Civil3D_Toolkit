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

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;

namespace BH.oM.Adapters.Civil3D
{
    public class Civil3DRuntimeContext
    {
        public CivilDocument CivilDocument { get; }
        public Database Database { get; }
        public Transaction Transaction { get; }

        public Document Document { get; }

        public BlockTable BlockTable { get; }
        public BlockTableRecord BlockTableRecord { get; }

        // public Civil3DRuntimeContext(CivilDocument civilDoc, Database db, Transaction tr)
        // {
        //     CivilDocument = civilDoc;
        //     Database = db;
        //     Transaction = tr;
        // }
        public Civil3DRuntimeContext(Document acDoc, CivilDocument civilDoc, Database db, Transaction tr, BlockTable acBlkTbl, BlockTableRecord acBlkTblRec)
        {
            Document = acDoc;
            CivilDocument = civilDoc;
            Database = db;
            Transaction = tr;

            BlockTable = acBlkTbl;
            BlockTableRecord = acBlkTblRec;
        }
    }
}



