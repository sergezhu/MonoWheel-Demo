using UnityEngine;
using UnityEngine.UI;

namespace Core.Data.LevelPreloader
{
    public class PreloaderView : MonoBehaviour, IPreloaderProgressBar
    {
        [SerializeField]
        private Slider _slider;
    
        private void Awake()
        {
            _slider.value = 0;
        }

        public void OnProgressChanged(float value)
        {
            _slider.value = value;
        }
    }
}
