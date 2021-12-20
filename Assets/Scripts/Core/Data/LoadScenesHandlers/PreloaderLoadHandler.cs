using System.Linq;
using Core.Data.LevelPreloader;
using Core.Data.SceneContext;
using UnityEngine;

namespace Core.Data.LoadScenesHandlers
{
    public class PreloaderLoadHandler : SceneLoadHandler
    {
        protected override void OnActivateScene(SceneContextLoader sceneContextLoader)
        {
            var preloaders = Object.FindObjectsOfType<MonoBehaviour>().OfType<IPreloaderProgressBar>().ToArray();
            var preloader = preloaders[0];
            sceneContextLoader.BeginLoadingSceneWithPreload(preloader);
        }
    }
}
