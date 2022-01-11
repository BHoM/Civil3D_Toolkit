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
