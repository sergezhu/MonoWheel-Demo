using System;
using Attributes;
using Core.Player.PlayerSO;
using Core.Settings.SettingsSO;
using UnityEngine;

namespace Core.Player
{
    public class FromGroundDistanceCalculator : MonoBehaviour, IChangePlayerDataHandler
    {
        [Space]
        [SerializeField]
        private GroundDistanceControllerSettingsSO _settings;

        [Header("Debug Values")]
        [SerializeField][GUIReadOnly]
        private float _debugFeetAverageDistance;
        [SerializeField][GUIReadOnly]
        private float _debugLastStableBetweenFeetDistance;
    
        private FromGroundToPointDistanceProvider _fromZeroPointToFromGroundToPointDistanceProvider;
        private FromGroundToPointDistanceProvider _fromFootRearPointToFromGroundToPointDistanceProvider;
        private FromGroundToPointDistanceProvider _fromFootMiddlePointToFromGroundToPointDistanceProvider;
        private FromGroundToPointDistanceProvider _fromFootFrontPointToFromGroundToPointDistanceProvider;
    
        private float _lastRelativeBetweenFeetDistance;
        private float _lastStableBetweenFeetDistance;
        private float _feetAverageDistanceMultiplier;
        private float _suspensionDumperReactionMultiplier;

        private MonoWheel _monoWheel;
        private SpringJoint2D _dumperSpringJoint;

        public void Init(MonoWheel monoWheel)
        {
            if (monoWheel == null)
                throw new NullReferenceException();
        
            _monoWheel = monoWheel;
        
            _lastStableBetweenFeetDistance = MotorMono.DefaultStableBetweenFeetDistance;
            _dumperSpringJoint = _monoWheel.DumperSpringJoint;

            _fromZeroPointToFromGroundToPointDistanceProvider = _monoWheel.FromZeroPointToFromGroundToPointDistanceProvider;
            _fromFootRearPointToFromGroundToPointDistanceProvider = _monoWheel.FromFootRearPointToFromGroundToPointDistanceProvider;
            _fromFootMiddlePointToFromGroundToPointDistanceProvider = _monoWheel.FromFootMiddlePointToFromGroundToPointDistanceProvider;
            _fromFootFrontPointToFromGroundToPointDistanceProvider = _monoWheel.FromFootFrontPointToFromGroundToPointDistanceProvider;
        }

        public void EnableDistanceProviders()
        {
            _fromZeroPointToFromGroundToPointDistanceProvider.enabled = true;
            _fromFootRearPointToFromGroundToPointDistanceProvider.enabled = true;
            _fromFootMiddlePointToFromGroundToPointDistanceProvider.enabled = true;
            _fromFootFrontPointToFromGroundToPointDistanceProvider.enabled = true;
        }
        
        public void DisableDistanceProviders()
        {
            _fromZeroPointToFromGroundToPointDistanceProvider.enabled = false;
            _fromFootRearPointToFromGroundToPointDistanceProvider.enabled = false;
            _fromFootMiddlePointToFromGroundToPointDistanceProvider.enabled = false;
            _fromFootFrontPointToFromGroundToPointDistanceProvider.enabled = false;
        }
    
        public void EnableAccuracyCalculation()
        {
            _fromZeroPointToFromGroundToPointDistanceProvider.EnableAccuracyCalculation();
            _fromFootRearPointToFromGroundToPointDistanceProvider.EnableAccuracyCalculation();
            _fromFootMiddlePointToFromGroundToPointDistanceProvider.EnableAccuracyCalculation();
            _fromFootFrontPointToFromGroundToPointDistanceProvider.EnableAccuracyCalculation();
        }

        public void DisableAccuracyCalculation()
        {
            _fromZeroPointToFromGroundToPointDistanceProvider.DisableAccuracyCalculation();
            _fromFootRearPointToFromGroundToPointDistanceProvider.DisableAccuracyCalculation();
            _fromFootMiddlePointToFromGroundToPointDistanceProvider.DisableAccuracyCalculation();
            _fromFootFrontPointToFromGroundToPointDistanceProvider.DisableAccuracyCalculation();
        }

