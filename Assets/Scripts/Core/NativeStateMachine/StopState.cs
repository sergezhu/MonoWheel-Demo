using System;
using Core.AnimationControl;
using Core.InputControl;
using Core.Player;

namespace Core.NativeStateMachine
{
    public class StopState : IState
    {
        private readonly AnimationController _animationController;
        private readonly InputController _inputController;
        private readonly BeforeStopDelayTimer _beforeStopDelayTimer;
        private readonly MotorMono _motorMono;

        public StopState(AnimationController animationController, InputController inputController, BeforeStopDelayTimer beforeStopDelayTimer, MotorMono motorMono)
        {
            _animationController = animationController;
            _inputController = inputController;
            _beforeStopDelayTimer = beforeStopDelayTimer;
            _motorMono = motorMono;
        }

        public event Action Enter;
        public event Action Exit;

        public int GetHash()
        {
            return AnimationController.HashStop;
        }

        public void Tick()
        {
            DoUpdate();
        }

        public void OnEnter()
        {
            _animationController.SetMainAnimatorState(GetHash());
            
            var footStepHeight = _motorMono.GetFootStepHeightRelative();
            _animationController.MainAnimator.SetFloat(AnimationController.HashRelativeWheelSize, footStepHeight);
            _animationController.FeetAnimator.SetFloat(AnimationController.HashRelativeWheelSize, footStepHeight);
        }

        public void OnExit()
        {
        }
        
        private void DoUpdate()
        {
            var footStepHeight = _motorMono.GetFootStepHeightRelative();
            _animationController.MainAnimator.SetFloat(AnimationController.HashRelativeWheelSize, footStepHeight);
            _animationController.FeetAnimator.SetFloat(AnimationController.HashRelativeWheelSize, footStepHeight);
        }
    }
}
