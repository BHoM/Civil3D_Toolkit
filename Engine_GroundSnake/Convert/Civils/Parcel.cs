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

        public static BHC.Parcel FromCivil3D(this ADC.Parcel civParcel)
        {
            return new BHC.Parcel
            {
                Area = civParcel.Area,
                AreaLocation = civParcel.AreaLocation.FromCivil3D(),
                Centroid = civParcel.Centroid.FromCivil3D(),
                Description = civParcel.Description,
                Name = civParcel.Name,
                Number = civParcel.Number,
            };
        }

        /***************************************************/
    }
}
