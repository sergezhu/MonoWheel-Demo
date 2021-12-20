using System;
using System.Collections.Generic;
using Attributes;
using Core.Level.Challenges;
using UnityEngine;

namespace Core.Data.PlayerProgress
{
    [Serializable]
    public struct LevelProgress
    {
        [SerializeField][GUIReadOnly]
        private string _sceneName;
        
        [Space]
        [SerializeField]
        private bool _bronzePassed;
        [SerializeField]
        private bool _silverPassed;
        [SerializeField]
        private bool _goldPassed;

        [Space]
        [SerializeField]
        private List<ChallengeProgress> _challengesProgress;


        public bool LevelPassed => _bronzePassed;
        public bool BronzePassed => _bronzePassed;
        public bool SilverPassed => _silverPassed;
        public bool GoldPassed => _goldPassed;
        public string SceneName => _sceneName;
        public IEnumerable<ChallengeProgress> ChallengesProgress => _challengesProgress;

        public LevelProgress(string sceneName, List<BaseChallenge> challenges)
        {
            _sceneName = sceneName;
            _bronzePassed = false;
            _silverPassed = false;
            _goldPassed = false;
            _challengesProgress = new List<ChallengeProgress>();

            SetupChallenges(challenges);
        }

        public LevelProgress(LevelProgress refLevelProgress)
        {
            _sceneName = refLevelProgress.SceneName;
            _bronzePassed = refLevelProgress.BronzePassed;
            _silverPassed = refLevelProgress.SilverPassed;
            _goldPassed = refLevelProgress.GoldPassed;
        
            _challengesProgress = new List<ChallengeProgress>();
            _challengesProgress.AddRange(refLevelProgress.ChallengesProgress);
        }
        
        public LevelProgress(string sceneName, LevelProgress refLevelProgress)
        {
            _sceneName = sceneName;
            _bronzePassed = refLevelProgress.BronzePassed;
            _silverPassed = refLevelProgress.SilverPassed;
            _goldPassed = refLevelProgress.GoldPassed;
        
            _challengesProgress = new List<ChallengeProgress>();
            _challengesProgress.AddRange(refLevelProgress.ChallengesProgress);
        }

        public void Validate()
        {
            ValidatePassedType();
        }

        private void ValidatePassedType()
        {
            if (_goldPassed)
            {
                SetGoldPassed();
                return;
            }

            if (_silverPassed)
            {
                SetSilverPassed();
                return;
            }
        }

        public void SetBronzePassed()
        {
            _bronzePassed = true;
            _silverPassed = false;
            _goldPassed = false;
        }

        public void SetSilverPassed()
        {
            _bronzePassed = true;
            _silverPassed = true;
            _goldPassed = false;
        }

        public void SetGoldPassed()
        {
            _bronzePassed = true;
            _silverPassed = true;
            _goldPassed = true;
        }

        public void Reset()
        {
            _bronzePassed = false;
            _silverPassed = false;
            _goldPassed = false;
        }

        public bool IsAllChallengesPassed()
        {
            var passed = _challengesProgress.Count != 0;

            foreach (var challengeProgress in _challengesProgress)
            {
                if (challengeProgress.IsPassed == false)
                {
                    passed = false;
                    break;
                }
            }

            return passed;
        }

        public int GetAchievedCharacterStatsPoints()
        {
            var points = 0;

            if (LevelPassed)
                points++;

            if (IsAllChallengesPassed())
                points++;

            return points;
        }

        private void SetupChallenges(List<BaseChallenge> challenges)
        {
            if (challenges == null)
                throw new NullReferenceException();
        
            _challengesProgress = new List<ChallengeProgress>();

            foreach (var challenge in challenges)
            {
                _challengesProgress.Add(new ChallengeProgress(challenge.UniqueName, challenge.IsPassed));
            }
        }
    }
}