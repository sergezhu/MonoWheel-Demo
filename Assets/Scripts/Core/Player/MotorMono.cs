using System;
using Core.InputControl;
using Core.Level;
using Core.Settings.SettingsSO;
using UnityEngine;

namespace Core.Player
{
    public class MotorMono : MonoBehaviour, IChangePlayerDataHandler
    {
        public event Action<JumpDirection> JumpStart;
        public event Action JumpEnd;
        public event Action<float> JumpPeak;
        public event Action<int> FaceDirectionChanged;

        public const float DefaultSuspensionDamperReaction = 68.6f;
        public const float DefaultStableBetweenFeetDistance = 0.112941f;
        public const float SuspensionDumperReactionMultiplier = 0.001f;

        private const float Epsilon = 0.0001f;
        private const float VelocityVerifyThreshold = 0.05f;
        private const float AccelerateMultiplier = 100f;
        private const float MaxAnimatorTilt = 45f;
        private const float MinimalRelativeJumpPower = 0.25f;
        private const float MaximalRelativeJumpPower = 20f;
        private const float OverheatMultiplier = 100f;
        private const float StabilizePower = 1.8e-5f;

        [SerializeField]
        private RiderSpawner _riderSpawner;
        [SerializeField]
        private FromGroundDistanceCalculator fromGroundDistanceCalculator;

        [Space]
        [SerializeField]
        private GearSettingsSO _gearSettings;

        private MonoWheel _monoWheel;
        private Rigidbody2D _baseRigidbody;
        private Rigidbody2D _wheelRigidbody;
        private Rigidbody2D _suspensionDumperRigidbody;
        private HingeJoint2D _wheelHingeJoint;
        private GroundChecker _wheelGroundChecker;
        private GroundChecker _baseGroundChecker;
        private PlayerParameters.PlayerParameters _playerParameters;
    
        private Rigidbody2D _currentAnimatedGround;
        private JointMotor2D _motor;

        private int _faceDirection = 1;
        private int _dumpIterationsCount;

        private float _horizontalDirection;
        private float _maxWheelSpeed;
        private float _wheelAccelerateFactor;
        private float _desiredSpeed;
        private float _currentTiltSpeed;
        private float _tiltAccelerate;
        private float _betweenJumpsUnlockTime;
        private float _jumpingStartTime;
        private Vector3 _jumpingStartPosition;
        private Vector2 _baseRBVelocityPrevious;
        private Vector2 _baseRBVelocityCurrent;
        private Vector2 _groundVelocity;
        private Vector2 _storedGroundVelocity;

        private bool _jumpStartFlag;
        private bool _jumpPeakFlag;
        private bool _isJumping;
        private bool _isCrashed;
    
        private Vector3 _storedWheelPosition;
        private Vector3 _storedDamperPosition;
        private Vector3 _storedBasePosition;

        public float Tilt { get; private set; }
        public float RelativeTilt { get; private set; }
        public float RelativePower { get; private set; }
        public float RelativeTiltSigned { get; private set; }
        public float RelativeFeetTilt { get; private set; }
        public float RelativeCharacterTilt { get; private set; }
        public float DesiredRelativeSpeed { get; private set; }
        public float ModifiedTiltAccelerate { get; private set; }
        public float ModifiedTiltDecelerate { get; private set; }
    
        public float MaxModifiedForwardSpeed { get; private set; }
        public float CurrentMotorForwardRelativeSpeed { get; private set; }
        public float CurrentForwardNormalizedRelativeSpeed { get; private set; }
        public float MaxModifiedBackwardSpeed { get; private set; }
        public float CurrentMotorBackwardRelativeSpeed { get; private set; }
        public float CurrentNormalizedBackwardSpeed { get; private set; }
        public float CurrentMotorRelativeSpeed { get; private set; }
        public float CurrentMotorAbsoluteSpeed { get; private set; }
        public float CurrentMotorAccelerate { get; private set; }
        public float ModifiedMotorForwardAccelerateFactor { get; private set; }
        public float ModifiedMotorBackwardAccelerateFactor { get; private set; }
        public float ModifiedMotorDecelerateFactor { get; private set; }
    
