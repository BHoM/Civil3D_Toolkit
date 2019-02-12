using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHC = BH.oM.Civils.Elements;
using ADC = Autodesk.Civil.DatabaseServices;

namespace BH.Engine.GroundSnake
{
    public static partial class Convert
    {
        public static BHC.FlowDirection ToBHoM(this ADC.FlowDirectionMethodType acFlowDirectionType)
        {
            switch (acFlowDirectionType)
            {
                case Autodesk.Civil.DatabaseServices.FlowDirectionMethodType.Bidirectional:
                    return BHC.FlowDirection.Bidirectional;
                case Autodesk.Civil.DatabaseServices.FlowDirectionMethodType.BySlope:
                    return BHC.FlowDirection.BySlope;
                case Autodesk.Civil.DatabaseServices.FlowDirectionMethodType.EndToStart:
                    return BHC.FlowDirection.EndToStart;
                case Autodesk.Civil.DatabaseServices.FlowDirectionMethodType.StartToEnd:
                    return BHC.FlowDirection.StartToEnd;
                default:
                    return BHC.FlowDirection.Undefined;
            }
        }
    }
}
