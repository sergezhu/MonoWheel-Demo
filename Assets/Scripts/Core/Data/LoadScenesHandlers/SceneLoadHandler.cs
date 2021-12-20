using System;
using Core.Data.SceneContext;

namespace Core.Data.LoadScenesHandlers
{
    public abstract class SceneLoadHandler
    {
        public event Action SceneActivated;

        protected abstract void OnActivateScene(SceneContextLoader sceneContextLoader);
    
        public void ActivateScene(SceneContextLoader sceneContextLoader)
        {
            OnActivateScene(sceneContextLoader);
            SceneActivated?.Invoke();
        }
    }
}
