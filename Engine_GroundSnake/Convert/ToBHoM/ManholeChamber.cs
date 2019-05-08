using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHG = BH.oM.Geometry;
using BHC = BH.oM.Civils.Elements;
using ADC = Autodesk.Civil.DatabaseServices;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHC.ManholeChamber ToBHoM(this ADC.Structure acStructure)
        {
            return new BHC.ManholeChamber
            {
                CentrePoint = acStructure.Location.ToBHoM(),
                InternalLength = acStructure.InnerDiameterOrWidth,
                //InternalWidth = acStructure.InnerLength,
                InternalDepth = acStructure.Height,
                //ToDo: Find appropriate data for WallThickness
                WallThickness = acStructure.FloorThickness,
                //ToDo: Find appropriate data for SurroundThickness 
                //SurroundThickness = acStructure.HeadwallBaseWidth,
                ChamberOrientation = acStructure.Rotation,
                ChamberShape = acStructure.BoundingShape.ToBHoM(),
                //ToDo: Decide whether to condense into one frame diameter property or keep separate
                CoverLength = acStructure.FrameDiameter,
                CoverWidth = acStructure.FrameDiameter,
                CoverDepth = acStructure.FrameHeight,
                //ToDo: Investigate whether it is possible to separate cover orientation and chamber orientation in Civil3D
                CoverOrientation = acStructure.Rotation,
                //BeddingDepth = acStructure.HeadwallBaseThickness
            };
        }

        /***************************************************/
    }
}
