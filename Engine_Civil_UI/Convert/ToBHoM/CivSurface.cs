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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHG = BH.oM.Geometry;
using BHC = BH.oM.Civils.Elements;
using ADC = Autodesk.Civil.DatabaseServices;
using BH.Engine.Geometry;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHC.CivSurface ToBHoM(this ADC.TinSurface acSurface)
        {
            //Converting a Triangulated Irregular Network Surface (TinSurface)
            List<BHG.Polyline> polylines = new List<oM.Geometry.Polyline>();

            foreach (ADC.TinSurfaceTriangle triangle in acSurface.Triangles)
                polylines.Add(triangle.ToBHoM());

            return new BHC.CivSurface
            {
                Triangles = polylines,
            };
        }

        public static BHG.Polyline ToBHoM(this ADC.TinSurfaceTriangle triangle)
        {
            List<BHG.Point> pts = new List<oM.Geometry.Point>();
            pts.Add(triangle.Vertex1.Location.FromCivil3D());
            pts.Add(triangle.Vertex2.Location.FromCivil3D());
            pts.Add(triangle.Vertex3.Location.FromCivil3D());
            pts.Add(pts.First());
            return BH.Engine.Geometry.Create.Polyline(pts);
        }

        public static BHC.CivSurface ToBHoM(this ADC.TinVolumeSurface acSurface)
        {
            List<BHG.Polyline> pLines = new List<oM.Geometry.Polyline>();
            
            return new BHC.CivSurface
            {
                Triangles = pLines,
            };
        }
    }
}

