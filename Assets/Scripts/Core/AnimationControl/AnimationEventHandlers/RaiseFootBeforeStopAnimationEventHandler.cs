using System;
using UnityEngine;

namespace Core.AnimationControl.AnimationEventHandlers
{
    public class RaiseFootBeforeStopAnimationEventHandler : MonoBehaviour
    {
        private Action OnMainOperationCallback;
    
        public bool IsReady { get; private set; }

        public void OnAnimationRaiseFootBeforeStopDoMainOperation()
        {
            UnityEngine.Debug.Log("OnAnimationRaiseFootBeforeStop");
            OnMainOperationCallback?.Invoke();
            OnMainOperationCallback = null;
            IsReady = true;
        }

        public void Init(Action onMainOperationCallback)
        {
            ResetReady();
            OnMainOperationCallback = onMainOperationCallback;
        }
        public void ResetReady()
        {
            IsReady = false;
        }
    }
}
