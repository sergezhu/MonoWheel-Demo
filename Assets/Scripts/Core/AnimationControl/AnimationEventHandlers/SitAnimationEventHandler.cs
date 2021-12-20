using System;
using UnityEngine;

namespace Core.AnimationControl.AnimationEventHandlers
{
    public class SitAnimationEventHandler : MonoBehaviour
    {
        public Action OnMainOperationCallback;
    
        public bool IsReady { get; private set; }

        public void OnAnimationSitDoMainOperation()
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
