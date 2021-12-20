using System;
using Core.Player.PlayerSO;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Data.PlayerProgress
{
    [Serializable]
    public struct CharacterProgress
    {
        [SerializeField]
        private CharacterModifiersPoints _modifierPoints;

        public CharacterProgress(CharacterModifiersPoints modifiersPoints)
        {
            _modifierPoints = modifiersPoints;
        }

        public CharacterModifiersPoints ModifierPoints => _modifierPoints;
    }
}