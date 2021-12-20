using System;
using Core.AnimationControl;
using Core.InputControl;
using Core.Player;

namespace Core.NativeStateMachine
{
    public class MoveState : IState
    {
        private readonly AnimationController _animationController;
        private readonly AccelerationProvider _accelerationProvider;
        private readonly MotorMono _motorMono;
        private readonly Movement _movement;
        private readonly JumpPower _jumpPower;

        public MoveState(AnimationController animationController, AccelerationProvider accelerationProvider, MotorMono motorMono, Movement movement, JumpPower jumpPower)
        {
            _animationController = animationController;
            _accelerationProvider = accelerationProvider;
            _motorMono = motorMono;
            _movement = movement;
            _jumpPower = jumpPower;
        }

        public event Action Enter;
        public event Action Exit;

        public int GetHash()
        {
            return AnimationController.HashMove;
        }

        public void Tick()
        {
            DoUpdate();
        }

        public void OnEnter()
        {
            _animationController.SetMainAnimatorState(GetHash());
            _animationController.MainAnimator.SetFloat(AnimationController.HashRelativeWheelSize, _motorMono.GetFootStepHeightRelative());
        }

        public void OnExit()
        {
        }

        private void DoUpdate()
        {
            _animationController.MainAnimator.SetFloat(AnimationController.HashRelativeSpeed, _motorMono.RelativeCharacterTilt);
            _animationController.MainAnimator.SetFloat(AnimationController.HashVerticalRelativeAcceleration,_accelerationProvider.Acceleration);
            
            _animationController.FeetAnimator.SetFloat(AnimationController.HashRelativeTilt, _motorMono.RelativeFeetTilt);

            _movement.DoUpdate();
            _jumpPower.DoUpdate();
        }

        private void OnPreparingEnded()
        {
            _animationController.SetMainAnimatorState(GetHash());
            _jumpPower.SetDecrease();
            
            _jumpPower.PreparingEnded -= OnPreparingEnded;
        }
    }
}
