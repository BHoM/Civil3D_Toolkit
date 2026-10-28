/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

using BH.Engine.Geometry;
using BH.oM.Base.Attributes;
using BHG = BH.oM.Geometry;

using ACD = Autodesk.AutoCAD.DatabaseServices;
using ACG = Autodesk.AutoCAD.Geometry;

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Civil3D.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Converts BH.oM.Geometry.Arc to a Civil 3D Arc.")]
        [Input("arc", "BH.oM.Geometry.Arc to convert.")]
        [Output("arc", "The converted Civil 3D Arc.")]
        public static ACD.Arc ToCivil3D(this BHG.Arc arc)
        {
            if (arc == null)
                return null;

            //Three points fix the plane, radius and direction, so AutoCAD works out the OCS angles itself
            ACG.Point3d start = arc.StartPoint().ToCivil3D();
            ACG.Point3d mid = arc.PointAtParameter(0.5).ToCivil3D();
            ACG.Point3d end = arc.EndPoint().ToCivil3D();

            ACG.CircularArc3d geArc = new ACG.CircularArc3d(start, mid, end);
            ACD.Arc acArc = ACD.Curve.CreateFromGeCurve(geArc) as ACD.Arc;

            if (acArc == null)
                BH.Engine.Base.Compute.RecordError("Could not convert an Arc to a Civil 3D arc.");

            return acArc;
        }

        /***************************************************/
    }
}
