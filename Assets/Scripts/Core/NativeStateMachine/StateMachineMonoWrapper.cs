using Core.AnimationControl;
using Core.InputControl;
using Core.Player;
using UnityEngine;

namespace Core.NativeStateMachine
{
    public class StateMachineMonoWrapper : MonoBehaviour, IChangePlayerDataHandler
    {
        private MotorMono _motorMono;
        private CrashDetector _crashDetector;
        private InputController _inputController;
        private BeforeStopDelayTimer _beforeStopDelayTimer;
        private JumpPower _jumpPower;
        private Movement _movement;
        private SaltoTracker _saltoTracker;
        private AccelerationProvider _accelerationProvider;

        private StateMachine _stateMachine;

        private MoveState _move;
        private StopState _stop;
        private PreparingWithTimerStopState _preparingStopWithTimer;
        private RaisingFootBeforeStopState _raisingFootBeforeStop;
        private RaisingFootAfterStopState _raisingFootAfterStop;
        private MoveToJumpPreparingState _moveToJumpPreparing;
        private JumpPreparingToMoveState _jumpPreparingToMove;
        private JumpingState _jumping;
        private SitState _sit;
        private FlipState _flip;
        private SaltoState _salto;
        private CrashState _crash;

        private AnimationController _animationController;
        private Ragdoll _ragdoll;

        private bool _isInitialized;
        private bool _readyWait;

        public MotorMono MotorMono => _motorMono;
        public JumpPower JumpPower => _jumpPower;
        public Movement Movement => _movement;
        public SaltoTracker SaltoTracker => _saltoTracker;
        public AccelerationProvider AccelerationProvider => _accelerationProvider;

        public string CurrentStateName => _stateMachine.GetCurrentStateName();

        private void Awake()
        {
            _stateMachine = new StateMachine();
            _stateMachine.Stop();
        }

        public void Init(InputController inputController, MotorMono motorMono, CrashDetector crashDetector, BeforeStopDelayTimer beforeStopDelayTimer, JumpPower jumpPower, 
            Movement movement, SaltoTracker saltoTracker, AccelerationProvider accelerationProvider)
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            
            _inputController = inputController;
            _motorMono = motorMono;
            _crashDetector = crashDetector;
            _beforeStopDelayTimer = beforeStopDelayTimer;
            _jumpPower = jumpPower;
            _movement = movement;
            _saltoTracker = saltoTracker;
            _accelerationProvider = accelerationProvider;

            if (_readyWait)
            {
                SetupStateMachine();
                _readyWait = false;
            }
        }

        private void Update()
        {
            if(_stateMachine.IsActive)
                _stateMachine.Tick();
        }

        public void OnChangeWheelHandle(MonoWheel wheel)
        {
        }
        
        public void OnChangeCharacterHandle(Character character)
        {
            _animationController = character.AnimationController;
            _ragdoll = character.Ragdoll;
        }

        public void OnChangeReady()
        {
            if (_isInitialized == false)
            {
                _readyWait = true;
                return;
            }
            
            SetupStateMachine();
            Start();
        }

        public void OnChangePreparing()
        {
            _stateMachine.SaveState();
            _stateMachine.Stop();
        }

        public void Stop()
        {
            _stateMachine.Stop();
        }

        public void Start()
        {
            _stateMachine.Start();
        }
        
        public void DisableTransitions()
        {
            _stateMachine.DisableTransitions();
        }

        public void EnableTransitions()
        {
            _stateMachine.EnableTransitions();
        }

        private void SetupStateMachine()
        {
            UnityEngine.Debug.Log("SetupStateMachine");
            
            _stateMachine.LoadState();

            SetupStates();
            SetupTransitions();

            _stateMachine.SetState(_move);
        }

