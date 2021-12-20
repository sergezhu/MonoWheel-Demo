using System;
using System.Collections.Generic;
using System.Linq;
using Core.Data.LoadScenesHandlers;
using Core.Data.SceneContext;
using Core.Player;
using Core.Player.InitialData;
using Core.Player.PlayerParameters;
using Core.Skins;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Data.PlayerProgress
{
    public class PlayerProgressLoader : MonoBehaviour
    {
        public event Action<PlayerProgress> PlayerProgressLoaded;
        public event Action<PlayerProgress> RequestOnSceneViewUpdate;
        public event Action<PlayerProgress> PlayerProgressChangesPreparing;
    
        private const string SaveName = "PlayerProgress";

        [SerializeField]
        private WheelsLibrary _wheelsLibrary;
        [SerializeField]
        private SkinsLibrary _skinsLibrary;
        [SerializeField]
        private InitialWheelsData _initialWheelsData;
        [SerializeField]
        private InitialCharacterStatsData _initialCharacterStatsData;

        [Space]
        [SerializeField]
        private PlayerParameters _playerParameters;
    
        [Space]
        [SerializeField]
        private PlayerProgress _currentPlayerProgress;

        [Space]
        [SerializeField]
        private bool _clearPlayerPrefsWhenStart = false;

        public PlayerProgress CurrentProgress => _currentPlayerProgress;
        public PlayerParameters PlayerParameters => _playerParameters;

        private SceneContextLoader _sceneContextLoader;
        private SceneLoadHandler _levelMapLoadHandler;
        

        public void Init(SceneContextLoader sceneContextLoader)
        {
            _sceneContextLoader = sceneContextLoader;
            _levelMapLoadHandler = _sceneContextLoader.GetSceneLoadHandler(_sceneContextLoader.LevelMapSceneName);
        
            if(_clearPlayerPrefsWhenStart)
                ClearPlayerPrefs();
        
            _levelMapLoadHandler.SceneActivated += OnLevelMapSceneActivated;
            
            DontDestroyOnLoad(gameObject);
        }

        private void OnLevelMapSceneActivated()
        {
            LoadProgressFromPlayerPrefs();
            DoRequestOnSceneViewUpdate();
        }

        private void LoadProgressFromPlayerPrefs()
        {
            
            PlayerProgressChangesPreparing?.Invoke(_currentPlayerProgress);
        
            if(_currentPlayerProgress != null)
                _currentPlayerProgress.Changed -= OnProgressChanged;
            
            if (TryLoadFromPlayerPrefs() == false)
            {
                CreateNewProgress();
                SaveProgressToPlayerPrefs();
            }
            
            CurrentProgress.Changed += OnProgressChanged;
        
            CurrentProgress.SetupWheelWrappersProgressFromLibrary(_wheelsLibrary);
            CurrentProgress.SetupSkinWrappersProgressFromLibrary(_skinsLibrary);
        
            PlayerProgressLoaded?.Invoke(CurrentProgress);
        }

        private void DoRequestOnSceneViewUpdate()
        {
            RequestOnSceneViewUpdate?.Invoke(_currentPlayerProgress);
        }

        private void CreateNewProgress()
        {
            _currentPlayerProgress = new PlayerProgress(_initialWheelsData, _initialCharacterStatsData, _skinsLibrary.PurchasableSkins.ToList(),  750);
        }

        private void SaveProgressToPlayerPrefs()
        {
            if (CurrentProgress is null)
                CreateNewProgress();
        
            var playerProgressJson = JsonUtility.ToJson(CurrentProgress);
            PlayerPrefs.SetString(SaveName, playerProgressJson);
        }

        [ContextMenu("Clear PlayerPrefs Progress")]
        public void ClearPlayerPrefs()
        {
            PlayerProgressChangesPreparing?.Invoke(_currentPlayerProgress);
        
            _currentPlayerProgress.Changed -= SaveProgressToPlayerPrefs;

            if (PlayerPrefs.HasKey(SaveName))
            {
                PlayerPrefs.DeleteKey(SaveName);
            }
        }

        private void OnProgressChanged()
        {
            SaveProgressToPlayerPrefs();
        }

        private bool TryLoadFromPlayerPrefs()
        {
            PlayerProgress progress = null;

            if (PlayerPrefs.HasKey(SaveName))
            {
                var playerProgressJson = PlayerPrefs.GetString(SaveName);
                progress = JsonUtility.FromJson<PlayerProgress>(playerProgressJson);
            }

            _currentPlayerProgress = progress;
            return _currentPlayerProgress is null == false;
        }
    }
}
