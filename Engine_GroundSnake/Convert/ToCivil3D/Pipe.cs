using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHG = BH.oM.Geometry;
using BHC = BH.oM.Civils.Elements;
using ADC = Autodesk.Civil.DatabaseServices;

namespace BH.Engine.GroundSnake
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static ADC.Pipe ToCivil3D(this BHC.Pipe bhPipe)
        {
            throw new NotImplementedException();
        }

        /***************************************************/
    }
}