        public float OverheatPercent { get; private set; }
        public bool CanJump => MonoWheel.WheelGroundChecker.IsGrounded && _isJumping == false && _betweenJumpsUnlockTime < Epsilon;
        public bool CanSalto => _isJumping && (Time.time - _jumpingStartTime) > _gearSettings.BeforeSaltoLockTime;
        public bool IsStopped => MonoWheel.StopChecker.IsStopped;
        public float BaseJumpHeight => _gearSettings.BaseJumpHeight * PlayerParameters.GetJumpHeightMultiplier();
        public bool IsStabilizeActive { get; set; }
        public bool IsActive { get; private set; }
        public int FaceDirection => _faceDirection;

        public PlayerParameters.PlayerParameters PlayerParameters => _playerParameters;
        public MonoWheel MonoWheel => _monoWheel;
        public RiderSpawner RiderSpawner => _riderSpawner;


        public void Init(PlayerParameters.PlayerParameters playerParameters)
        {
            _playerParameters = playerParameters;
        }
        private void Awake ()
        {
            IsActive = false;
            IsStabilizeActive = true;

            _maxWheelSpeed = _gearSettings.MaxForwardWheelSpeed;
        }

        private void OnDisable()
        {
            DisableEvents();
        }

        private void EnableEvents()
        {
            Debug.Log($"{this} Enable");
        
            _baseGroundChecker.GroundOn += OnBaseAnyGroundOn;
            _baseGroundChecker.GroundOff += OnBaseAnyGroundOff;
        
            _wheelGroundChecker.AnimatedGroundOn += OnWheelAnimatedGroundOn;
            _wheelGroundChecker.AnimatedGroundOff += OnWheelAnimatedGroundOff;
        }

        private void DisableEvents()
        {
            Debug.Log($"{this} Disable");
        
            _baseGroundChecker.GroundOn -= OnBaseAnyGroundOn;
            _baseGroundChecker.GroundOff -= OnBaseAnyGroundOff;
        
            _wheelGroundChecker.AnimatedGroundOn -= OnWheelAnimatedGroundOn;
            _wheelGroundChecker.AnimatedGroundOff -= OnWheelAnimatedGroundOff;
        }


        private void Update()
        {
            if (IsActive == false)
                return;

            _betweenJumpsUnlockTime -= Time.deltaTime;
            _betweenJumpsUnlockTime = Mathf.Clamp(_betweenJumpsUnlockTime, 0, _gearSettings.JumpingDelay);
        }

        private void FixedUpdate ()
        {
            if (IsActive == false)
                return;
        
            UpdateOverheat();
            UpdateGearState();

            if (_dumpIterationsCount > 0 && _isCrashed == false)
                DumpRigidbodyVelocities();

            if (IsStabilizeActive)
                Stabilize();
        
            JumpHandle();
        }
    
        private bool InitializeWheel(MonoWheel monoWheel)
        {
            if(Equals(monoWheel, _monoWheel))
                return  false;

            _monoWheel = monoWheel;
            return true;
        }

        public void OnChangeWheelHandle(MonoWheel wheel)
        {
            InitializeWheel(wheel);
            UpdateReferences();
            OnWheelParametersChanged();
        }

        public void OnChangeCharacterHandle(Character character)
        {
        }

        public void OnChangeReady()
        {
            IsActive = true;
            EnableEvents();
        
            MaxModifiedForwardSpeed = _gearSettings.MaxForwardWheelSpeed * PlayerParameters.GetMaxForwardVelocityMultiplier();
            MaxModifiedBackwardSpeed = _gearSettings.MaxBackwardWheelSpeed * PlayerParameters.GetMaxBackwardVelocityMultiplier();

            ModifiedMotorForwardAccelerateFactor = _gearSettings.WheelForwardAccelerateFactor * PlayerParameters.GetVelocityAccelerateMultiplier() * AccelerateMultiplier;
            ModifiedMotorBackwardAccelerateFactor = _gearSettings.WheelBackwardAccelerateFactor * PlayerParameters.GetVelocityAccelerateMultiplier() * AccelerateMultiplier;
            ModifiedMotorDecelerateFactor = _gearSettings.WheelDecelerateFactor * AccelerateMultiplier;
        }

        public void OnChangePreparing()
        {
            IsActive = false;
            DisableEvents();
        }

