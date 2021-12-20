using System;
using Core.AnimationControl;
using Core.InputControl;
using Core.Player;

namespace Core.NativeStateMachine
{
    public class SitState : IState
    {
        private readonly AnimationController _animationController;
        private readonly MotorMono _motorMono;
        private readonly Movement _movement;
        private readonly JumpPower _jumpPower;


        public SitState(AnimationController animationController, MotorMono motorMono, Movement movement, JumpPower jumpPower)
        {
            _animationController = animationController;
            _motorMono = motorMono;
            _movement = movement;
            _jumpPower = jumpPower;
        }

        public event Action Enter;
        public event Action Exit;

        public int GetHash()
        {
            return AnimationController.HashSit;
        }

        public void Tick()
        {
            DoUpdate();
        }

        public void OnEnter()
        {
            _animationController.SetMainAnimatorState(GetHash());
        }

        public void OnExit()
        {
        }

        private void DoUpdate()
        {
            _movement.DoUpdate();
            _jumpPower.DoUpdate();
            
            _animationController.MainAnimator.SetFloat(AnimationController.HashRelativeSpeed, _motorMono.RelativeCharacterTilt);
            _animationController.FeetAnimator.SetFloat(AnimationController.HashRelativeTilt, _motorMono.RelativeFeetTilt);
        }
    }
}
