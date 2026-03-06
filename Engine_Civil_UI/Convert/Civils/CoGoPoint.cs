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
using BHC = BH.oM.Civils.Elements;

using ACG = Autodesk.AutoCAD.Geometry;

using ADC = Autodesk.Civil.DatabaseServices;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHC.CoGoPoint FromCivil3D(this ADC.CogoPoint cogoPoint)
        {
            BHC.CoGoPoint pt = new BHC.CoGoPoint();

            try
            {
                pt.Easting = cogoPoint.Easting;
            }
            catch(Exception e)
            {
                BH.Engine.Base.Compute.RecordError("Error reading Easting - " + e.ToString());
            }

            try
            {
                pt.Elevation = cogoPoint.Elevation;
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError("Error reading Elevation - " + e.ToString());
            }

            try
            {
                pt.FullDescription = cogoPoint.FullDescription;
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError("Error reading FullDescription - " + e.ToString());
            }

            try
            {
                pt.GridEasting = cogoPoint.GridEasting;
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError("Error reading GridEasting - " + e.ToString());
            }

            try
            {
                pt.GridNorthing = cogoPoint.GridNorthing;
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError("Error reading GridNorthing - " + e.ToString());
            }

            try
            {
                pt.Latitude = cogoPoint.Latitude;
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError("Error reading Latitude - " + e.ToString());
            }

            try
            {
                pt.Longitude = cogoPoint.Longitude;
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError("Error reading Longitude - " + e.ToString());
            }

            try
            {
                pt.Northing = cogoPoint.Northing;
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError("Error reading Northing - " + e.ToString());
            }

            try
            {
                pt.PointName = cogoPoint.PointName;
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError("Error reading PointName - " + e.ToString());
            }

            try
            {
                pt.PointNumber = cogoPoint.PointNumber;
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError("Error reading PointNumber - " + e.ToString());
            }

            try
            {
                pt.RawDescription = cogoPoint.RawDescription;
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError("Error reading RawDescription - " + e.ToString());
            }

            try
            {
                pt.Location = cogoPoint.Location.FromCivil3D();
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError("Error reading Location - " + e.ToString());
            }

            return pt;
        }

        /***************************************************/
    }
}


