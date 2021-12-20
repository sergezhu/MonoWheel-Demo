using Core.Level.Environment;
using UnityEngine;

namespace Core.Player
{
    public class FromGroundToPointDistanceProvider : MonoBehaviour
    {
        [SerializeField][Range(0.1f, 1f)]
        private float _distanceRequestingInterval = 0.25f;
        [SerializeField][Range(0.1f, 1f)]
        private float _detectionRayLength = 0.5f;
        [SerializeField]
        private bool _accuracyCalculationEnabled = false;

        private Transform _transform;
        private float _time;
        private int _groundLayer;

        public float LastDistance { get; private set; }
        public Vector2 LastProjectionPoint { get; private set; }
        public bool IsGroundDetected { get; private set; }
        public bool IsAnimatedGroundDetected { get; private set; }

        private void Awake()
        {
            _transform = transform;
            _groundLayer = LayerMask.GetMask("Ground");
        }

        private void OnEnable()
        {
            StartDistanceRequesting();
        }

        private void OnDisable()
        {
            StopDistanceRequesting();
        }

        private void Update()
        {
            DoUpdate();
        }

        public void EnableAccuracyCalculation()
        {
            _accuracyCalculationEnabled = true;
        }

        public void DisableAccuracyCalculation()
        {
            _accuracyCalculationEnabled = false;
        }

        private void StartDistanceRequesting()
        {
            _time = 0;
        }
    
        private void StopDistanceRequesting()
        {
        }

        private void DoUpdate()
        {
            _time += Time.deltaTime;
            DoIteration();
        }

        private void DoFixedUpdate()
        {
            _time += Time.fixedDeltaTime;
            DoIteration();
        }

        private void DoIteration()
        {
            if (_time >= _distanceRequestingInterval || _accuracyCalculationEnabled)
            {
                _time = 0;
                IsGroundDetected = TryFindDistance();

#if UNITY_EDITOR
                Debug.DrawLine(_transform.position, _transform.position + (Vector3) (Vector2.down * LastDistance), Color.yellow,
                    _distanceRequestingInterval);
#endif
            }
        }

        private bool TryFindDistance()
        {
            var result = FindCenterProjectionOnGround(out var projectionPoint);
            if (result == false)
                return false;

            LastProjectionPoint = projectionPoint;
            LastDistance = _transform.position.y - LastProjectionPoint.y;
        
            return true;
        }


        private bool FindCenterProjectionOnGround(out Vector2 result)
        {
            result = Vector2.zero;
            var center = _transform.position;
            var hit = Physics2D.Raycast(center, Vector2.down, _detectionRayLength, _groundLayer);

            IsAnimatedGroundDetected = false;

            if (hit.collider is null == false)
            {
                if (hit.collider.TryGetComponent<Ground>(out var ground))
                {
                    IsAnimatedGroundDetected = ground.IsAnimated;
                
                    result = hit.point;
                    return true;
                }
            }

            return false;
        }
    }
}
