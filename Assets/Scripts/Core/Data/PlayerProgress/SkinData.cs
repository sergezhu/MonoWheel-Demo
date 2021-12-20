using System;
using UnityEngine;

namespace Core.Data.PlayerProgress
{
    [Serializable]
    public struct SkinData
    {
        [SerializeField]
        private string _skinName;

        public SkinData(string skinName)
        {
            _skinName = skinName;
        }

        public string SkinName => _skinName;
    }
}