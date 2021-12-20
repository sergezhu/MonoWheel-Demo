using System;
using Core.AnimationControl;
using Core.InputControl;
using Core.Player;

namespace Core.NativeStateMachine
{
    public class JumpingState : IState
    {
        private readonly AnimationController _animationController;
        private readonly AccelerationProvider _accelerationProvider;
        private readonly MotorMono _motorMono;
        private readonly Movement _movement;
        private readonly JumpPower _jumpPower;
        private readonly SaltoTracker _saltoTracker;
    
        public bool IsJumpingEnded { get; private set; }
        public bool IsActive { get; private set; }
        
        public bool SaltoLocked { get; private set; }

        public JumpingState(AnimationController animationController, AccelerationProvider accelerationProvider, MotorMono motorMono, Movement movement, JumpPower jumpPower, SaltoTracker saltoTracker)
        {
            _animationController = animationController;
            _accelerationProvider = accelerationProvider;
            _motorMono = motorMono;
            _movement = movement;
            _jumpPower = jumpPower;
            _saltoTracker = saltoTracker;
        }

        public event Action Enter;
        public event Action Exit;

        public int GetHash()
        {
            return AnimationController.HashMove;
        }

        public void Tick()
        {
            _movement.DoUpdate();
            _jumpPower.DoUpdate();
            _animationController.MainAnimator.SetFloat(AnimationController.HashJumpPreparingPower, _jumpPower.RelativePower);
            _animationController.MainAnimator.SetFloat(AnimationController.HashRelativeSpeed, _motorMono.RelativeCharacterTilt);
            _animationController.MainAnimator.SetFloat(AnimationController.HashVerticalRelativeAcceleration,_accelerationProvider.Acceleration);
            
            _animationController.FeetAnimator.SetFloat(AnimationController.HashRelativeTilt, _motorMono.RelativeFeetTilt);
        }

        public void OnEnter()
        {
            _animationController.SetMainAnimatorState(GetHash());

            if (_saltoTracker.IsSuccessfully == false)  //вошли в стейт не из сальто
            {
                _jumpPower.SetDecrease();
                _motorMono.PowerJump(_jumpPower.RelativePower);
            }
            else                                        //вошли в стейт из сальто
            {
                SaltoLocked = _saltoTracker.AllowTurnIfButtonPressedAgain == false;
                _saltoTracker.ResetParent();
                _motorMono.IsStabilizeActive = true;
            }

            IsJumpingEnded = false;
            IsActive = true;

            _motorMono.JumpEnd += OnJumpEnd;
            
            Enter?.Invoke();
        }

        public void OnExit()
        {
            IsJumpingEnded = false;
            IsActive = false;

            _saltoTracker.SuccessfullyReset();
            _motorMono.JumpEnd -= OnJumpEnd;

            Exit?.Invoke();
        }

        private void OnJumpEnd()
        {
            IsJumpingEnded = true;
            SaltoLocked = false;
        }
    }
}
