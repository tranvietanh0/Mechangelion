using UnityEngine;
using System.Collections;

namespace EpicToonFX
{
    public class ETFXLightFade : MonoBehaviour
    {
        [Header("Seconds to dim the light")]
        public float life = 0.2f;
        public bool killAfterLife = true;

        private Light li;
        private float initIntensity;

        // Use this for initialization
        void Start()
        {
            if (this.gameObject.GetComponent<Light>())
            {
                this.li            = this.gameObject.GetComponent<Light>();
                this.initIntensity = this.li.intensity;
            }
            /*else
                print("No light object found on " + gameObject.name);*/
        }

        // Update is called once per frame
        void Update()
        {
            if (this.gameObject.GetComponent<Light>())
            {
                this.li.intensity -= this.initIntensity * (Time.deltaTime / this.life);
                if (this.killAfterLife && this.li.intensity <= 0)
                    //Destroy(gameObject);
					Destroy(this.gameObject.GetComponent<Light>());
            }
        }
    }
}