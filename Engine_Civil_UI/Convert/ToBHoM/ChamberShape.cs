using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHC = BH.oM.Civils.Elements;
using ADC = Autodesk.Civil.DatabaseServices;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {
        public static BHC.ChamberShape ToBHoM(this ADC.BoundingShapeType acBoundingShapeType)
        {
            switch (acBoundingShapeType)
            {
                case Autodesk.Civil.DatabaseServices.BoundingShapeType.Undefined:
                    return BHC.ChamberShape.Undefined;
                case Autodesk.Civil.DatabaseServices.BoundingShapeType.Cylinder:
                    return BHC.ChamberShape.Circular;
                case Autodesk.Civil.DatabaseServices.BoundingShapeType.Box:
                    return BHC.ChamberShape.Rectangular;
                case Autodesk.Civil.DatabaseServices.BoundingShapeType.Sphere:
                    return BHC.ChamberShape.Spherical;
                default:
                    return BHC.ChamberShape.Undefined;
            }
        }
    }
}