        private void OnWheelParametersChanged()
        {
            MonoWheel.WheelResizer.UpdateSize(PlayerParameters.Size, PlayerParameters.DamperStrength);
        }

        private void UpdateReferences()
        {
            _baseRigidbody = MonoWheel.BaseRigidbody;
            _wheelRigidbody = MonoWheel.WheelRigidbody;
            _suspensionDumperRigidbody = MonoWheel.SuspensionDumperRigidbody;
            _wheelHingeJoint = MonoWheel.WheelHingeJoint;
        
            _wheelGroundChecker = MonoWheel.WheelGroundChecker;
            _baseGroundChecker = MonoWheel.BaseGroundChecker;
        }

        private void UpdateGearState()
        {
            //Calculate Accelerate
            var wheelAccelerate = 0f;
            var overheatModifier = Mathf.Clamp(1f - OverheatPercent / 100f, 0, 2f);

            if (Mathf.Approximately(_horizontalDirection, 0) == false)
            {
                MaxModifiedForwardSpeed = _gearSettings.MaxForwardWheelSpeed * PlayerParameters.GetMaxForwardVelocityMultiplier();
                MaxModifiedBackwardSpeed = _gearSettings.MaxBackwardWheelSpeed * PlayerParameters.GetMaxBackwardVelocityMultiplier();
            
                _maxWheelSpeed = Mathf.Approximately(Mathf.Sign(_faceDirection), Mathf.Sign(_horizontalDirection)) 
                    ? MaxModifiedForwardSpeed
                    : MaxModifiedBackwardSpeed;

                ModifiedMotorForwardAccelerateFactor = _gearSettings.WheelForwardAccelerateFactor * PlayerParameters.GetVelocityAccelerateMultiplier() * overheatModifier * AccelerateMultiplier;
                ModifiedMotorBackwardAccelerateFactor = _gearSettings.WheelBackwardAccelerateFactor * PlayerParameters.GetVelocityAccelerateMultiplier() * overheatModifier * AccelerateMultiplier;
                ModifiedMotorDecelerateFactor = _gearSettings.WheelDecelerateFactor * AccelerateMultiplier;

                var wheelAccelerateFactorAbs = Mathf.Approximately(Mathf.Sign(_faceDirection), Mathf.Sign(_horizontalDirection))
                    ? ModifiedMotorForwardAccelerateFactor * _gearSettings.WheelForwardAccelerateFromSpeed.Evaluate(Mathf.Abs(CurrentMotorRelativeSpeed))
                    : ModifiedMotorBackwardAccelerateFactor * _gearSettings.WheelBackwardAccelerateFromSpeed.Evaluate(Mathf.Abs(CurrentMotorRelativeSpeed));

                wheelAccelerateFactorAbs = Mathf.Approximately(Mathf.Sign(CurrentMotorRelativeSpeed), Mathf.Sign(_horizontalDirection))
                    ? wheelAccelerateFactorAbs
                    : ModifiedMotorDecelerateFactor * _gearSettings.WheelDecelerateFromSpeed.Evaluate(Mathf.Abs(CurrentMotorRelativeSpeed));

                wheelAccelerate = wheelAccelerateFactorAbs;
            }
            else
            {
                ModifiedMotorDecelerateFactor = _gearSettings.WheelDecelerateFactor * AccelerateMultiplier;
                wheelAccelerate = ModifiedMotorDecelerateFactor * _gearSettings.WheelDecelerateFromSpeed.Evaluate(Mathf.Abs(CurrentMotorRelativeSpeed));
            }

            _desiredSpeed = _isCrashed ? 0 : _maxWheelSpeed * _horizontalDirection;
        
            //var wheelAccelerate = Mathf.Approximately(_desiredSpeed, 0) ? _wheelDecelerateFactor : _wheelAccelerateFactor;
            wheelAccelerate = _isCrashed ? ModifiedMotorDecelerateFactor : wheelAccelerate;

            //Calculate Motor Speed
            _motor = MonoWheel.WheelHingeJoint.motor;
            var wheelSpeedDeltaBefore = _desiredSpeed - CurrentMotorAbsoluteSpeed;
            CurrentMotorAccelerate = Mathf.Sign(wheelSpeedDeltaBefore) * wheelAccelerate;
            CurrentMotorAbsoluteSpeed += CurrentMotorAccelerate * Time.fixedDeltaTime;
            var wheelSpeedDeltaAfter = _desiredSpeed - CurrentMotorAbsoluteSpeed;
        
            if (Math.Abs(Mathf.Sign(wheelSpeedDeltaBefore) - Mathf.Sign(wheelSpeedDeltaAfter)) > Epsilon)
                CurrentMotorAbsoluteSpeed = _desiredSpeed;
        
            _motor.motorSpeed = CurrentMotorAbsoluteSpeed;
            MonoWheel.WheelHingeJoint.motor = _motor;

            //Calculate Relative Motor Speed
            CurrentMotorForwardRelativeSpeed = CurrentMotorAbsoluteSpeed / MaxModifiedForwardSpeed;
            CurrentMotorBackwardRelativeSpeed = CurrentMotorAbsoluteSpeed / MaxModifiedBackwardSpeed;
            CurrentMotorRelativeSpeed = Mathf.Approximately(Mathf.Sign(_faceDirection), Mathf.Sign(_horizontalDirection))
                ? CurrentMotorForwardRelativeSpeed
                : CurrentMotorBackwardRelativeSpeed;

            //Calculate Tilt
            ModifiedTiltAccelerate = _gearSettings.TiltAccelerateFactor * PlayerParameters.GetTiltAccelerateMultiplier() * AccelerateMultiplier;
            ModifiedTiltDecelerate = _gearSettings.TiltDecelerateFactor * AccelerateMultiplier;

            var tiltAccelerate = _desiredSpeed == 0
                ? ModifiedTiltDecelerate
                : ModifiedTiltAccelerate;
        
            var tiltSpeedDeltaBefore = _desiredSpeed - _currentTiltSpeed;
            _currentTiltSpeed += Mathf.Sign(tiltSpeedDeltaBefore) * tiltAccelerate * Time.fixedDeltaTime;
            var tiltSpeedDeltaAfter = _desiredSpeed - _currentTiltSpeed;
        
            //Check Is Achieved Desired Speed
            if (Math.Abs(Mathf.Sign(tiltSpeedDeltaBefore) - Mathf.Sign(tiltSpeedDeltaAfter)) > Epsilon)
                _currentTiltSpeed = _desiredSpeed;
        
            RelativePower = _gearSettings.TiltFromSpeed.Evaluate(Mathf.Abs(_currentTiltSpeed) / _maxWheelSpeed);
        }

