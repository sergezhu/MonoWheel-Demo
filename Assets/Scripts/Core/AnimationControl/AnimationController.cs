using System.Linq;
using Core.AnimationControl.AnimationEventHandlers;
using UnityEngine;

namespace Core.AnimationControl
{
    public class AnimationController : MonoBehaviour
    {
    
        [SerializeField]
        private Animator _mainAnimator;
        [SerializeField]
        private Animator _feetAnimator;

        public FlipAnimationEventHandler FlipAnimationEventHandler { get; private set; }
        public RaiseFootBeforeStopAnimationEventHandler RaiseFootBeforeStopAnimationEventHandler { get; private set; }
        public RaiseFootAfterStopAnimationEventHandler RaiseFootAfterStopAnimationEventHandler { get; private set; }
        public JumpAnimationEventHandler JumpAnimationEventHandler { get; private set; }
        public SitAnimationEventHandler SitAnimationEventHandler { get; private set; }

        public static readonly int HashSit = Animator.StringToHash("Sit");
        public static readonly int HashStop = Animator.StringToHash("Stop");
        public static readonly int HashMove = Animator.StringToHash("Move");
        public static readonly int HashFlip = Animator.StringToHash("Flip");
        public static readonly int HashSalto = Animator.StringToHash("Salto");
        public static readonly int HashMovingBody = Animator.StringToHash("Moving_Body");
        public static readonly int HashMovingFeet = Animator.StringToHash("Moving_Feet");
        public static readonly int HashStopDependsFromWheelSize = Animator.StringToHash("Stop_DependsFromWheelSize");
        public static readonly int HashRaisingFootBeforeStop = Animator.StringToHash("RaisingFootBeforeStop");
        public static readonly int HashRaisingFootAfterStop = Animator.StringToHash("RaisingFootAfterStop");
        public static readonly int HashRaisingFootBeforeJump = Animator.StringToHash("RaisingFootBeforeJump");
        public static readonly int HashJumpPreparing = Animator.StringToHash("JumpPreparing");
        public static readonly int HashRaisingFootBeforeSit = Animator.StringToHash("RaisingFootBeforeSit");
        public static readonly int HashRelativeSpeed = Animator.StringToHash("RelativeSpeed");
        public static readonly int HashRelativeNormalizedSpeed = Animator.StringToHash("RelativeNormalizedSpeed");
        public static readonly int HashRelativeTilt = Animator.StringToHash("RelativeTilt");
        public static readonly int HashVerticalRelativeAcceleration = Animator.StringToHash("VerticalRelativeAcceleration");
        public static readonly int HashJumpPreparingPower = Animator.StringToHash("JumpPreparingPower");
        public static readonly int HashSaltoPreparingPower = Animator.StringToHash("SaltoPreparingPower");
        public static readonly int HashRelativeWheelSize = Animator.StringToHash("RelativeWheelSize");
        public static readonly int HashSaltoRotateRelative = Animator.StringToHash("SaltoRotateRelative");


        private int[] _animatorHashes;
        private string[] _animatorHashNames;
        private AnimatorStateInfo _currentBaseState;

        //private static readonly int RelativeTilt = Animator.StringToHash("RelativeTilt");

        public int CurrentHash { get; private set; }

        public Animator MainAnimator => _mainAnimator;
        public Animator FeetAnimator => _feetAnimator;

        private void Awake()
        {
            _animatorHashes = new[]
            {
                HashSit,
                HashStop,
                HashMove,
                HashFlip,
                HashSalto,
                HashRaisingFootBeforeStop,
                HashRaisingFootAfterStop,
                HashRaisingFootBeforeJump,
                HashRaisingFootBeforeSit,
                HashJumpPreparing
            };

            _animatorHashNames = new[]
            {
                "Sit",
                "Stop",
                "Move",
                "Flip",
                "Salto",
                "RaisingFootBeforeStop",
                "RaisingFootAfterStop",
                "RaisingFootBeforeJump",
                "RaisingFootBeforeSit",
                "JumpPreparing"
            };

            FlipAnimationEventHandler = GetComponent<FlipAnimationEventHandler>();
            RaiseFootBeforeStopAnimationEventHandler = GetComponent<RaiseFootBeforeStopAnimationEventHandler>();
            RaiseFootAfterStopAnimationEventHandler = GetComponent<RaiseFootAfterStopAnimationEventHandler>();
            JumpAnimationEventHandler = GetComponent<JumpAnimationEventHandler>();
            SitAnimationEventHandler = GetComponent<SitAnimationEventHandler>();
        }

        private void Update()
        {
            _currentBaseState = _mainAnimator.GetCurrentAnimatorStateInfo(0);
        }

        public void SetMainAnimatorState(int hash)
        {
            MainAnimator.SetBool(hash, true);
            FeetAnimator.SetBool(hash, true);
            CurrentHash = hash;

            var hashIndex = _animatorHashes.ToList().IndexOf(hash);

            _animatorHashes.Where(h => h != hash).ToList().ForEach(otherHash =>
            {
                MainAnimator.SetBool(otherHash, false);
                FeetAnimator.SetBool(otherHash, false);
            });
        }

        public bool IsMoveAnimatorState()
        {
            return _currentBaseState.shortNameHash == HashMovingBody;
        }

        public bool IsStopDependsFromWheelSize()
        {
            return _currentBaseState.shortNameHash == HashStopDependsFromWheelSize;
        }
    }
}
