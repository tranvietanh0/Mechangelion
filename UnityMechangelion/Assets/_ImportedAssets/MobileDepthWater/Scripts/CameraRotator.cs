namespace Assets.MobileOptimizedWater.Scripts
{
    using UnityEngine;

    public class CameraRotator : MonoBehaviour
    {
        [SerializeField] private float speed = 5f;

        public void Update()
        {
            var angles = this.transform.eulerAngles;
            angles.y += Time.deltaTime * this.speed;

            this.transform.eulerAngles = angles;
        }
    }
}
