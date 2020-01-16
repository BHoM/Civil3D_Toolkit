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

        public static BHC.CivAlignmentType FromCivil3D(this ADC.AlignmentType alignmentType)
        {
            switch (alignmentType)
            {
                case ADC.AlignmentType.Centerline:
                    return BHC.CivAlignmentType.Centerline;
                case ADC.AlignmentType.CurbReturn:
                    return BHC.CivAlignmentType.CurbReturn;
                case ADC.AlignmentType.Offset:
                    return BHC.CivAlignmentType.Offset;
                case ADC.AlignmentType.Rail:
                    return BHC.CivAlignmentType.Rail;
                case ADC.AlignmentType.Utility:
                    return BHC.CivAlignmentType.Utility;
                default:
                    return BHC.CivAlignmentType.Undefined;
            }
        }

        /***************************************************/
    }
}
