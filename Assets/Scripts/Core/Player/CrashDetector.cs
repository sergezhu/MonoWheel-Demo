using System;
using System.Collections.Generic;
using System.Linq;
using Core.InputControl;
using Core.Level.Environment.Triggers;
using Core.Settings.SettingsSO;
using UnityEngine;
using Random = UnityEngine.Random;

using UnityEditor;

namespace Core.Player
{
    public enum CrashType
    {
        None,
        BigHeightFall,
        ObstacleCollision,
        OverHeat,
        FootMiss,
        CrashAreaEnter
    }

    public enum WarningType
    {
        None,
        First,
        Second
    }

    public enum CollisionTouchDirection
    {
        Left,
        Right
    }

    [RequireComponent(typeof(MotorMono))]
    public class CrashDetector : MonoBehaviour, IChangePlayerDataHandler
    {
        public event Action<WarningType> WarningStateChanged;
        public event Action<CrashType> CrashStateChanged;
        public event Action<float> OverheatValueChanged;
        public event Action<CollisionTouchDirection> DangerousTouched;
        public event Action DangerousNonTouched;

        [Space]
        [SerializeField]
        private CrashDetectorSettingsSO _settings;

        [Space]
        [SerializeField]
        private List<TriggerCrash> _crashAreas;

        private MotorMono _motorMono;
        private SaltoTracker _saltoTracker;
        private GroundChecker _wheelGroundChecker;
        private Rigidbody2D _wheelRB;
        private Rigidbody2D _baseRB;
        private CollisionDetector _baseCollisionDetector;
        private CollisionDetector _wheelCollisionDetector;
        private Transform _characterCollisionDetectorsParent;
        private DangerousCollisionsDelayTimer _dangerousCollisionsDelayTimer;

        private Vector2 _startFallPosition;
        private Vector2 _endFallPosition;
        private Vector2 _lastKinematicVelocity;
        private float _fallCrashProbability;
        private float _collisionCrashProbability;
        private float _fallHeight;
        private float _warningTimer;

        private bool _isHeightCrashDetecting;
        private bool _isCollisionCrashDetecting;
        private bool _isOverheatCrashDetecting;
        private bool _isFootMissCrashDetecting;
        private bool _isCrashAreaEnterDetecting;
        private bool _isGrounded;
        private bool _isJumping;
        private bool _isActive;
        private bool _isInitialized;
        private bool _enablingWait;
        private bool _isSubscribed;

        private List<CollisionDetector> _characterCollisionDetectors;
        private List<CollisionDetectorInfo> _collidedPartsData;
        private List<CollisionDetectorInfo> _lastCollidedPartsData;
        private List<Rigidbody2D> _dangerousTouchedParts;

        public CrashType CrashState { get; private set; }
        public WarningType WarningState { get; private set; }
        public IReadOnlyList<CollisionDetectorInfo> LastCollidedParts => _lastCollidedPartsData;
        public float ModifiedOverheatSpeed => (1f + _motorMono.OverheatPercent / 100f) * _settings.OverheatIncreasingSpeed;

        public float ModifiedMinHeight => _settings.MinHeight * _motorMono.PlayerParameters.GetMinFallHeightMultiplier();
        public float ModifiedMaxHeight => _settings.MaxHeight * _motorMono.PlayerParameters.GetMaxFallHeightMultiplier();


        public void Init(MotorMono motorMono, SaltoTracker saltoTracker, DangerousCollisionsDelayTimer dangerousCollisionsDelayTimer)
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
        
            _motorMono = motorMono;
            _saltoTracker = saltoTracker;
            _dangerousCollisionsDelayTimer = dangerousCollisionsDelayTimer;
        
            if(_enablingWait)
                Enable();
            else
                DisableDetection();

            _collidedPartsData = new List<CollisionDetectorInfo>();
            _lastCollidedPartsData = new List<CollisionDetectorInfo>();
            _dangerousTouchedParts = new List<Rigidbody2D>();
        }

