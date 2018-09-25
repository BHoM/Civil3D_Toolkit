using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using System;

namespace Civil3D_Toolkit
{
    public class PipeDecorator
    {
        private readonly Pipe _oPipe;
        private readonly Civil3DObjectRetriever _objectRetriever;

        public PipeDecorator(Pipe oPipe, Civil3DObjectRetriever objectRetriever)
        {
            _oPipe = oPipe;
            _objectRetriever = objectRetriever;
        }

        public Pipe Pipe
        {
            get
            {
                return _oPipe;
            }
        }

        public bool HasDownstreamStructure()
        {
            return !(_oPipe.EndStructureId.IsNull);
        }

        public Structure GetDownstreamStructure()
        {
            if (!HasDownstreamStructure())
            {
                throw new Exception("No downstream structure");
            }
            return _objectRetriever.GetStructure(_oPipe.EndStructureId);
        }

        private Structure GetUpstreamStructure()
        {
            return _objectRetriever.GetStructure(_oPipe.StartStructureId);
        }

        public string GetPipeInternalDimensions()
        {
            // Returns pipe diameter for circular pipes, or width x height for any other shape of pipe.
            if (_oPipe.CrossSectionalShape == SweptShapeType.Circular)
            {
                return (Convert.ToString(_oPipe.InnerDiameterOrWidth * 1000));
            }

            return (Convert.ToString(_oPipe.InnerDiameterOrWidth * 1000) + " x " + Convert.ToString(_oPipe.InnerHeight * 1000));
        }

        public string GetPipeGradient()
        {
            // Returns gradient X where gradient is represented by 1 in X.
            // If gradient is flatter than 1 in 1000, returns "Horizontal".
            double gradient = 1 / ((_oPipe.StartPoint.Z - _oPipe.EndPoint.Z) / _oPipe.Length2DCenterToCenter);

            if (gradient > 1000)
            {
                return "Horizontal";
            }

            return Convert.ToString(gradient);
        }
    }
}
