using Autodesk.Civil.DatabaseServices;
using System;

namespace Civil3D_Toolkit
{
    public class PipePrinter : IObjectPrinter<Pipe>
    {
        private readonly PipeDecoratorFactory _pipeDecoratorFactory;

        public PipePrinter(PipeDecoratorFactory pipeDecoratorFactory)
        {
            _pipeDecoratorFactory = pipeDecoratorFactory;
        }

        public string[] Print(Pipe oPipe)
        {
            PipeDecorator oDecoratedPipe = _pipeDecoratorFactory.Create(oPipe);
            return new[]
            {
                oDecoratedPipe.GetPipeInternalDimensions(),
                Convert.ToString(oPipe.Length3DToInsideEdge),
                oDecoratedPipe.GetPipeGradient(),
                Convert.ToString(Math.Round(oPipe.EndPoint.Z - (oPipe.InnerDiameterOrWidth / 2), 2)),
                oDecoratedPipe.HasDownstreamStructure() ? oDecoratedPipe.GetDownstreamStructure().Name : ""
            };
        }
    }
}