        private void SetupTransitions()
        {
            _stateMachine.ClearTransitions();
            
            _stateMachine.AddTransition(_move, _preparingStopWithTimer, Move_To_PreparingStopWithTimer_Condition);
            _stateMachine.AddTransition(_preparingStopWithTimer, _raisingFootBeforeStop, PreparingStopWithTimer_To_RaisingFootBeforeStop_Condition);
            _stateMachine.AddTransition(_preparingStopWithTimer, _move, PreparingStopWithTimer_To_Move_Condition);
            _stateMachine.AddTransition(_raisingFootBeforeStop, _stop, RaisingFootBeforeStop_To_Stop_Condition);
            _stateMachine.AddTransition(_stop, _raisingFootAfterStop, Stop_To_RaisingFootAfterStop_Condition);
            _stateMachine.AddTransition(_raisingFootAfterStop, _move, RaisingFootAfterStop_To_Move_Condition);

            _stateMachine.AddTransition(_move, _moveToJumpPreparing, Move_To_MoveToJumpPreparing_Condition);
            _stateMachine.AddTransition(_moveToJumpPreparing, _jumping, MoveToJumpPreparing_To_Jumping_Condition);
            _stateMachine.AddTransition(_moveToJumpPreparing, _jumpPreparingToMove, MoveToJumpPreparing_To_JumpPreparingToMove_Condition);
            _stateMachine.AddTransition(_jumpPreparingToMove, _moveToJumpPreparing, JumpPreparingToMove_To_MoveToJumpPreparing_Condition);
            _stateMachine.AddTransition(_jumpPreparingToMove, _jumping, JumpPreparingToMove_To_Jumping_Condition);
            _stateMachine.AddTransition(_jumpPreparingToMove, _move, JumpPreparingToMove_To_Move_Condition);
            _stateMachine.AddTransition(_jumping, _move, Jumping_To_Move_Condition);

            _stateMachine.AddTransition(_move, _sit, Move_To_Sit_Condition);
            _stateMachine.AddTransition(_sit, _move, Sit_To_Move_Condition);

            _stateMachine.AddTransition(_move, _flip, Move_To_Flip_Condition);
            _stateMachine.AddTransition(_flip, _move, Flip_To_Move_Condition);

            _stateMachine.AddTransition(_jumping, _salto, Jumping_To_Salto_Condition);
            _stateMachine.AddTransition(_salto, _jumping, Salto_To_Jumping_Condition);

            _stateMachine.AddTransition(_move, _crash, Move_To_Crash_Condition);
            _stateMachine.AddTransition(_moveToJumpPreparing, _crash, PreparingBeforeJump_To_Crash_Condition);
            _stateMachine.AddTransition(_jumping, _crash, Jumping_To_Crash_Condition);
            _stateMachine.AddTransition(_sit, _crash, Sit_To_Crash_Condition);
            _stateMachine.AddTransition(_salto, _crash, Salto_To_Crash_Condition);
        }

        private void SetupStates()
        {
            _move = new MoveState(_animationController, _accelerationProvider, _motorMono, _movement, _jumpPower);
            _stop = new StopState(_animationController, _inputController, _beforeStopDelayTimer, _motorMono);
            _preparingStopWithTimer = new PreparingWithTimerStopState(_animationController, _accelerationProvider, _beforeStopDelayTimer, _jumpPower);
            _raisingFootBeforeStop = new RaisingFootBeforeStopState(_animationController, _inputController, _motorMono);
            _raisingFootAfterStop = new RaisingFootAfterStopState(_animationController, _inputController);

            _moveToJumpPreparing = new MoveToJumpPreparingState(_animationController, _accelerationProvider, _motorMono, _jumpPower, _movement);
            _jumpPreparingToMove = new JumpPreparingToMoveState(_animationController, _accelerationProvider, _motorMono, _jumpPower, _movement);
            _jumping = new JumpingState(_animationController, _accelerationProvider, _motorMono, _movement, _jumpPower, _saltoTracker);

            _sit = new SitState(_animationController, _motorMono, _movement, _jumpPower);
            _flip = new FlipState(_animationController, _inputController);
            _salto = new SaltoState(_animationController, _inputController, _movement, _jumpPower, _saltoTracker);
            _crash = new CrashState(_animationController, _motorMono, _movement, _jumpPower, _crashDetector, _ragdoll);
        }

        public void DebugCrash()
        {
            _stateMachine.SetState(_crash);
        }

        private bool Move_To_PreparingStopWithTimer_Condition()
        {
            var value = _inputController.IsActiveMoveLeftButton() == false
                        && _inputController.IsActiveMoveRightButton() == false
                        && _inputController.IsActiveJumpButton() == false
                        && MotorMono.IsStopped
                        && _animationController.IsMoveAnimatorState();

            return value;
        }

