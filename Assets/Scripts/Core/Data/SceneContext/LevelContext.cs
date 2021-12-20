using System;
using UnityEngine;

namespace Core.Data.SceneContext
{
    [Serializable]
    public class LevelContext
    {
        [SerializeField]
        private LevelSettings _targetLevelSettings;
        public LevelSettings TargetLevelSettings { get; set; }
    }
}
