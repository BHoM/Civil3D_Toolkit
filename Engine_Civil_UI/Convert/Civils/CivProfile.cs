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

        public static BHC.CivProfile FromCivil3D(this ADC.Profile civProfile)
        {
            return new BHC.CivProfile
            {
                DataSourceName = civProfile.DataSourceName,
                Description = civProfile.Description,
                DisplayName = civProfile.DisplayName,
                ElevationMax = civProfile.ElevationMax,
                ElevationMin = civProfile.ElevationMin,
                EndingStation = civProfile.EndingStation,
                Length = civProfile.Length,
                Name = civProfile.Name,
                Offset = civProfile.Offset,
                ProfileType = civProfile.ProfileType.FromCivil3D(),
                StartingStation = civProfile.StartingStation,
            };
        }

        /***************************************************/
    }
}
