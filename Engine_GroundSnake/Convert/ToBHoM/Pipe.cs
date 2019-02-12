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

        public static BHC.Pipe ToBHoM(this ADC.Pipe acPipe)
        {
            return new BHC.Pipe
            {
                CentreLine = new BHG.Line { Start = acPipe.StartPoint.ToBHoM(), End = acPipe.EndPoint.ToBHoM() },
                Diameter = acPipe.InnerDiameterOrWidth,
                Thickness = acPipe.WallThickness,
                FlowDirection = acPipe.FlowDirectionMethod.ToBHoM(),
            };

        }

        /***************************************************/
    }
}
