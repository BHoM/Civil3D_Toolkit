﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHG = BH.oM.Geometry;

using ACG = Autodesk.AutoCAD.Geometry;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHG.Point FromCivil3D(this ACG.Point3d pt)
        {
            return new BHG.Point { X = pt.X, Y = pt.Y, Z = pt.Z };
        }

        /***************************************************/
    }
}