        public void UpdateParameters()
        {
            //var maxSize = _monoWheel.Parameters.GearParameters.SizeBounds.Max;
            var maxSize = WheelGearParameters.SizeBounds.Max;
            //var minSize = _monoWheel.Parameters.GearParameters.SizeBounds.Min;
            var minSize = WheelGearParameters.SizeBounds.Min;
            var currentSize = _monoWheel.Parameters.GearParameters.Size;
            var relativeSize = (currentSize - minSize) / (maxSize - minSize);

            var distMax = _settings._feetAverageDistanceMultiplierBounds.Max;
            var distMin = _settings._feetAverageDistanceMultiplierBounds.Min;
            _feetAverageDistanceMultiplier = distMin + (distMax - distMin) * relativeSize;
        
            //var maxStrength = _monoWheel.Parameters.GearParameters.DumperStrengthBounds.Max;
            var maxStrength = WheelGearParameters.DumperStrengthBounds.Max;
            //var minStrength = _monoWheel.Parameters.GearParameters.DumperStrengthBounds.Min;
            var minStrength = WheelGearParameters.DumperStrengthBounds.Min;
            var currentStrength = _monoWheel.Parameters.GearParameters.DumperStrengthValue;
            var relativeStrength = (currentStrength - minStrength) / (maxStrength - minStrength);
        
            var reactMax = _settings._suspensionDumperReactionMultiplierBounds.Max;
            var reactMin = _settings._suspensionDumperReactionMultiplierBounds.Min;
            _suspensionDumperReactionMultiplier = reactMax - (reactMax - reactMin) * relativeStrength;
        }

        public int GetFromGroundedFootFoundedPointsCount()
        {
            var count = 0;
        
            var providers = new FromGroundToPointDistanceProvider[]
            {
                _fromFootRearPointToFromGroundToPointDistanceProvider,
                _fromFootMiddlePointToFromGroundToPointDistanceProvider,
                _fromFootFrontPointToFromGroundToPointDistanceProvider
            };

            foreach (var provider in providers)
                count += provider.IsGroundDetected ? 1 : 0;

            return count;
        }

        public bool TryGetFromGroundFootAverageDistance(out float result)
        {
            result = 0;
            var sum = 0f;
            var count = 0;

            var providers = new[]
            {
                _fromFootRearPointToFromGroundToPointDistanceProvider,
                _fromFootMiddlePointToFromGroundToPointDistanceProvider,
                _fromFootFrontPointToFromGroundToPointDistanceProvider
            };

            foreach (var provider in providers)
            {
                sum += provider.LastDistance;
                count++;
            }

            if (count == 0)
            {
                Debug.Log("You have not DistanceProviders!");
                return false;
            }

            result = sum / count;
            return true;
        }

        public bool TryGetBetweenFeetDistance(out float result)
        {
            result = 0f;

            if (TryGetFromGroundFootAverageDistance(out var averageDistance))
            {
                result = Mathf.Abs(_fromZeroPointToFromGroundToPointDistanceProvider.LastDistance - averageDistance);
                Debug.Log($"TryGetBetweenFeetDistance : {result}");
                return true;
            }
        
            Debug.Log($"TryGetBetweenFeetDistance : {false}");

            return false;
        }
    
        public float GetFootStepHeightRelative()
        {
            var damperSpringJoint = _dumperSpringJoint;
        
            var reactionDifference = _settings._useReactionOffset && damperSpringJoint is null == false
                ? damperSpringJoint.reactionForce.y - MotorMono.DefaultSuspensionDamperReaction
                : 0;
        
            if (TryGetFromGroundFootAverageDistance(out var feetAverageDistance))
            {
                if (_fromZeroPointToFromGroundToPointDistanceProvider.IsAnimatedGroundDetected == false)
                    _lastStableBetweenFeetDistance = feetAverageDistance;
            
                _debugFeetAverageDistance = feetAverageDistance;
                _debugLastStableBetweenFeetDistance = _lastStableBetweenFeetDistance;

                var feetDistance = _fromZeroPointToFromGroundToPointDistanceProvider.IsAnimatedGroundDetected ? _lastStableBetweenFeetDistance : feetAverageDistance;
                _lastRelativeBetweenFeetDistance = Mathf.Clamp01(feetDistance * _feetAverageDistanceMultiplier - reactionDifference * MotorMono.SuspensionDumperReactionMultiplier * _suspensionDumperReactionMultiplier);
            }
        
            return _lastRelativeBetweenFeetDistance;
        }

        public void OnChangeWheelHandle(MonoWheel wheel)
        {
            Init(wheel);
            UpdateParameters();
        
            //DisableAccuracyCalculation();
            EnableAccuracyCalculation();
        }

        public void OnChangeCharacterHandle(Character character)
        {
        }

        public void OnChangeReady()
        {
        }

        public void OnChangePreparing()
        {
        }
    }
}
