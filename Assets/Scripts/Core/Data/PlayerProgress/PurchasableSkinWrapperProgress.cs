using System;
using Attributes;
using Core.Skins;

namespace Core.Data.PlayerProgress
{
    [Serializable]
    public struct PurchasableSkinWrapperProgress
    {
        [GUIReadOnly]
        public PurchasableSkin Skin;
        public PurchasableSkinProgress Progress;
    }
}