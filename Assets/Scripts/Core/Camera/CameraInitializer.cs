using Cinemachine;
using Core.Level.Environment;
using Core.Player;
using Core.Settings.SettingsSO;
using UnityEngine;

namespace Core.Camera
{
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class CameraInitializer : MonoBehaviour, IChangePlayerDataHandler
    {
        [SerializeField]
        private CameraSettingsSO _settings;
        [SerializeField]
        private LevelStart _levelStart;

        private CinemachineVirtualCamera _camera;
        private Transform _trackedObject;

        private void Awake()
        {
            _camera = GetComponent<CinemachineVirtualCamera>();

            InitCameraValues();
            Subscribe();
        }

        private void Subscribe()
        {
            _levelStart.Started += OnLevelStarted;
        }
    
        private void Unsubscribe()
        {
            _levelStart.Started -= OnLevelStarted;
        }

        private void OnLevelStarted(float time)
        {
            _camera.Follow = _trackedObject;
            Unsubscribe();
        }

        private void InitCameraValues()
        {
            _camera.m_Lens.OrthographicSize = _settings.Size;

            var cinemachineTransposer = _camera.GetCinemachineComponent<CinemachineFramingTransposer>();
        
            cinemachineTransposer.m_XDamping = _settings.Damping.x;
            cinemachineTransposer.m_YDamping = _settings.Damping.y;

            cinemachineTransposer.m_TrackedObjectOffset = _settings.FollowOffset;

            cinemachineTransposer.m_LookaheadTime = _settings.LookAheadTime;
            cinemachineTransposer.m_LookaheadSmoothing = _settings.LookAheadSmoothing;
            cinemachineTransposer.m_ScreenX = _settings.ScreenRelativeOffset.x;
            cinemachineTransposer.m_ScreenY = _settings.ScreenRelativeOffset.y;
            cinemachineTransposer.m_DeadZoneDepth = _settings.DeadZoneDepth;
            cinemachineTransposer.m_DeadZoneWidth = _settings.RelativeDeadZoneSize.x;
            cinemachineTransposer.m_DeadZoneHeight = _settings.RelativeDeadZoneSize.y;
            cinemachineTransposer.m_SoftZoneWidth = _settings.RelativeSoftZoneSize.x;
            cinemachineTransposer.m_SoftZoneHeight = _settings.RelativeSoftZoneSize.y;
            cinemachineTransposer.m_BiasX = _settings.Bias.x;
            cinemachineTransposer.m_BiasY = _settings.Bias.y;
        }

        public void OnChangeWheelHandle(MonoWheel wheel)
        {
        }

        public void OnChangeCharacterHandle(Character character)
        {
            _trackedObject = character.CameraTrackingObject;
        }

        public void OnChangeReady()
        {
        }

        public void OnChangePreparing()
        {
        }
    }
}
