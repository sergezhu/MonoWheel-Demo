using System;
using Core.AnimationControl;
using Core.InputControl;

namespace Core.NativeStateMachine
{
    public class RaisingFootAfterStopState : IState
    {
        private readonly AnimationController _animationController;
        private readonly InputController _inputController;

        public RaisingFootAfterStopState(AnimationController animationController, InputController inputController)
        {
            _animationController = animationController;
            _inputController = inputController;
        }

        public event Action Enter;
        public event Action Exit;

        public int GetHash()
        {
            return AnimationController.HashRaisingFootAfterStop;
        }

        public void Tick()
        {
        }

        public void OnEnter()
        {
            _animationController.RaiseFootAfterStopAnimationEventHandler.ResetReady();
            _animationController.SetMainAnimatorState(GetHash());
        }

        public void OnExit()
        {
            _animationController.RaiseFootAfterStopAnimationEventHandler.ResetReady();
        }
    }
}
