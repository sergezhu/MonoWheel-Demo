using System.Collections.Generic;
using System.Linq;
using Core.MonoBehaviourStateMachine.CharacterStates;
using Core.Player;
using DG.Tweening;
using UnityEngine;

namespace Core.AnimationControl.AnimationEventHandlers
{
    public class SaltoAnimationEventHandler : MonoBehaviour
    {
        [SerializeField] 
        private Transform monoRoot;
        [SerializeField] 
        private MotorMono motorMono;
        [SerializeField] 
        private GameObject character;
    
        private CharacterNormalState _charNormalState;
        private CharacterSitState _charSitState;
    
        [SerializeField] [Range(0.1f, 3f)]
        private float duration = 0.5f;

        private List<Rigidbody2D> _childrenRigidbodies;

        private void Awake()
        {
            _childrenRigidbodies = monoRoot.GetComponentsInChildren<Rigidbody2D>().ToList();
        
            _charNormalState = character.GetComponent<CharacterNormalState>();
            _charSitState = character.GetComponent<CharacterSitState>();
        }


        [ContextMenu("Salto Forward")]
        public void SaltoForward()
        {
            Salto(true);
        }
        [ContextMenu("Salto Backward")]
        public void SaltoBackward()
        {
            Salto(false);
        }
    
        private void Salto(bool forwardRotate)
        {
            if (motorMono.MonoWheel.WheelGroundChecker.IsGrounded)
                return;
        
            var isCWDirection = forwardRotate && motorMono.FaceDirection == 1 || !forwardRotate && motorMono.FaceDirection == -1;
            var initRotation = monoRoot.rotation;
            var angle = isCWDirection ? 359 : -359;
            var v = new Vector3(0, 0, angle);
            var targetRotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
        
            EnableKinematic();
        
            _charNormalState.enabled = false;
            _charSitState.enabled = true;
            motorMono.enabled = false;

            monoRoot.DORotate(v, duration, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutCubic)
                .OnComplete(() =>
                {
                    motorMono.enabled = true;
                    DisableKinematic();
                });
        
        }

        private void EnableKinematic()
        {
            _childrenRigidbodies.ForEach(rb =>
            {
                rb.isKinematic = true;
            });
        }
    
        private void DisableKinematic()
        {
            _childrenRigidbodies.ForEach(rb =>
            {
                rb.isKinematic = false;
            });
        }
    }
}