        private void DoReset()
        {
            CrashState = CrashType.None;
            WarningState = WarningType.None;
        
            CrashStateChanged?.Invoke(CrashState);
            WarningStateChanged?.Invoke(WarningState);
        
            _startFallPosition = Vector2.zero;
            _endFallPosition = Vector2.zero;
            _isHeightCrashDetecting = false;
            //LastCollidedPart = null;
        
            _lastCollidedPartsData.Clear();
        }

        private void Update()
        {
            if (CrashState != CrashType.None || _isActive == false)
                return;
        
            UpdatePowerOverheatDetection();

            _dangerousCollisionsDelayTimer.UpdateTimer();
        }

        private void FixedUpdate()
        {
            if (CrashState != CrashType.None || _isActive == false)
                return;

            CheckCollidedData();
        
            var groundCheckerIsGrounded = _wheelGroundChecker.IsGrounded;
        
            if (_isGrounded != groundCheckerIsGrounded)
            {
                _isGrounded = groundCheckerIsGrounded;

                if (_isGrounded == false)
                {
                    BeginHeightCrashDetect();
                }
                else
                {
                    EndHeightCrashDetect();
                }
            }
        
            //if currentState == Stop
            UpdateFootMissDetection();
        }

        private void CheckCollidedData()
        {
            if (_collidedPartsData.Count == 0)
                return;

            _lastCollidedPartsData.AddRange(_collidedPartsData);
        
            if(_dangerousTouchedParts.Count == 0)
                CollisionCrashHandle();
        
            _collidedPartsData.Clear();
        }

        public void EnableDetection()
        {
            _isActive = true;
            BeginDetection();
            BeginHeightCrashDetect();
        }
    
        public void DisableDetection()
        {
            _isActive = false;
            EndDetection();
            EndHeightCrashDetect();
        }
    
        private void Enable()
        {
            if (_isInitialized == false)
            {
                _enablingWait = true;
                return;
            }
            _isActive = true;
            _enablingWait = false;
        
            Subscribe();
            BeginDetection();
        }

        private void Disable()
        {
            EndDetection();
            Unsubscribe();
            _isActive = false;
        }

        private void Subscribe()
        {
            if (_isSubscribed || _isInitialized == false)
                return;

            _isSubscribed = true;
        
            SubscribeWheelEvents();
            SubscribeCharacterEvents();
            SubscribeCommonEvents();
        }

        private void Unsubscribe()
        {
            if (_isSubscribed == false || _isInitialized == false)
                return;

            _isSubscribed = false;
        
            UnsubscribeCommonEvents();
            UnsubscribeCharacterEvents();
            UnsubscribeWheelEvents();
        }

        private void BeginDetection()
        {
            DoReset();
            BeginCollisionCrashDetect();
            BeginOverheatCrashDetect();
            BeginFootMissCrashDetect();
            BeginCrashAreaEnterDetect();
        }
    
