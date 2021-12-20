using System;
using Core.View;

namespace Core.Data.PlayerProgress
{
    [Serializable]
    public struct ViewProgress
    {
        public ISpriteResolverView SpriteResolverView;
        public SpriteResolverSkinData[] ResolverData;
    }
}