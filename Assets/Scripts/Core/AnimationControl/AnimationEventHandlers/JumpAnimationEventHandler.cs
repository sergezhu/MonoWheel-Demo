using System;
using UnityEngine;

namespace Core.AnimationControl.AnimationEventHandlers
{
    public class JumpAnimationEventHandler : MonoBehaviour
    {
        public Action OnMainOperationCallback;
    
        public bool IsReady { get; private set; }

        public void OnAnimationJumpDoMainOperation()
        {
            IsReady = true;
            OnMainOperationCallback?.Invoke();
            OnMainOperationCallback = null;
        }

        public void ResetReady()
        {
            IsReady = false;
        }
    }
}
