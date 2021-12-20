using System;
using System.Linq;
using Core.AnimationControl;
using Core.InputControl;
using Core.Player;

namespace Core.NativeStateMachine
{
    public class CrashState : IState
    {
        private readonly AnimationController _animationController;
        private readonly MotorMono _motorMono;
        private readonly Movement _movement;
        private readonly JumpPower _jumpPower;
        private readonly Ragdoll _ragdoll;
        private readonly CrashDetector _crashDetector;

        public CrashState(AnimationController animationController, MotorMono motorMono, Movement movement, JumpPower jumpPower, CrashDetector crashDetector, Ragdoll ragdoll)
        {
            _animationController = animationController;
            _motorMono = motorMono;
            _movement = movement;
            _jumpPower = jumpPower;
            _crashDetector = crashDetector;
            _ragdoll = ragdoll;
        }

        public event Action Enter;
        public event Action Exit;

        public int GetHash()
        {
            return -1;
        }

        public void Tick()
        {
            _movement.DoUpdate();
            _jumpPower.DoUpdate();
        }

        public void OnEnter()
        {
            var lastCollidedRigidbodies = _crashDetector.LastCollidedParts.Select(data => data.PlayerPart).ToList();
            
            _ragdoll.Init(_motorMono.MonoWheel.Velocity * 0.75f, lastCollidedRigidbodies);
            _ragdoll.Enable();
            _motorMono.Crash();
        }

        public void OnExit()
        {
            throw new InvalidOperationException("You can not resurrect an character after crash. Reload an scene please");
        }
    }
}
