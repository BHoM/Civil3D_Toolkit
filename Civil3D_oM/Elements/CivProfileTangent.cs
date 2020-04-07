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

        public virtual CivProfileConstraintType Constraint1 { get; set; } = CivProfileConstraintType.Undefined;

        public virtual CivProfileConstraintType Constraint2 { get; set; } = CivProfileConstraintType.Undefined;

        public virtual double EndElevation { get; set; } = 0;

        public virtual double EndStation { get; set; } = 0;

        public virtual int EntityAfter { get; set; } = 0;

        public virtual int EntityBefore { get; set; } = 0;

        public virtual int EntityId { get; set; } = 0;

        public virtual CivProfileEntityType EntityType { get; set; } = CivProfileEntityType.Undefined;

        public virtual double Grade { get; set; } = 0;

        public virtual double HighestDesignSpeed { get; set; } = 0;

        public virtual double Length { get; set; } = 0;

        public virtual double MinimumKValueHSD { get; set; } = 0;

        public virtual double MinimumKValuePSD { get; set; } = 0;

        public virtual double MinimumKValueSSD { get; set; } = 0;

        public virtual double StartElevation { get; set; } = 0;

        public virtual double StartStation { get; set; } = 0;

        public virtual double ThroughPoint1Elevation { get; set; } = 0;

        public virtual double ThroughPoint1Station { get; set; } = 0;

        public virtual double ThroughPoint2Elevation { get; set; } = 0;

        public virtual double ThroughPoint2Station { get; set; } = 0;

        /***************************************************/
    }
}



