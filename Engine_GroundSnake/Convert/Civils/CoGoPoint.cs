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
            return new BHC.CoGoPoint
            {
                Easting = cogoPoint.Easting,
                Elevation = cogoPoint.Elevation,
                FullDescription = cogoPoint.FullDescription,
                GridEasting = cogoPoint.GridEasting,
                GridNorthing = cogoPoint.GridNorthing,
                Latitude = cogoPoint.Latitude,
                Longitude = cogoPoint.Longitude,
                Northing = cogoPoint.Northing,
                PointName = cogoPoint.PointName,
                PointNumber = cogoPoint.PointNumber,
                RawDescription = cogoPoint.RawDescription,
                Location = cogoPoint.Location.FromCivil3D(),
            };
        }

        /***************************************************/
    }
}
