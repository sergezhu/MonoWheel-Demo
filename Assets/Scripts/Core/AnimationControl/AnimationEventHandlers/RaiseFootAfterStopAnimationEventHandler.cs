using System;
using UnityEngine;

namespace Core.AnimationControl.AnimationEventHandlers
{
    public class RaiseFootAfterStopAnimationEventHandler : MonoBehaviour
    {
        private Action OnMainOperationCallback;
    
        public bool IsReady { get; private set; }

        public void OnAnimationRaiseFootAfterStopDoMainOperation()
        {
            OnMainOperationCallback?.Invoke();
            OnMainOperationCallback = null;
            IsReady = true;
        }

        public void ResetReady()
        {
            IsReady = false;
        }
    }
}
