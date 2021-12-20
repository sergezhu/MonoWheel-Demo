using System;
using Core.Data.PlayerProgress;
using Core.Player.PlayerSO;
using UnityEngine;

namespace Core.Player.PlayerParameters
{
    public class PlayerParameters : MonoBehaviour, IChangePlayerDataHandler
    {
        [SerializeField]
        [Tooltip("Parameters Scaling depends on difference between base and current values")]
        private MonoWheelParametersSO _baseWheelParameters;
        [SerializeField]
        private CharacterModifiersSO _characterModifiers;

        private CharacterModifiers _currentCharacterModifiers;
        private CharacterModifiersSteps _currentCharacterModifiersSteps;
        private PlayerProgress _currentPlayerProgress;
        private MonoWheel _currentWheel;

        private CharacterModifiers CurrentCharacterModifiers => _currentCharacterModifiers;
        public CharacterModifiersSteps CurrentCharacterModifiersSteps => _currentCharacterModifiersSteps;
        private WheelGearParameters BaseWheelGearParameters => _baseWheelParameters.GearParameters;
        private WheelGearParameters CurrentWheelGearParameters => _currentWheel.Parameters.GearParameters;

        public float DamperStrength => CurrentWheelGearParameters.DumperStrengthValue;
        public float Size => CurrentWheelGearParameters.Size;
        public float Power => CurrentWheelGearParameters.Power;
        public float Weight => CurrentWheelGearParameters.Weight;
        public float TorqueMoment => CurrentWheelGearParameters.TorqueMoment;

        public WheelParametersBounds WheelParametersBounds => new WheelParametersBounds
        {
            PowerBounds = WheelGearParameters.PowerBounds,
            SizeBounds = WheelGearParameters.SizeBounds,
            WeightBounds = WheelGearParameters.WeightBounds,
            DumperStrengthBounds = WheelGearParameters.DumperStrengthBounds
        };


        public void OnChangeWheelHandle(MonoWheel wheel)
        {
            if (wheel == null)
                throw new NullReferenceException();

            _currentWheel = wheel;
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

        public void Init(PlayerProgress progress)
        {
            _currentPlayerProgress = progress;
        
            _characterModifiers.SetPoints(_currentPlayerProgress.CharacterModifiersPoints);
            _currentCharacterModifiers = _characterModifiers.GetCurrentValues();
            _currentCharacterModifiersSteps = _characterModifiers.GetSteps();
        }

        public float GetVelocityAccelerateMultiplier()
        {
            var m = 1f;
            m *= BaseWheelGearParameters.Weight / CurrentWheelGearParameters.Weight;
            m *= CurrentWheelGearParameters.TorqueMoment / BaseWheelGearParameters.TorqueMoment;

            return m;
        }
    
        public float GetTiltAccelerateMultiplier()
        {
            var m = 1f;
            m *= CurrentCharacterModifiers.TiltAccelerateModifier;

            return m;
        }
    
        public float GetMaxForwardVelocityMultiplier()
        {
            var m = 1f;
            m *= CurrentWheelGearParameters.Power / BaseWheelGearParameters.Power;
            m *= CurrentCharacterModifiers.MaxForwardSpeedModifier;
        
            //TODO добавить зависимость макс. скорости от размера колеса и типа поверхности 

            return m;
        }
    
        public float GetMaxBackwardVelocityMultiplier()
        {
            var m = 1f;
            m *= CurrentWheelGearParameters.Power / BaseWheelGearParameters.Power;
            m *= CurrentCharacterModifiers.MaxBackwardSpeedModifier;
        
            //TODO добавить зависимость макс. скорости от размера колеса и типа поверхности 

            return m;
        }
    
        public float GetJumpHeightMultiplier()
        {
            var m = 1f;
            m *= BaseWheelGearParameters.Weight / CurrentWheelGearParameters.Weight;
            m *= CurrentCharacterModifiers.JumpHeightModifier;

            return m;
        }
    
        public float GetMinFallHeightMultiplier()
        {
            var m = 1f;
            m *= BaseWheelGearParameters.DumperStrengthValue / CurrentWheelGearParameters.DumperStrengthValue;
            m *= CurrentCharacterModifiers.StabilityModifier;

            return m;
        }
    
        public float GetMaxFallHeightMultiplier()
        {
            var m = 1f;
            m *= BaseWheelGearParameters.DumperStrengthValue / CurrentWheelGearParameters.DumperStrengthValue;
            m *= CurrentCharacterModifiers.StabilityModifier;

            return m;
        }
    
        public float GetOverloadSpeedMultiplier()
        {
            var m = 1f;
            m *= 1f / CurrentCharacterModifiers.StabilityModifier;

            return m;
        }
    
        public float GetOverloadSpeedAngleMultiplier()
        {
            var m = 1f;
            m *= BaseWheelGearParameters.TorqueMoment / CurrentWheelGearParameters.TorqueMoment;
            m *= m;

            return m;
        }
    
        public float GetSaltoSpeedMultiplier()
        {
            var m = 1f;
            m *= CurrentCharacterModifiers.SaltoSpeedModifier;

            return m;
        }
    }
}