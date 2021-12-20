using System;
using Core.Level.Environment;
using UnityEngine;

namespace Core.Player
{
    public struct CollisionDetectorInfo
    {
        public Rigidbody2D PlayerPart;
        public Rigidbody2D ObstaclePart;
        public Vector2 ContactPosition;
        public Vector2 TranslateVelocity;
        public Vector2 Normal;
        public float AngularVelocity;
        public bool AlwaysDefineCollisionsAsCrash;
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class CollisionDetector : MonoBehaviour
    {
        public event Action<CollisionDetectorInfo> MonoPartCollisionEnter;
        public event Action<CollisionDetectorInfo> MonoPartCollisionExit;

        [SerializeField]
        private bool _crashCollisionsDetectEnable;
        [SerializeField]
        private bool _alwaysDefineCollisionsAsCrash = false;

        [Space]
        [SerializeField]
        private SpriteRenderer _tintSpriteRenderer;
    
        private Rigidbody2D _rb;
        private Transform _transform;
        private SpriteRenderer _spriteRenderer;
        private Vector3 _kinematicVelocity;
        private Vector3 _previousPosition;
        private Vector3 _currentPosition;
        private Vector3 _velocity;

        private float _previousAngle;
        private float _currentAngle;
        private float _angularVelocity;


        private readonly Color _enableDetectingColor = new Color(1f, 1f, 1f, 1f);
        private readonly Color _disableDetectingColor = new Color(1f, .5f, .5f, 1f);

        public Vector2 LastKinematicVelocity => _kinematicVelocity;
        public bool AlwaysDefineCollisionsAsCrash => _alwaysDefineCollisionsAsCrash;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _transform = transform;
        
            _previousPosition = _transform.position;
            _currentPosition = _previousPosition;
        
            _previousAngle = _transform.rotation.eulerAngles.z;
            _currentAngle = _previousAngle;
        }

        private void FixedUpdate()
        {
            _currentPosition = _transform.position;
            _velocity = (_currentPosition - _previousPosition) / Time.fixedDeltaTime;
            _previousPosition = _currentPosition;

            _currentAngle = _transform.rotation.eulerAngles.z;
            _angularVelocity = (_currentAngle - _previousAngle) / Time.fixedDeltaTime;
            _previousAngle = _currentAngle;
        }

        public void EnableCrashCollisionDetection()
        {
            _crashCollisionsDetectEnable = true;
        
#if UNITY_EDITOR
            UpdateTint();
#endif
            //Debug.Log($"EnableCollisionDetection  [{name}] : {_collisionsDetectEnable}");
        }
    
        public void DisableCrashCollisionDetection()
        {
            _crashCollisionsDetectEnable = false;
        
#if UNITY_EDITOR
            UpdateTint();
#endif
        }

        public void UpdatePhysicVelocitiesFromKinematic(Rigidbody2D lastCrashedPart, Vector2 rootVelocity, Vector2 normal)
        {
            var modifier = (lastCrashedPart is null) == false && lastCrashedPart == _rb ? 0.8f : -0.3f;

            var normalizedRootVelocity = rootVelocity.normalized;
            var normalizedMirrorRootVelocity = normalizedRootVelocity - normal * (2f * Vector2.Dot(normalizedRootVelocity, normal));
            _kinematicVelocity = normalizedMirrorRootVelocity * modifier;
            _rb.velocity = _kinematicVelocity;
        }
    
        public void UpdatePhysicVelocitiesFromKinematic(Rigidbody2D lastCrashedPart, Vector2 rootVelocity)
        {
            var modifier = (lastCrashedPart is null) == false && lastCrashedPart == _rb ? -0.3f : 0.8f;
            
            _kinematicVelocity = rootVelocity * modifier;
            _rb.velocity = _kinematicVelocity;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent<Obstacle>(out var obstacle))
            {
                if(_crashCollisionsDetectEnable == false)
                    return;

                var info = new CollisionDetectorInfo()
                {
                    PlayerPart = _rb,
                    ObstaclePart = other.rigidbody,
                    TranslateVelocity = _velocity,
                    AngularVelocity = _angularVelocity,
                    Normal = other.contacts[0].normal,
                    ContactPosition = other.contacts[0].point,
                    AlwaysDefineCollisionsAsCrash = _alwaysDefineCollisionsAsCrash
                };
            
                MonoPartCollisionEnter?.Invoke(info);
            }
        }
    
        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent<Obstacle>(out var obstacle))
            {

                if(_crashCollisionsDetectEnable == false)
                    return;

                var info = new CollisionDetectorInfo()
                {
                    PlayerPart = _rb,
                    ObstaclePart = other.rigidbody,
                    TranslateVelocity = _velocity,
                    AngularVelocity = _angularVelocity,
                    Normal = Vector2.zero,
                    ContactPosition = Vector2.zero,
                    AlwaysDefineCollisionsAsCrash = _alwaysDefineCollisionsAsCrash
                };
            
                MonoPartCollisionExit?.Invoke(info);
            }
        }

        private void UpdateTint()
        {
            //Comment "return" if you need tint
            return;
        
            var color = _crashCollisionsDetectEnable ? _enableDetectingColor : _disableDetectingColor;
        
            if(_tintSpriteRenderer == null)
                Debug.Log($"               UpdateTint FAIL!!!     [{this}]");
            else 
                _tintSpriteRenderer.color = color;
        }
    }
}