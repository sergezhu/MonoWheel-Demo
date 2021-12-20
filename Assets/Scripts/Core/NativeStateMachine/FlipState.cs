using System;
using Core.AnimationControl;
using Core.InputControl;

namespace Core.NativeStateMachine
{
    public class FlipState : IState
    {
        private readonly AnimationController _animationController;
        private readonly InputController _inputController;

        public FlipState(AnimationController animationController, InputController inputController)
        {
            _animationController = animationController;
            _inputController = inputController;
        }

        public event Action Enter;
        public event Action Exit;

        public int GetHash()
        {
            return AnimationController.HashFlip;
        }

        public void Tick()
        {
        }

        public void OnEnter()
        {
            _animationController.FlipAnimationEventHandler.ResetReady();
            _animationController.SetMainAnimatorState(GetHash());
        }

        public void OnExit()
        {
        }
    }
}
