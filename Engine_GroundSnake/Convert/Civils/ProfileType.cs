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

        public static BHC.CivProfileType FromCivil3D(this ADC.ProfileType profileType)
        {
            switch(profileType)
            {
                case ADC.ProfileType.CorridorFeature:
                    return BHC.CivProfileType.CorridorFeature;
                case ADC.ProfileType.CurbReturnProfile:
                    return BHC.CivProfileType.CurbReturnProfile;
                case ADC.ProfileType.EG:
                    return BHC.CivProfileType.EG;
                case ADC.ProfileType.FG:
                    return BHC.CivProfileType.FG;
                case ADC.ProfileType.File:
                    return BHC.CivProfileType.File;
                case ADC.ProfileType.OffsetProfile:
                    return BHC.CivProfileType.OffsetProfile;
                case ADC.ProfileType.Superimposed:
                    return BHC.CivProfileType.Superimposed;
                default:
                    return BHC.CivProfileType.Undefined;
            }
        }

        /***************************************************/
    }
}
