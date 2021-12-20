using System;
using Core.AnimationControl;
using Core.InputControl;
using Core.Player;

namespace Core.NativeStateMachine
{
    public class RaisingFootBeforeStopState : IState
    {
        private readonly AnimationController _animationController;
        private readonly InputController _inputController;
        private readonly MotorMono _motorMono;

        public RaisingFootBeforeStopState(AnimationController animationController, InputController inputController, MotorMono motorMono)
        {
            _animationController = animationController;
            _inputController = inputController;
            _motorMono = motorMono;
        }

        public event Action Enter;
        public event Action Exit;

        public int GetHash()
        {
            return AnimationController.HashRaisingFootBeforeStop;
        }

        public void Tick()
        {
            DoUpdate();
        }

        public void OnEnter()
        {
            _animationController.RaiseFootBeforeStopAnimationEventHandler.ResetReady();
            _animationController.SetMainAnimatorState(GetHash());

            var footStepHeight = _motorMono.GetFootStepHeightRelative();
            _animationController.MainAnimator.SetFloat(AnimationController.HashRelativeWheelSize, footStepHeight);
            _animationController.FeetAnimator.SetFloat(AnimationController.HashRelativeWheelSize, footStepHeight);
        }

        public void OnExit()
        {
            _animationController.RaiseFootBeforeStopAnimationEventHandler.ResetReady();
        }

        private void DoUpdate()
        {
        }
    }
}