        public void SetDirection(int direction)
        {
            _horizontalDirection = Mathf.Clamp(direction, -1, 1);
        }

        private void Jump(float relativeJumpPower)
        {
            relativeJumpPower = Mathf.Clamp(relativeJumpPower, MinimalRelativeJumpPower, MaximalRelativeJumpPower);
        
            if (CanJump == false)
                return;

            InitJump();

            var jumpForceFactor = _gearSettings.ForwardJumpForceFactor;
            var jumpDirection = JumpDirection.Forward;
            if(Mathf.Approximately(_horizontalDirection, 0))
            {
                if (Mathf.Approximately(Mathf.Sign(CurrentMotorAbsoluteSpeed), Mathf.Sign(_faceDirection)) == false)
                {
                    jumpForceFactor = _gearSettings.BackwardJumpForceFactor;
                    jumpDirection = Mathf.Approximately(CurrentMotorAbsoluteSpeed, 0) ? JumpDirection.Forward : JumpDirection.Backward;
                }
            }
            else
            {
                if (Mathf.Approximately(Mathf.Sign(CurrentMotorAccelerate), Mathf.Sign(_faceDirection)) == false)
                {
                    jumpForceFactor = _gearSettings.BackwardJumpForceFactor;
                    jumpDirection = JumpDirection.Backward;
                }
            }

            var powerDirection = Vector3.up;
            var jumpForce = jumpForceFactor * (_wheelRigidbody.mass + _baseRigidbody.mass);
            var jumpPower = powerDirection.normalized * (jumpForce * relativeJumpPower);
            _baseRigidbody.AddForce(jumpPower, ForceMode2D.Impulse);
        
            JumpStart?.Invoke(jumpDirection);
            Debug.Log($"Jump Start.  relativeJumpPower [{relativeJumpPower}]     _wheelAccelerate [{CurrentMotorAccelerate}]     jumpForceFactor [{jumpForceFactor}]     Ground Velocity [{_groundVelocity}]");
        }
    
