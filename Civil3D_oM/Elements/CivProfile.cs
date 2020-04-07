using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Civils.Elements
{
    public class CivProfile : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        public virtual string DataSourceName { get; set; } = "";

        public virtual string Description { get; set; } = "";

        public virtual string DisplayName { get; set; } = "";

        public virtual double ElevationMax { get; set; } = 0;

        public virtual double ElevationMin { get; set; } = 0;

        public virtual double EndingStation { get; set; } = 0;

        public virtual List<ICivProfileEntity> Entities { get; set; } = new List<ICivProfileEntity>();

        public virtual double Length { get; set; } = 0;

        public virtual double Offset { get; set; } = 0;

        public virtual CivProfileType ProfileType { get; set; } = CivProfileType.Undefined;

        public virtual double StartingStation { get; set; } = 0;

        /***************************************************/
    }
}

