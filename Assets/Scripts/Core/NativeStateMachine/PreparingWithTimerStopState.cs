using System;
using Core.AnimationControl;
using Core.InputControl;

namespace Core.NativeStateMachine
{
    public class PreparingWithTimerStopState : IState
    {
        private readonly AnimationController _animationController;
        private readonly AccelerationProvider _accelerationProvider;
        private readonly BeforeStopDelayTimer _beforeStopDelayTimer;
        private readonly JumpPower _jumpPower;

        public PreparingWithTimerStopState(AnimationController animationController, AccelerationProvider accelerationProvider, BeforeStopDelayTimer beforeStopDelayTimer, JumpPower jumpPower)
        {
            _animationController = animationController;
            _accelerationProvider = accelerationProvider;
            _beforeStopDelayTimer = beforeStopDelayTimer;
            _jumpPower = jumpPower;
        }

        public event Action Enter;
        public event Action Exit;

        public int GetHash()
        {
            return -1;
        }

        public void Tick()
        {
            _beforeStopDelayTimer.UpdateTimer();
            _jumpPower.DoUpdate();
            
            _animationController.MainAnimator.SetFloat(AnimationController.HashVerticalRelativeAcceleration,_accelerationProvider.Acceleration);
        }

        public void OnEnter()
        {
            _beforeStopDelayTimer.EnableStopTimer();
        }

        public void OnExit()
        {
            _beforeStopDelayTimer.DisableStopTimer();
        }
    }
}