        public void HeightJump(float targetHeight)
        {
            var relativeHeight = targetHeight * PlayerParameters.GetJumpHeightMultiplier() / BaseJumpHeight;
            var relativePower = Mathf.Sqrt(relativeHeight);
            Jump(relativePower);
        }
    
        public void PowerJump(float relativePower)
        {
            relativePower *= Mathf.Sqrt(PlayerParameters.GetJumpHeightMultiplier());
            Jump(relativePower);
        }

        private void InitJump()
        {
            _baseRBVelocityCurrent = _baseRigidbody.velocity;
            _baseRBVelocityPrevious = _baseRBVelocityCurrent;
        
            _jumpStartFlag = true;
            _jumpPeakFlag = false;
            //_jumpPreparingFlag = false;
            _isJumping = true;
            _jumpingStartTime = Time.time;
            _jumpingStartPosition = _wheelRigidbody.transform.position;

            _storedGroundVelocity = _groundVelocity;
        }

        private void JumpHandle()
        {
            _baseRBVelocityCurrent = _baseRigidbody.velocity;

            var isVerticalVelocityDirectionChanged = (_baseRBVelocityPrevious.y) > 0 && (_baseRBVelocityCurrent.y) < 0;
            var isDeltaOfVelocityCorrect = Mathf.Abs(_baseRBVelocityPrevious.y - _baseRBVelocityCurrent.y) > VelocityVerifyThreshold;
            
            _baseRBVelocityPrevious = _baseRBVelocityCurrent;

            if(_isJumping && _jumpStartFlag && isVerticalVelocityDirectionChanged)
            {
                _jumpStartFlag = false;
                _jumpPeakFlag = true;

                var jumpPeakHeight = _baseRigidbody.transform.position.y - _jumpingStartPosition.y;
                JumpPeak?.Invoke(jumpPeakHeight);
            
                return;
            }

            if (_isJumping && _jumpPeakFlag && _wheelGroundChecker.IsGrounded)
            {
                _isJumping = false;
                _jumpPeakFlag = false;
                _jumpingStartTime = 0;
                _betweenJumpsUnlockTime = _gearSettings.JumpingDelay;

                JumpEnd?.Invoke();
            }
        }

        private void Stabilize()
        {
            var stabilizedVector = StabilizedVectorCalculate(_currentTiltSpeed);
            var baseUpVector = Vector3.up;

            var axisFromRotate = Vector3.Cross (baseUpVector, stabilizedVector);
            Tilt = Vector3.Angle(baseUpVector, stabilizedVector) * Mathf.Sign(axisFromRotate.z) * -1;

            float torqueForce = axisFromRotate.magnitude * Mathf.Sign(axisFromRotate.z) - _baseRigidbody.angularVelocity;
            torqueForce *= StabilizePower;

            _baseRigidbody.AddTorque(torqueForce * _baseRigidbody.mass, ForceMode2D.Impulse);
            _baseRigidbody.MoveRotation(-1 * Tilt);
            _suspensionDumperRigidbody.MoveRotation(1 * Tilt);
        }

        private Vector3 StabilizedVectorCalculate(float desiredSpeed)
        {
            var moveDirection = (int) Mathf.Sign(desiredSpeed);
       
            var moveSpeed = Mathf.Abs(desiredSpeed);
            var relativeSpeed = moveSpeed / _gearSettings.MaxForwardWheelSpeed;
            RelativeTilt = _gearSettings.TiltFromSpeed.Evaluate(moveSpeed / _maxWheelSpeed);
            RelativeTiltSigned = moveDirection * _faceDirection * RelativeTilt;
            DesiredRelativeSpeed = moveDirection * _faceDirection * relativeSpeed;

            var maxTilt = _faceDirection == moveDirection ? _gearSettings.ForwardMaxTiltAngle : _gearSettings.BackwardMaxTiltAngle;
            var stabilizedTilt = maxTilt * RelativeTilt * moveDirection * -1;

            RelativeFeetTilt = Mathf.Sign(DesiredRelativeSpeed) * Mathf.Abs(stabilizedTilt / MaxAnimatorTilt);
            RelativeCharacterTilt = Mathf.Sign(DesiredRelativeSpeed) * Mathf.Abs(stabilizedTilt / maxTilt);

            var stabilizedVector = Quaternion.Euler(0, 0, stabilizedTilt) * Vector3.up;
        
            return stabilizedVector;
        }

