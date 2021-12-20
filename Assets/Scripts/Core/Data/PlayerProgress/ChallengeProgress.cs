using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Data.PlayerProgress
{
    [Serializable]
    public struct ChallengeProgress
    {
        [SerializeField]
        private string _challengeID;
        [SerializeField]
        private bool _isPassed;

        public ChallengeProgress(string id, bool isPassed)
        {
            if (string.IsNullOrEmpty(id))
                throw new InvalidOperationException();

            _challengeID = id;
            _isPassed = isPassed;
        }

        public string ChallengeID => _challengeID;
        public bool IsPassed => _isPassed;
    }
}