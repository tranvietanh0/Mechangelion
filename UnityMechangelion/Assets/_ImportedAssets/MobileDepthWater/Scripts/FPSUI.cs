namespace Assets.MobileOptimizedWater.Scripts
{
    using Assets.Scripts.Helpers;
    using UnityEngine;
    using UnityEngine.UI;

    public class FPSUI : MonoBehaviour
    {
        [SerializeField] private Text fpsText;

        private FPSCounter fpsCounter;

        public void Awake()
        {
            this.fpsCounter = new FPSCounter();
        }

        public void Update()
        {
            this.fpsCounter.Update(Time.deltaTime);
            this.fpsText.text = "Fps: " + this.fpsCounter.GetAverageFps(1f).ToString("###");
        }
    }
}