        private void EndDetection()
        {
            EndCollisionCrashDetect();
            EndOverheatCrashDetect();
            EndFootMissCrashDetect();
            EndCrashAreaEnterDetect();
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        private void SubscribeCommonEvents()
        {
            Debug.Log($"{this} EnableEvents");
        
            _motorMono.JumpStart += OnJumpStart;
            _motorMono.JumpPeak += OnJumpPeak;
            _motorMono.JumpEnd += OnJumpEnd;

            _saltoTracker.RootKinematicVelocityChanged += OnKinematicVelocityChanged;
        }

        private void UnsubscribeCommonEvents()
        {
            Debug.Log($"{this} DisableEvents");
        
            _motorMono.JumpStart -= OnJumpStart;
            _motorMono.JumpPeak -= OnJumpPeak;
            _motorMono.JumpEnd -= OnJumpEnd;

            _saltoTracker.RootKinematicVelocityChanged -= OnKinematicVelocityChanged;
        }

        private void SubscribeCharacterEvents()
        {
            _characterCollisionDetectors.ForEach(cc =>
            {
                cc.MonoPartCollisionEnter += OnMonoPartCollisionEnter;
                cc.MonoPartCollisionExit += OnMonoPartCollisionExit;
            });
        }

        private void UnsubscribeCharacterEvents()
        {
            _characterCollisionDetectors.ForEach(cc =>
            {
                cc.MonoPartCollisionEnter -= OnMonoPartCollisionEnter;
                cc.MonoPartCollisionExit -= OnMonoPartCollisionExit;
            });
        }

        private void SubscribeWheelEvents()
        {
            _baseCollisionDetector.MonoPartCollisionEnter += OnMonoPartCollisionEnter;
            _baseCollisionDetector.MonoPartCollisionExit += OnMonoPartCollisionExit;
            _wheelCollisionDetector.MonoPartCollisionEnter += OnMonoPartCollisionEnter;
            _wheelCollisionDetector.MonoPartCollisionExit += OnMonoPartCollisionExit;
        }

        private void UnsubscribeWheelEvents()
        {
            _baseCollisionDetector.MonoPartCollisionEnter -= OnMonoPartCollisionEnter;
            _baseCollisionDetector.MonoPartCollisionExit -= OnMonoPartCollisionExit;
            _wheelCollisionDetector.MonoPartCollisionEnter -= OnMonoPartCollisionEnter;
            _wheelCollisionDetector.MonoPartCollisionExit -= OnMonoPartCollisionExit;
        }

        private void SubscribeCrashAreasEvents()
        {
            _crashAreas.ForEach(trigger => trigger.Crash += CrashAreaEnterHandle);
        }

        private void UnsubscribeCrashAreaEvents()
        {
            _crashAreas.ForEach(trigger => trigger.Crash -= CrashAreaEnterHandle);
        }
    
        private void CrashAreaEnterHandle()
        {
            if(_isCrashAreaEnterDetecting == false)
                return;

            CrashState = CrashType.CrashAreaEnter;
            CrashStateChanged?.Invoke(CrashState);
            UpdateVelocitiesAfterCrashAreaEnter();
        }


        public void OnChangeCharacterHandle(Character character)
        {
            _characterCollisionDetectorsParent = character.CharacterCollisionDetectorParent;
            _characterCollisionDetectors = _characterCollisionDetectorsParent.GetComponentsInChildren<CollisionDetector>().ToList();
        }

        public void OnChangeWheelHandle(MonoWheel wheel)
        {
            _wheelGroundChecker = wheel.WheelGroundChecker;
        
            _wheelRB = wheel.WheelRigidbody;
            _wheelCollisionDetector = _wheelRB.GetComponent<CollisionDetector>();
            _baseRB = wheel.BaseRigidbody;
            _baseCollisionDetector = _baseRB.GetComponent<CollisionDetector>();
        }

        public void OnChangeReady()
        {
            Enable();
        }

        public void OnChangePreparing()
        {
            Disable();
        }

        private void OnJumpStart(JumpDirection direction)
        {
            _isJumping = true;
        }

        private void OnJumpPeak(float peakHeight)
        {
            SetStartFallPosition();
        }

        private void OnJumpEnd()
        {
            _isJumping = false;
            SetEndFallPosition();
        }

        private void BeginHeightCrashDetect()
        {
            if(_isHeightCrashDetecting)
                return;

            _isHeightCrashDetecting = true;
            _fallHeight = 0;
        
            SetStartFallPosition();
        }

        private void EndHeightCrashDetect()
        {
            if(_isHeightCrashDetecting == false)
                return;

            SetEndFallPosition();
        
            _fallHeight = _startFallPosition.y - _endFallPosition.y;

            _fallCrashProbability = (_fallHeight - ModifiedMinHeight) / (ModifiedMaxHeight - ModifiedMinHeight);
            _fallCrashProbability = Mathf.Clamp(_fallCrashProbability, 0, 1);

            var rnd = Random.Range(0, 1f);
            if(rnd < _fallCrashProbability)
                HeightCrashHandle();
        
            _isHeightCrashDetecting = false;
        }

        private void SetStartFallPosition()
        {
            _startFallPosition = _wheelRB.position;
        }

        private void SetEndFallPosition()
        {
            _endFallPosition = _wheelRB.position;
        }

        private void BeginCollisionCrashDetect()
        {
            _isCollisionCrashDetecting = true;
        }

        private void EndCollisionCrashDetect()
        {
            _isCollisionCrashDetecting = false;
        }
    
        private void BeginOverheatCrashDetect()
        {
            _isOverheatCrashDetecting = true;
        }
        private void EndOverheatCrashDetect()
        {
            _isOverheatCrashDetecting = false;
        }
    
        private void BeginFootMissCrashDetect()
        {
            _isFootMissCrashDetecting = true;
        }
        private void EndFootMissCrashDetect()
        {
            _isFootMissCrashDetecting = false;
        }
    
        private void BeginCrashAreaEnterDetect()
        {
            _isCrashAreaEnterDetecting = true;
            SubscribeCrashAreasEvents();
        }
        private void EndCrashAreaEnterDetect()
        {
            _isCrashAreaEnterDetecting = false;
            UnsubscribeCrashAreaEvents();
        }


        private void HeightCrashHandle()
        {
            if(_isHeightCrashDetecting == false)
                return;
        
            Debug.Log("<color=red>Height Crash Detected!</color>");

            CrashState = CrashType.BigHeightFall;
            _lastCollidedPartsData.Clear();
        
            CrashStateChanged?.Invoke(CrashState);
            //HeightCrashed?.Invoke();
        }
    
        //private void OnMonoPartCollided(Rigidbody2D characterPart, Rigidbody2D obstacleRigidbody,  Vector2 velocity, Vector2 normal)
        private void OnMonoPartCollisionEnter(CollisionDetectorInfo info)
        {
            if (CrashState != CrashType.None)
                return;

            _collidedPartsData.Add(info);

            _collisionCrashProbability = info.AlwaysDefineCollisionsAsCrash == false
                ? (info.TranslateVelocity.magnitude - _settings.MinVelocity) / (_settings.MaxVelocity - _settings.MinVelocity)
                : 1f;
            _collisionCrashProbability = Mathf.Clamp(_collisionCrashProbability, 0, 1f);

            var rnd = Random.Range(0, 1f);
        
            Debug.Log($"<color=yellow>OnMonoPartCollisionEnter : part [{info.PlayerPart}], " +
                                  $"velocity [{info.TranslateVelocity}], angularVelocity [{info.AngularVelocity}], \n" +
                                  $"normal [{info.Normal}], isGrounded [{_motorMono.MonoWheel.WheelGroundChecker.IsGrounded}], " +
                                  $"_collisionCrashProbability {_collisionCrashProbability}, rnd [{rnd}]</color>");

            if (rnd < _collisionCrashProbability || _motorMono.MonoWheel.WheelGroundChecker.IsGrounded == false)
            {
                CollisionCrashHandle();
            }
            else
            {
                var touchDirection = info.Normal.x < 0 ? CollisionTouchDirection.Right : CollisionTouchDirection.Left;

                if (_dangerousTouchedParts.Contains(info.PlayerPart) == false)
                {
                    _dangerousTouchedParts.Add(info.PlayerPart);
                
                    if(_dangerousTouchedParts.Count == 1)
                        DangerousTouched?.Invoke(touchDirection);
                }
            }
        }
    
        private void OnMonoPartCollisionExit(CollisionDetectorInfo info)
        {
            if (CrashState != CrashType.None)
                return;
        
            Debug.Log($"<color=yellow>OnMonoPartCollisionExit : part [{info.PlayerPart}]");
        
            if (_dangerousTouchedParts.Contains(info.PlayerPart))
            {
                _dangerousTouchedParts.Remove(info.PlayerPart);

                if (_dangerousTouchedParts.Count == 0)
                {
                    _dangerousCollisionsDelayTimer.End += OnTimerEnd;
                    _dangerousCollisionsDelayTimer.EnableStopTimer();
                }
            
            }
        }

        private void OnTimerEnd()
        {
            _dangerousCollisionsDelayTimer.End -= OnTimerEnd;
        
            DangerousNonTouched?.Invoke();
        }

        private void OnKinematicVelocityChanged(Vector2 velocity)
        {
            _lastKinematicVelocity = velocity;
        }

        private void CollisionCrashHandle()
        {
            if(_isCollisionCrashDetecting == false)
                return;

            CrashState = CrashType.ObstacleCollision;

            CrashStateChanged?.Invoke(CrashState);
            UpdateVelocitiesAfterCollisionCrash();
        }

        private void UpdateVelocitiesAfterCollisionCrash()
        {
            _lastCollidedPartsData.ForEach(data =>
            {
                _characterCollisionDetectors.ForEach(cc =>
                {
                    cc.UpdatePhysicVelocitiesFromKinematic(data.PlayerPart, data.TranslateVelocity, data.Normal);
                });
        
                _wheelCollisionDetector.UpdatePhysicVelocitiesFromKinematic(data.PlayerPart, data.TranslateVelocity, data.Normal);
                _baseCollisionDetector.UpdatePhysicVelocitiesFromKinematic(data.PlayerPart, data.TranslateVelocity, data.Normal);
            });
        }

        private void UpdateVelocityOverheatDetection()
        {
            if (_isOverheatCrashDetecting == false)
                return;

            var relativeSpeed = _motorMono.CurrentMotorForwardRelativeSpeed;
            var storedWarningState = WarningState;

            if (relativeSpeed < _settings.ThresholdOfRelativePower && storedWarningState != WarningType.None)
            {
                _warningTimer = 0;
            
                WarningState = WarningType.None;
                WarningStateChanged?.Invoke(WarningState);
            }
        
            if (relativeSpeed >= _settings.ThresholdOfRelativePower)
            {
                _warningTimer += Time.deltaTime;
            
                if (storedWarningState == WarningType.None)
                {
                    WarningState = WarningType.First;
                    WarningStateChanged?.Invoke(WarningState);
                    Debug.Log("WarningType.First");
                }
            
                if (storedWarningState == WarningType.First && _warningTimer >= _settings.FirstWarningDuration)
                {
                    WarningState = WarningType.Second;
                    WarningStateChanged?.Invoke(WarningState);
                    Debug.Log("WarningType.Second");
                }
            
                if (storedWarningState == WarningType.Second && _warningTimer >= _settings.FirstWarningDuration + _settings.SecondWarningDuration)
                {
                    WarningState = WarningType.None;
                    WarningStateChanged?.Invoke(WarningState);
                
                    OverheatCrashHandle();
                    Debug.Log("OverheatCrashHandle");
                }
            }
        }
    
        private void UpdatePowerOverheatDetection()
        {
            if (_isOverheatCrashDetecting == false)
                return;

            var relativePower = _motorMono.RelativePower;
            var storedWarningState = WarningState;

            //Debug.Log($"UpdatePowerOverheatDetection : ModifiedOverheatSpeed [{ModifiedOverheatSpeed}]     _warningTimer [{_warningTimer}]");
            if (relativePower >= _settings.ThresholdOfRelativePower)
            {
                _warningTimer += ModifiedOverheatSpeed * Time.deltaTime;
                OverheatValueChanged?.Invoke(Mathf.Clamp(_warningTimer, 0, _settings.FirstWarningDuration) / _settings.FirstWarningDuration);
            
                if (storedWarningState == WarningType.None && _warningTimer >= _settings.FirstWarningDuration)
                {
                    WarningState = WarningType.First;
                    WarningStateChanged?.Invoke(WarningState);
                    Debug.Log("WarningType.First");
                }
            
                if (storedWarningState == WarningType.First && _warningTimer >= _settings.FirstWarningDuration + _settings.SecondWarningDuration)
                {
                    WarningState = WarningType.Second;
                    WarningStateChanged?.Invoke(WarningState);
                    Debug.Log("WarningType.Second");
                }
            
                if (storedWarningState == WarningType.Second && _warningTimer >= _settings.FirstWarningDuration + _settings.SecondWarningDuration * 2f)
                {
                    WarningState = WarningType.None;
                    WarningStateChanged?.Invoke(WarningState);
                
                    OverheatCrashHandle();
                    Debug.Log("OverheatCrashHandle");
                }
            }
            else
            {
                _warningTimer -= _settings.OverheatDecreasingSpeed * Time.deltaTime;
                _warningTimer = Mathf.Clamp(_warningTimer, 0, _settings.FirstWarningDuration);
                OverheatValueChanged?.Invoke(_warningTimer / _settings.FirstWarningDuration);

                if (storedWarningState != WarningType.None)
                {
                    WarningState = WarningType.None;
                    WarningStateChanged?.Invoke(WarningState);
                }
            }
        
        }

        private void OverheatCrashHandle()
        {
            if (_isOverheatCrashDetecting == false)
                return;

            CrashState = CrashType.OverHeat;
            CrashStateChanged?.Invoke(CrashState);
        
            EndOverheatCrashDetect();
            UpdateVelocitiesAfterOverheatCrash();
        }

        private void UpdateVelocitiesAfterOverheatCrash()
        {
            var newCharacterVelocity = _motorMono.FaceDirection * _motorMono.CurrentMotorForwardRelativeSpeed * _settings.CrashCharacterVelocityFactor * Vector2.right + 1.25f * Vector2.up;
            var newBaseVelocity = _motorMono.FaceDirection * _motorMono.CurrentMotorForwardRelativeSpeed * _settings.CrashBaseVelocityFactor * 0.33f * Vector2.left;
            var newWheelVelocity = Vector2.zero;
        
            _characterCollisionDetectors.ForEach(cc =>
            {
                cc.UpdatePhysicVelocitiesFromKinematic(null, newCharacterVelocity);
            });
        
            _wheelCollisionDetector.UpdatePhysicVelocitiesFromKinematic(null, newWheelVelocity);
            _baseCollisionDetector.UpdatePhysicVelocitiesFromKinematic(null, newBaseVelocity);
        }
    
        private void UpdateFootMissDetection()
        {
            if (_isFootMissCrashDetecting == false)
                return;
            
            // some actions for handle miss a foot if it is happened
        }

        private void FootMissCrashHandle()
        {
            if (_isFootMissCrashDetecting == false)
                return;

            CrashState = CrashType.FootMiss;
            CrashStateChanged?.Invoke(CrashState);
        
            UpdateVelocitiesAfterFootMiss();
        }

        private void UpdateVelocitiesAfterFootMiss()
        {
        }
    
        private void UpdateVelocitiesAfterCrashAreaEnter()
        {
            var newCharacterVelocity = _motorMono.MonoWheel.WheelRigidbody.velocity;
            var newBaseVelocity = newCharacterVelocity;
            var newWheelVelocity = newCharacterVelocity;
        
            _characterCollisionDetectors.ForEach(cc =>
            {
                cc.UpdatePhysicVelocitiesFromKinematic(null, newCharacterVelocity);
            });
        
            _wheelCollisionDetector.UpdatePhysicVelocitiesFromKinematic(null, newWheelVelocity);
            _baseCollisionDetector.UpdatePhysicVelocitiesFromKinematic(null, newBaseVelocity);
        }

#if UNITY_EDITOR

        public void FindAreas()
        {
            _crashAreas =  FindObjectsOfType<TriggerCrash>().ToList();
            EditorUtility.SetDirty(this);
        }
#endif
    }
}