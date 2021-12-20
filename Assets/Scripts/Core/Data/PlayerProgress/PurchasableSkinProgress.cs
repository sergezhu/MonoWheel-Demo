using System;
using Attributes;
using UnityEngine;

namespace Core.Data.PlayerProgress
{
    [Serializable]
    public struct PurchasableSkinProgress
    {
        [SerializeField][GUIReadOnly]
        private string _skinGroupID;
        [SerializeField]
        private bool _isPurchased;
        [SerializeField]
        private bool _isCurrent;

        public PurchasableSkinProgress(string skinGroupID, bool isPurchased, bool isCurrent)
        {
            _skinGroupID = skinGroupID;
            _isPurchased = isPurchased;
            _isCurrent = isCurrent;
        }

        public string SkinGroupID => _skinGroupID;
        public bool IsPurchased => _isPurchased;
        public bool IsCurrent => _isCurrent;
    }
}