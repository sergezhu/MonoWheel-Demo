using Core.Data;
using Core.Data.PlayerProgress;
using Core.Data.SceneContext;
using Core.Player;
using Core.Player.InitialData;
using Core.Skins;
using UnityEngine;

namespace Core
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField]
        private PlayerProgressLoader _playerProgressLoader;
        [SerializeField]
        private SceneContextLoader _sceneContextLoader;
        [SerializeField]
        private WheelsLibrary _wheelsLibrary;
        [SerializeField]
        private SkinsLibrary _skinsLibrary;
        [SerializeField]
        private InitialCharacterStatsData _initialCharacterStatsData;
        [SerializeField]
        private InitialWheelsData _initialWheelsData;


        private void Awake()
        {
            Run();
        }

        private void Run()
        {
            _wheelsLibrary.SetDontDestroyOnLoad();
            _initialCharacterStatsData.SetDontDestroyOnLoad();
            _initialWheelsData.SetDontDestroyOnLoad();

            _sceneContextLoader.Init();
            _playerProgressLoader.Init(_sceneContextLoader);

            DontDestroyReferencesHolder.PlayerProgressLoader = _playerProgressLoader;
            DontDestroyReferencesHolder.SceneContextLoader = _sceneContextLoader;
        
            _sceneContextLoader.LoadLevelMapScene();
        }
    }
}