        private void DumpRigidbodyVelocities()
        {
            _dumpIterationsCount--;
            _dumpIterationsCount = _dumpIterationsCount < 0 ? 0 : _dumpIterationsCount;
        
            var baseRigidbodyVelocity = _baseRigidbody.velocity;
            baseRigidbodyVelocity.x *= 0.1f;
            _baseRigidbody.velocity = baseRigidbodyVelocity;

            var suspensionDamperVelocity = _suspensionDumperRigidbody.velocity;
            suspensionDamperVelocity.x *= 0.1f;
            _suspensionDumperRigidbody.velocity = suspensionDamperVelocity;
        
            _motor = _wheelHingeJoint.motor;
        
            var newSpeed = _motor.motorSpeed * 0.1f;

            _motor.motorSpeed = newSpeed;
        }

        public void ToggleFlipDirection()
        {
            _faceDirection = _faceDirection == 1 ? -1 : 1;
            FaceDirectionChanged?.Invoke(_faceDirection);
        }
    
        public void EnableTransformLinks()
        {
            MonoWheel.EnableTransformLinks();
        }
        
        public void DisableTransformLinks()
        {
            MonoWheel.DisableTransformLinks();
        }
    
        public void EnableMonoSystemCollisionDetection()
        {
            MonoWheel.EnableMonoSystemCollisionDetection();
        }

        public void DisableMonoSystemCollisionDetection()
        {
            MonoWheel.DisableMonoSystemCollisionDetection();
        }

        public void UpdatePhysicVelocities(Vector2 rootVelocity)
        {
            MonoWheel.UpdatePhysicVelocities(rootVelocity);
        }

        public void Crash()
        {
            _isCrashed = true;
            IsStabilizeActive = false;
            DoCrashPitch();
            SetDirection(0);

            DisableTransformLinks();
        }

        private void DoCrashPitch()
        {
            var crashPitchPower = -1 * 1.5f * (_wheelRigidbody.mass + _baseRigidbody.mass) * _horizontalDirection;
            _baseRigidbody.AddTorque(crashPitchPower , ForceMode2D.Impulse);
        }
    
        private void OnBaseAnyGroundOn()
        {
            if (_isCrashed)
                return;
        
            if (_wheelGroundChecker.IsGrounded == false)
            {
                IsStabilizeActive = false;
            }
        }
        private void OnBaseAnyGroundOff()
        {
            if (_isCrashed)
                return;
        
            IsStabilizeActive = true;
        
        }

        private void OnWheelAnimatedGroundOn(Rigidbody2D rb)
        {
            if (_isCrashed)
                return;

            _currentAnimatedGround = rb;
        }

        private void OnWheelAnimatedGroundOff()
        {
            if (_isCrashed)
                return;

            _currentAnimatedGround = null;
        }

        private void UpdateOverheat()
        {
            if (_wheelGroundChecker.IsAverageNormalCalculationEnable == false)
                return;
            var averageNormal = _wheelGroundChecker.GetAverageGroundContactsNormal();
            var inputVector = _horizontalDirection * Vector2.left;

            OverheatPercent = 0f;

            if (Mathf.Approximately(inputVector.x, 0) == false  &&  Mathf.Approximately(averageNormal.x, 0) == false)
            {
                var dot = Vector2.Dot(inputVector, averageNormal);
                dot = float.IsNaN(dot) ? 0 : dot;
                
                OverheatPercent = dot * OverheatMultiplier * _playerParameters.GetOverloadSpeedMultiplier();
                OverheatPercent += dot * dot * OverheatMultiplier * _playerParameters.GetOverloadSpeedAngleMultiplier();
            }
        }

        public float GetFootStepHeightRelative()
        {
            return fromGroundDistanceCalculator.GetFootStepHeightRelative();
        }
    }
}