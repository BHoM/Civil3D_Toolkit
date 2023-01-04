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

namespace Civil3D_Toolkit
{
    class DataLinkDecorator
    {
        private readonly Civil3DObjectRetriever _objectRetriever;
        private readonly DataLinkManager _dataLinkManager;

        public DataLinkDecorator(Civil3DObjectRetriever objectRetriever, DataLinkManager dataLinkManager)
        {
            _objectRetriever = objectRetriever;
            _dataLinkManager = dataLinkManager;
        }

        public DataLink CreateDataLink(string dataLinkName, string dataLinkPath)
        {
            // Creates a DataLink object with the members it needs to be added to a drawing. Doesn't add it to the database.
            DataLink dl = new DataLink();
            dl.ConnectionString = dataLinkPath;
            dl.DataAdapterId = "AcExcel";
            dl.DataLinkOption = DataLinkOption.PersistCache;
            dl.Description = dataLinkName + " Automatic DataLink";
            dl.Name = dataLinkName;
            dl.ToolTip = "Automatic DataLink for " + dataLinkName;
            dl.UpdateOption = (int)UpdateOption.UpdateColumnWidth;
            ObjectId dlId = _dataLinkManager.AddDataLink(dl);
            return dl;
        }

        public void UpdateDataLink(string dataLinkName)
        {
            // Updates an existing DataLink assuming it is present in the drawing.
            ObjectId dlId = _dataLinkManager.GetDataLink(dataLinkName);
            DataLink dl = _objectRetriever.GetDataLink(dlId, OpenMode.ForWrite);
            dl.Update(UpdateDirection.SourceToData, UpdateOption.UpdateColumnWidth);
        }

        public bool DataLinkNameExistsInDrawing(string dataLinkName)
        {
            // Returns true if a DataLink with the name passed in is present in a drawing.
            return (_dataLinkManager.GetDataLink(dataLinkName) != ObjectId.Null);
        }
    }
}

