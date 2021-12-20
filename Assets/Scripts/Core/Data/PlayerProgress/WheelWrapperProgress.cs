using System;
using Attributes;
using Core.Player;

namespace Core.Data.PlayerProgress
{
    [Serializable]
    public struct WheelWrapperProgress
    {
        [GUIReadOnly]
        public MonoWheel Wheel;
        public WheelProgress Progress;
    }
}