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

        public static BHC.FeatureLine FromCivil3D(this ADC.FeatureLine civFeatureLine)
        {
            return new BHC.FeatureLine
            {
                Description = civFeatureLine.Description,
                Length2D = civFeatureLine.Length2D,
                Length3D = civFeatureLine.Length3D,
                MaxElevation = civFeatureLine.MaxElevation,
                MaxGrade = civFeatureLine.MaxGrade,
                MinElevation = civFeatureLine.MinElevation,
                MinGrade = civFeatureLine.MinGrade,
            };
        }

        /***************************************************/
    }
}
