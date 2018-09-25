using Autodesk.Civil.DatabaseServices;
using System;

namespace Civil3D_Toolkit
{
    public class StructurePrinter : IObjectPrinter<Structure>
    {
        private readonly StructureDecoratorFactory _structureDecoratorFactory;

        public StructurePrinter(StructureDecoratorFactory structureDecoratorFactory)
        {
            _structureDecoratorFactory = structureDecoratorFactory;
        }

        public string[] Print(Structure oStructure)
        {
            StructureDecorator oDecoratedStructure = _structureDecoratorFactory.Create(oStructure);

            if (oDecoratedStructure.IsNull() || oDecoratedStructure.IsInletOutlet())
            {
                return new[]
                {
                oStructure.Name,
                Convert.ToString(Math.Round(oStructure.Position.X, 3)),
                Convert.ToString(Math.Round(oStructure.Position.Y, 3)),
                Convert.ToString(Math.Round(oStructure.Position.Z, 2)),
                "",
                "",
                oStructure.PartDescription
                };
            }

            return new[]
            {
                oStructure.Name,
                Convert.ToString(Math.Round(oStructure.Position.X, 3)),
                Convert.ToString(Math.Round(oStructure.Position.Y, 3)),
                Convert.ToString(Math.Round(oStructure.Position.Z, 2)),
                Convert.ToString(Math.Round(oStructure.SumpElevation, 2)),
                Convert.ToString(Math.Round(oStructure.RimToSumpHeight, 2)),
                oStructure.PartDescription,
                oDecoratedStructure.GetManholeInternalDimensions(),
                oDecoratedStructure.GetManholeClearOpeningDimensions(),
                oDecoratedStructure.GetCoverType(),
                oDecoratedStructure.GetLoadingClass(),
            };
        }
    }
}