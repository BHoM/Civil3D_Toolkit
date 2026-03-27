using System;
using System.Collections.Generic;
using BH.oM.Base;

namespace BH.oM.Civils.Elements
{
    public class InfrastructureProjectRainbowDefinitions : BHoMObject
    {
        public virtual List<InfrastructureSystemRainbowDefinitions> Systems { get; set; }
            = new List<InfrastructureSystemRainbowDefinitions>();
    }

    public class InfrastructureSystemRainbowDefinitions : BHoMObject
    {
        public virtual string SystemName { get; set; } = ""; // e.g. "Potable", "Foul", "Power", etc. (if relevant)
        public virtual List<InfrastructureSystemSectionDefinitions> Sections { get; set; }
            = new List<InfrastructureSystemSectionDefinitions>();
    }

    public class InfrastructureSystemSectionDefinitions : BHoMObject
    {
        public virtual string SectionId { get; set; } = "";

        public virtual List<InfrastructureSystemSectionOffset> Offsets { get; set; }
            = new List<InfrastructureSystemSectionOffset>();

        public virtual List<InfrastructureSystemSectionCorridorWidth> CorridorWidths { get; set; }
            = new List<InfrastructureSystemSectionCorridorWidth>();
    }

    public class InfrastructureSystemSectionOffset : BHoMObject
    {
        public virtual double? LateralOffset { get; set; }
    }

    public class InfrastructureSystemSectionCorridorWidth : BHoMObject
    {
        public virtual double? TotalWidth { get; set; }
    }
}