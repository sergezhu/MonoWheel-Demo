using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Data.PlayerProgress
{
    [Serializable]
    public struct SpriteResolverSkinData
    {
        [SerializeField]
        private string _presenterID;
        [SerializeField]
        private List<SkinData> _skinsData;
    
        public SpriteResolverSkinData(string presenterID, List<SkinData> skinsData)
        {
            _presenterID = presenterID;
            _skinsData = skinsData;
        }

        public string PresenterID => _presenterID;
        public IReadOnlyList<SkinData> SkinsProgress => _skinsData;
    }
}