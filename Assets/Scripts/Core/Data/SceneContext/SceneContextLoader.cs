using System;
using System.Collections;
using System.Collections.Generic;
using Attributes;
using Core.Data.LevelPreloader;
using Core.Data.LoadScenesHandlers;
using UI.Popups;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR

#endif

namespace Core.Data.SceneContext
{
    public class SceneContextLoader : MonoBehaviour
    {
        private const string SaveName = "LevelContext";

#if UNITY_EDITOR
        [SerializeField]
        private SceneAsset _preloaderScene;
        [SerializeField]
        private SceneAsset _levelMapScene;
        [SerializeField]
        private SceneAsset _workshopScene;
        [SerializeField]
        private SceneAsset _shopScene;
        [SerializeField]
        private SceneAsset _levelUIScene;
        [SerializeField]
        private SceneAsset _popupsScene;
#endif

        [Space]
        [SerializeField][GUIReadOnly]
        private string _preloaderSceneName;
        [SerializeField][GUIReadOnly]
        private string _levelMapSceneName;
        [SerializeField][GUIReadOnly]
        private string _workshopSceneName;
        [SerializeField][GUIReadOnly]
        private string _shopSceneName;
        [SerializeField][GUIReadOnly]
        private string _levelUISceneName;
        [SerializeField][GUIReadOnly]
        private string _popupsSceneName;
    
        [Space]
        [SerializeField]
        private LevelContext _currentLevelContext;

        public LevelContext CurrentLevelContext => _currentLevelContext;

        //public string MainLoaderSceneName => _mainLoaderSceneName;
        public string PreloaderSceneName => _preloaderSceneName;
        public string LevelMapSceneName => _levelMapSceneName;
        public string WorkshopSceneName => _workshopSceneName;
        public string ShopSceneName => _shopSceneName;
        public string LevelUISceneName => _levelUISceneName;
        public string PopupsSceneName => _popupsSceneName;


        private Dictionary<string, SceneLoadHandler> _sceneLoadHandlers;
        private Dictionary<Type, PopupContext> _popupsContext;

