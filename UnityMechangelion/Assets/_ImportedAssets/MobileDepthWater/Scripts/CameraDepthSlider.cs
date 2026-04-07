namespace Assets.MobileOptimizedWater.Scripts
{
    using UnityEngine;
    using UnityEngine.UI;

    public class CameraDepthSlider : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private Transform cameraTransform;

        [Space]
        [SerializeField] private float minDistance;
        [SerializeField] private float maxDistance;

        [Space]
        [SerializeField] private float scrollDelta;
        [SerializeField] private float scrollSpeed;

        private Vector3 cameraDirectionToRoot;

        private float currentScrollSpeed;
        private float currentValue;

        public void Awake()
        {
            this.cameraDirectionToRoot = this.cameraTransform.localPosition.normalized;

            this.slider.value = 0.2f;
            this.OnSliderValueChanged();
        }

        public void OnSliderValueChanged()
        {
            this.UpdateDepthPosition(this.slider.value);
        }

#if UNITY_EDITOR
        public void Update()
        {
            if (Input.GetKey(KeyCode.W))
            {
                this.currentScrollSpeed = Mathf.Lerp(this.currentScrollSpeed, this.currentScrollSpeed + this.scrollDelta, Time.deltaTime * this.scrollSpeed);
                this.UpdateDepthPosition(Mathf.Lerp(this.currentValue, this.currentValue + this.currentScrollSpeed, Time.deltaTime * this.scrollSpeed));
            }
            else if (Input.GetKey(KeyCode.S))
            {
                this.currentScrollSpeed = Mathf.Lerp(this.currentScrollSpeed, this.currentScrollSpeed + this.scrollDelta, Time.deltaTime * this.scrollSpeed);
                this.UpdateDepthPosition(Mathf.Lerp(this.currentValue, this.currentValue - this.currentScrollSpeed, Time.deltaTime * this.scrollSpeed));
            }

            this.currentScrollSpeed = 0f;
        }
#endif

        private void UpdateDepthPosition(float value)
        {
            this.currentValue                  = Mathf.Clamp(value, 0f, 1f);
            this.cameraTransform.localPosition = this.cameraDirectionToRoot * Mathf.Lerp(this.minDistance, this.maxDistance, this.currentValue);
        }
    }
}