        private bool PreparingStopWithTimer_To_RaisingFootBeforeStop_Condition()
        {
            return _beforeStopDelayTimer.IsActive == false;
        }

        private bool PreparingStopWithTimer_To_Move_Condition()
        {
            var value = _inputController.IsActiveMoveLeftButton()
                        || _inputController.IsActiveMoveRightButton()
                        || _inputController.IsActiveJumpButton()
                        || MotorMono.IsStopped == false;

            return value;
        }

        private bool RaisingFootBeforeStop_To_Stop_Condition()
        {
            return _animationController.RaiseFootBeforeStopAnimationEventHandler.IsReady;
        }

        private bool Stop_To_RaisingFootAfterStop_Condition()
        {
            var value = (_inputController.IsActiveMoveLeftButton()
                         || _inputController.IsActiveMoveRightButton()
                         || _inputController.IsActiveJumpButton()
                         || MotorMono.IsStopped == false)
                        && _animationController.IsStopDependsFromWheelSize();

            return value;
        }

        private bool RaisingFootAfterStop_To_Move_Condition()
        {
            return _animationController.RaiseFootAfterStopAnimationEventHandler.IsReady;
        }

        private bool Move_To_MoveToJumpPreparing_Condition()
        {
            return _inputController.IsActiveJumpButton()
                   && MotorMono.CanJump
                   && _animationController.IsMoveAnimatorState();
        }

        private bool MoveToJumpPreparing_To_Jumping_Condition()
        {
            return _inputController.IsPressedJumpButton() == false
                   && MotorMono.CanJump;
        }

        private bool JumpPreparingToMove_To_Jumping_Condition()
        {
            return _inputController.IsPressedJumpButton() == false && MotorMono.CanJump;
        }

        private bool MoveToJumpPreparing_To_JumpPreparingToMove_Condition()
        {
            return MotorMono.CanJump == false;
        }

        private bool JumpPreparingToMove_To_MoveToJumpPreparing_Condition()
        {
            return MotorMono.CanJump == true;
        }

        private bool JumpPreparingToMove_To_Move_Condition()
        {
            return _jumpPower.IsActive == false;
        }

        private bool Jumping_To_Move_Condition()
        {
            return _jumping.IsJumpingEnded;
        }

        private bool Move_To_Sit_Condition()
        {
            var value = (_inputController.IsActiveMoveLeftButton() || _inputController.IsActiveMoveRightButton())
                        && _inputController.IsPressedSitButton()
                        && _animationController.IsMoveAnimatorState();

            return value;
        }

        private bool Sit_To_Move_Condition()
        {
            return _inputController.IsPressedSitButton() == false
                   || MotorMono.IsStopped;
        }

        private bool Move_To_Flip_Condition()
        {
            var value = (_inputController.IsActiveMoveLeftButton() || _inputController.IsActiveMoveRightButton())
                        && _inputController.IsActiveFlipButton()
                        && _animationController.IsMoveAnimatorState();

            return value;
        }

        private bool Flip_To_Move_Condition()
        {
            return _animationController.FlipAnimationEventHandler.IsReady;
        }

        private bool Jumping_To_Salto_Condition()
        {
            return _inputController.IsPressedSaltoButton() && MotorMono.CanSalto && _jumping.SaltoLocked == false;
        }

        private bool Salto_To_Jumping_Condition()
        {
            return _saltoTracker.IsSuccessfully;
        }


        private bool Move_To_Crash_Condition()
        {
            return _crashDetector.CrashState != CrashType.None
                   && _animationController.IsMoveAnimatorState();
        }

        private bool PreparingBeforeJump_To_Crash_Condition()
        {
            return _crashDetector.CrashState != CrashType.None;
        }

        private bool Jumping_To_Crash_Condition()
        {
            return _crashDetector.CrashState != CrashType.None;
        }

        private bool Sit_To_Crash_Condition()
        {
            return _crashDetector.CrashState != CrashType.None;
        }

        private bool Salto_To_Crash_Condition()
        {
            return _crashDetector.CrashState != CrashType.None;
        }
    }
}

