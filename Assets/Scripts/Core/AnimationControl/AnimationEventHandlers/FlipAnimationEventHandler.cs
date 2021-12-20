using System;
using Core.Player;
using UnityEngine;

namespace Core.AnimationControl.AnimationEventHandlers
{
    public class FlipAnimationEventHandler : MonoBehaviour, IChangePlayerDataHandler
    {
        private MotorMono _motorMono;
        private Transform _riderContainerTransform;
        private Transform _stepPointTransform;
    
        public Action OnMainOperationCallback;

        public bool IsReady { get; private set; }

    
        public void OnAnimationFlipDoMainOperation()
        {
            Flip();
            OnMainOperationCallback?.Invoke();
            OnMainOperationCallback = null;
            IsReady = true;
        }
    

        public void Constructor(MotorMono motorMono)
        {
            if (motorMono == null)
                throw new NullReferenceException();
        
            _motorMono = motorMono;
        }

        private void Init()
        {
            _riderContainerTransform = _motorMono.RiderSpawner.transform;
            _stepPointTransform = _motorMono.MonoWheel.StepPoint;
        
            ResetReady();
        }
    
        public void ResetReady()
        {
            IsReady = false;
        }

        private void Flip()
        {
            _motorMono.ToggleFlipDirection();

            var charContainerLocalScale = _riderContainerTransform.localScale;
            _riderContainerTransform.localScale = new Vector3(charContainerLocalScale.x * -1, charContainerLocalScale.y, charContainerLocalScale.z);

            var stepLocalScale = _stepPointTransform.localScale;
            _stepPointTransform.localScale = new Vector3(stepLocalScale.x * -1, stepLocalScale.y, stepLocalScale.z);
        }

    

        public void OnChangeWheelHandle(MonoWheel wheel)
        {
        }

        public void OnChangeCharacterHandle(Character character)
        {
        }

        public void OnChangeReady()
        {
            Init();
        }

        public void OnChangePreparing()
        {
        }
    }
}
