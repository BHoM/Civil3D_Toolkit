using Autodesk.Civil.DatabaseServices;

namespace Civil3D_Toolkit
{
    public class PipeDecoratorFactory
    {
        private readonly Civil3DObjectRetriever _objectRetriever;

        public PipeDecoratorFactory(Civil3DObjectRetriever objectRetriever)
        {
            _objectRetriever = objectRetriever;
        }

        public PipeDecorator Create(Pipe oPipe)
        {
            return new PipeDecorator(oPipe, _objectRetriever);
        }
    }
}
