using System;
using Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Data.PlayerProgress
{
    [Serializable]
    public struct WheelProgress
    {
        [SerializeField][GUIReadOnly]
        private string _wheelID;
        [SerializeField][GUIReadOnly]
        private bool _isCurrent;
        [SerializeField][GUIReadOnly]
        private bool _isPurchased;

        public WheelProgress(string wheelID, bool isCurrent, bool isPurchased)
        {
            _wheelID = wheelID;
            _isCurrent = isCurrent;
            _isPurchased = isPurchased;
        }

        public string WheelID => _wheelID;
        public bool IsCurrent => _isCurrent;
        public bool IsPurchased => _isPurchased;
    }
}