        public void Init()
        {
            SetupScenesNames();
            _sceneLoadHandlers ??= CreateLoadHandlers();
        
            UnityEngine.Debug.Log($"LevelContextLoader Awake");
            DontDestroyOnLoad(gameObject);
        
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private Dictionary<string, SceneLoadHandler> CreateLoadHandlers()
        {
            var sceneLoadHandlers = new Dictionary<string, SceneLoadHandler>
            {
                {_levelMapSceneName, new LevelMapLoadHandler()},
                {_preloaderSceneName, new PreloaderLoadHandler()},
                {_workshopSceneName, new WorkshopLoadHandler()},
                {_shopSceneName, new ShopLoadHandler()}
            };

            return sceneLoadHandlers;
        }

        private void SetupScenesNames()
        {
#if UNITY_EDITOR
            _preloaderSceneName = _preloaderScene == null ? "" : _preloaderScene.name;
            _levelMapSceneName = _levelMapScene == null ? "" : _levelMapScene.name;
            _workshopSceneName = _workshopScene == null ? "" : _workshopScene.name;
            _shopSceneName = _shopScene == null ? "" : _shopScene.name;
            _levelUISceneName = _levelUIScene == null ? "" : _levelUIScene.name;
            _popupsSceneName = _popupsScene == null ? "" : _popupsScene.name;
        
            //_mainLoaderSceneName = _mainLoaderScene == null ? "" : _mainLoaderScene.name;
        
            EditorUtility.SetDirty(this);
#endif
        }

        private void OnActiveSceneChanged(Scene previous, Scene current)
        {
            UnityEngine.Debug.Log($"Previous scene : [{previous.name}]  Current scene : [{current.name}]");

            if (_sceneLoadHandlers.ContainsKey(current.name))
            {
                var sceneLoadHandler = _sceneLoadHandlers[current.name];
                sceneLoadHandler.ActivateScene(this);
            }
        }

        private void OnValidate()
        {
            SetupScenesNames();
        }

        private void CreatePopupsContext()
        {
            _popupsContext = new Dictionary<Type, PopupContext>
            {
                {typeof(WheelBuyingConfirmPopup), null}, 
                {typeof(OfferBuyingConfirmPopup), null}, 
                {typeof(RealMoneyShopPopup), null}
            };
        }

        public void PlayerPrefsLoad()
        {
            if (TryLoadFromPlayerPrefs() == false)
                throw new InvalidOperationException();
        }

        public void SavePlayerPrefs()
        {
            if (CurrentLevelContext is null)
                throw new InvalidOperationException();
        
            var json = JsonUtility.ToJson(CurrentLevelContext);;
            PlayerPrefs.SetString(SaveName, json);
        }
    
        public void SavePlayerPrefs(LevelContext context)
        {
            if (context is null)
                throw new InvalidOperationException();
        
            _currentLevelContext = context;
            SavePlayerPrefs();
        }

        public void PlayerPrefsClear()
        {
            _currentLevelContext = null;
        
            if (PlayerPrefs.HasKey(SaveName))
            {
                PlayerPrefs.DeleteKey(SaveName);
            }
        }

        public void LoadPreloaderScene()
        {
            LoadSceneNormal(PreloaderSceneName);
        }
        public void LoadLevelMapScene()
        {
            LoadSceneNormal(LevelMapSceneName);
        }
    
        public void LoadWorkshopScene()
        {
            LoadSceneNormal(WorkshopSceneName);
        }

        public void LoadShopScene()
        {
            LoadSceneNormal(ShopSceneName);
        }

        public void LoadUIScene(Action<AsyncOperation> onCompleteCallback = null)
        {
            LoadSceneAdditive(LevelUISceneName, onCompleteCallback);
        }
        public void LoadPopupsScene(Action<AsyncOperation> onCompleteCallback = null)
        {
            LoadSceneAdditive(PopupsSceneName, onCompleteCallback);
        }

        public SceneLoadHandler GetSceneLoadHandler(string sceneName)
        {
            if (_sceneLoadHandlers.ContainsKey(sceneName) == false)
                throw new InvalidOperationException();

            return _sceneLoadHandlers[sceneName];
        }
    
        public void AddLevelLoadHandlerIfNotContains(string levelName)
        {
            if (_sceneLoadHandlers.ContainsKey(levelName))
                return;

            _sceneLoadHandlers.Add(levelName, new LevelLoadHandler());
        }

        private bool TryLoadFromPlayerPrefs()
        {
            string json;
            LevelContext pp = null;

            if (PlayerPrefs.HasKey(SaveName))
            {
                json = PlayerPrefs.GetString(SaveName);
                pp = JsonUtility.FromJson<LevelContext>(json);
            }

            _currentLevelContext = pp;
        
            return CurrentLevelContext is null == false;
        }
    
        public void LoadSceneNormal(string sceneName)
        {
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }

        public void LoadSceneAdditive(string sceneName, Action<AsyncOperation> onCompleteCallback = null)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            if (onCompleteCallback != null)
                operation.completed += onCompleteCallback;
        }
    
        private IEnumerator LoadSceneWithPreload(string sceneName, IPreloaderProgressBar preloader)
        {
            yield return null;

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = false;
            UnityEngine.Debug.Log($"Progress : {asyncOperation.progress}  Done : {asyncOperation.isDone}");
            preloader.OnProgressChanged(asyncOperation.progress / 0.9f);

            while (!asyncOperation.isDone)
            {
                //m_Text.text = "Loading progress: " + (asyncOperation.progress * 100) + "%";
                preloader.OnProgressChanged(asyncOperation.progress / 0.9f);

                if (asyncOperation.progress >= 0.9f)
                {
                    preloader.OnProgressChanged(1f);
                    asyncOperation.allowSceneActivation = true;
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        public void BeginLoadingSceneWithPreload(IPreloaderProgressBar preloader)
        {
            StartCoroutine(LoadSceneWithPreload(CurrentLevelContext.TargetLevelSettings.TargetSceneName, preloader));
        }
    }
}
