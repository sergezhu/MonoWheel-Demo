using System;
using Core.AnimationControl;
using Core.InputControl;
using Core.Player;

namespace Core.NativeStateMachine
{
    public class MoveToJumpPreparingState : IState
    {
        private readonly AnimationController _animationController;
        private readonly AccelerationProvider _accelerationProvider;
        private readonly MotorMono _motorMono;
        private readonly JumpPower _jumpPower;
        private readonly Movement _movement;

        public MoveToJumpPreparingState(AnimationController animationController, AccelerationProvider accelerationProvider, MotorMono motorMono, JumpPower jumpPower, Movement movement)
        {
            _animationController = animationController;
            _accelerationProvider = accelerationProvider;
            _motorMono = motorMono;
            _jumpPower = jumpPower;
            _movement = movement;
        }

        public event Action Enter;
        public event Action Exit;

        public int GetHash()
        {
            return AnimationController.HashJumpPreparing;
        }

        public void Tick()
        {
            DoMoveUpdate();
            DoJumpingUpdate();
        }

        public void OnEnter()
        {
            _animationController.SetMainAnimatorState(GetHash());
            
            if(_jumpPower.IsActive == false)
                _jumpPower.Init();
            
            _jumpPower.SetIncrease();
        }

        public void OnExit()
        {
        }

        private void DoMoveUpdate()
        {
            _animationController.MainAnimator.SetFloat(AnimationController.HashRelativeSpeed, _motorMono.RelativeCharacterTilt);
            _animationController.MainAnimator.SetFloat(AnimationController.HashVerticalRelativeAcceleration,_accelerationProvider.Acceleration);
            
            _animationController.FeetAnimator.SetFloat(AnimationController.HashRelativeTilt, _motorMono.RelativeFeetTilt);
        
            _movement.DoUpdate();
        }

        private void DoJumpingUpdate()
        {
            _jumpPower.DoUpdate();
            _animationController.MainAnimator.SetFloat(AnimationController.HashJumpPreparingPower, _jumpPower.RelativePower);
        }
    }
}
