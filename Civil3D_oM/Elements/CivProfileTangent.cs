using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Civils.Elements
{
    public class CivProfileTangent : BHoMObject, ICivProfileEntity
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        public CivProfileConstraintType Constraint1 { get; set; } = CivProfileConstraintType.Undefined;

        public CivProfileConstraintType Constraint2 { get; set; } = CivProfileConstraintType.Undefined;

        public double EndElevation { get; set; } = 0;

        public double EndStation { get; set; } = 0;

        public int EntityAfter { get; set; } = 0;

        public int EntityBefore { get; set; } = 0;

        public int EntityId { get; set; } = 0;

        public CivProfileEntityType EntityType { get; set; } = CivProfileEntityType.Undefined;

        public double Grade { get; set; } = 0;

        public double HighestDesignSpeed { get; set; } = 0;

        public double Length { get; set; } = 0;

        public double MinimumKValueHSD { get; set; } = 0;

        public double MinimumKValuePSD { get; set; } = 0;

        public double MinimumKValueSSD { get; set; } = 0;

        public double StartElevation { get; set; } = 0;

        public double StartStation { get; set; } = 0;

        public double ThroughPoint1Elevation { get; set; } = 0;

        public double ThroughPoint1Station { get; set; } = 0;

        public double ThroughPoint2Elevation { get; set; } = 0;

        public double ThroughPoint2Station { get; set; } = 0;

        /***************************************************/
    }
}



