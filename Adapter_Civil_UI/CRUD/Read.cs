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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Dimensional;
using BHC = BH.oM.Civils.Elements;
using BHG = BH.oM.Geometry;
using BH.UI.Civil.Engine;
//using BH.oM.Geometry;
using BH.Engine;
using BH.oM.Civils.Fragments;
using BH.Engine.Base;

using BH.oM.Data.Requests;

using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.Runtime;
using ADC = Autodesk.Civil.DatabaseServices;
using AAD = Autodesk.AutoCAD.DatabaseServices;
using ACG = Autodesk.AutoCAD.Geometry;
using BCF = BH.oM.Civils.Fragments;

//using Autodesk.AutoCAD.Runtime;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Internal.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using BH.oM.Adapter;

namespace BH.UI.Civil.Adapter
{
    public partial class CivilUIAdapter
    {

        /***************************************************/
        /**** Adapter Methods                           ****/
        /***************************************************/

        //General method called by the adapter when reading in data

        protected override IEnumerable<IBHoMObject> Read(FilterRequest request, ActionConfig actionConfig = null)
        {
            try
            {

                Debugger.Log("=== Read(FilterRequest) ENTER ===");
                
                // Prove which DLL is actually running
                Debugger.Log("Assembly: " + typeof(CivilUIAdapter).Assembly.Location);


                if (request == null)
                
                {
                    Debugger.Log("Request is NULL");
                    return new List<BH.oM.Base.IBHoMObject>();
                }


                Type requestedType = request.Type;
                Debugger.Log("RequestedType: " + (requestedType == null ? "NULL" : requestedType.FullName));

                
                if (requestedType == null)
                    return new List<BH.oM.Base.IBHoMObject>();


                CivilDocument civdoc = CivilApplication.ActiveDocument;

                var doc = Application.DocumentManager.MdiActiveDocument;
                var db = doc.Database;
                
                var reader = new BH.UI.Civil.Adapter.C3DReader();

                return reader.ReadByType(requestedType, db, civdoc);

            }
            
            catch (Exception ex)
            {
                Debugger.Log("EXCEPTION in Read(FilterRequest): " + ex);
                BH.Engine.Base.Compute.RecordError("Read(FilterRequest) failed: " + ex.Message);
                return new List<IBHoMObject>();
            }
            finally
            {
                Debugger.Log("=== Read(FilterRequest) EXIT ===");
            }

        }

        
    }
}


