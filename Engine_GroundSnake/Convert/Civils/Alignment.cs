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

        public static BHC.Alignment FromCivil3D(this ADC.Alignment civAlignment)
        {
            BHC.Alignment alignment = new BHC.Alignment
            {
                AlignmentType = civAlignment.AlignmentType.FromCivil3D(),
                Description = civAlignment.Description,
                DesignSpeeds = civAlignment.DesignSpeeds.Select(x => x.Value).ToList(),
                EndingStation = civAlignment.EndingStation,
                Length = civAlignment.Length,
                Name = civAlignment.Name,
                ReferencePoint = civAlignment.ReferencePoint.FromCivil3D(),
                ReferencePointStation = civAlignment.ReferencePointStation,
                SiteName = civAlignment.SiteName,
                StartingStation = civAlignment.StartingStation,
            };

            foreach(ADC.AlignmentEntity aEntity in civAlignment.Entities)
            {
                switch(aEntity.EntityType)
                {
                    case ADC.AlignmentEntityType.Line:
                        alignment.AlignmentCurves.Add(FromCivil3D(aEntity as dynamic));
                        break;
                    default:
                        alignment.AlignmentCurves.Add(null);
                        break;
                }
            }

            return alignment;
        }

        /***************************************************/
    }
}
