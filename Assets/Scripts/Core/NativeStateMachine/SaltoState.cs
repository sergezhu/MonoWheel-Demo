using System;
using Core.AnimationControl;
using Core.InputControl;

namespace Core.NativeStateMachine
{
    public class SaltoState : IState
    {
        private readonly AnimationController _animationController;
        private readonly InputController _inputController;
        private readonly Movement _movement;
        private readonly SaltoTracker _saltoTracker;
        private readonly JumpPower _jumpPower;


        public SaltoState(AnimationController animationController, InputController inputController, Movement movement, JumpPower jumpPower, SaltoTracker saltoTracker)
        {
            _animationController = animationController;
            _inputController = inputController;
            _movement = movement;
            _jumpPower = jumpPower;
            _saltoTracker = saltoTracker;
        }

        public event Action Enter;
        public event Action Exit;

        public int GetHash()
        {
            //  !!! temporary hash
            return AnimationController.HashSalto;
        }

        public void Tick()
        {
            _movement.DoUpdate();
            _jumpPower.DoUpdate();
        }

        public void OnEnter()
        {
            _animationController.SetMainAnimatorState(GetHash());
            _saltoTracker.RelativeOffsetChanged += OnRelativeOffsetChanged;
            _saltoTracker.DoSalto();
            
            Enter?.Invoke();
        }

        public void OnExit()
        {
            _saltoTracker.RelativeOffsetChanged -= OnRelativeOffsetChanged;
            _saltoTracker.ResetActive();
            
            Exit?.Invoke();
        }

        private void OnRelativeOffsetChanged(float value)
        {
            _animationController.MainAnimator.SetFloat(AnimationController.HashSaltoPreparingPower, value);
        }
    }
}
