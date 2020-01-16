using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHC = BH.oM.Civils.Elements;

using ACG = Autodesk.AutoCAD.Geometry;

using ADC = Autodesk.Civil.DatabaseServices;

using Autodesk.AutoCAD.DatabaseServices;

namespace BH.UI.Civil.Engine
{
    public static partial class Convert
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHC.Block FromCivil3D(this BlockReference civBlock)
        {
            return new BHC.Block
            {
                BlockName = civBlock.BlockName,
                CanCastShadow = civBlock.CastShadows,
                IsPersistent = civBlock.IsPersistent,
                IsPlanar = civBlock.IsPlanar,
                Layer = civBlock.Layer,
                Material = civBlock.Material,
                CanReceiveShadow = civBlock.ReceiveShadows,
                Rotation = civBlock.Rotation,
                IsVisible = civBlock.Visible,
                Position = civBlock.Position.FromCivil3D(),
                Normal = civBlock.Normal.FromCivil3D(),
                CollisionType = civBlock.CollisionType.FromCivil3D(),
                Bounds = civBlock.Bounds.HasValue ? civBlock.Bounds.Value.FromCivil3D() : new oM.Geometry.BoundingBox(),
                Name = civBlock.Name,
            };
        }

        /***************************************************/
    }
}
