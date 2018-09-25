using Autodesk.Civil.DatabaseServices;

namespace Civil3D_Toolkit
{
    public class StructureDecoratorFactory
    {
        private readonly Civil3DObjectRetriever _objectRetriever;

        public StructureDecoratorFactory(Civil3DObjectRetriever objectRetriever)
        {
            _objectRetriever = objectRetriever;
        }

        public StructureDecorator Create(Structure oStructure)
        {
            return new StructureDecorator(oStructure, _objectRetriever);
        }
    }
}
