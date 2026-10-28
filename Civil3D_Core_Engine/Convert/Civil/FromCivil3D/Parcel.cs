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

using BHC = BH.oM.Adapters.Civil3D.Elements;
using BHG = BH.oM.Geometry;

using ACD = Autodesk.AutoCAD.DatabaseServices;

using ADC = Autodesk.Civil.DatabaseServices;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHC.Parcel FromCivil3D(this ADC.Parcel civParcel)
        {
            return new BHC.Parcel
            {
                AreaLocation = civParcel.AreaLocation.FromCivil3D(),
                Boundary = ParcelBoundary(civParcel),
                Description = civParcel.Description,
                Name = civParcel.Name,
                Number = civParcel.Number,
            };
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static BHG.ICurve ParcelBoundary(ADC.Parcel civParcel)
        {
            ACD.Curve baseCurve = civParcel.BaseCurve;
            BHG.ICurve boundary = baseCurve.IFromCivil3D(null) as BHG.ICurve;

            if (boundary == null)
                BH.Engine.Base.Compute.RecordWarning($"The boundary of parcel {civParcel.Name} could not be converted, so it was left empty.");

            return boundary;
        }

        /***************************************************/
    }
}


