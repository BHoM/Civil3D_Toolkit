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

        public string DataSourceName { get; set; } = "";

        public string Description { get; set; } = "";

        public string DisplayName { get; set; } = "";

        public double ElevationMax { get; set; } = 0;

        public double ElevationMin { get; set; } = 0;

        public double EndingStation { get; set; } = 0;

        public double Length { get; set; } = 0;

        public double Offset { get; set; } = 0;

        public CivProfileType ProfileType { get; set; } = CivProfileType.Undefined;

        public double StartingStation { get; set; } = 0;

        /***************************************************/
    }
}

