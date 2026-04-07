namespace Assets.Scripts.Helpers
{
    using UnityEngine;

    public class FPSCounter
    {
        private const float Frequency = 0.1f;
        private const int DataSize = 100;

        private float[] fpsData;
        private float[] deltaTimeData;

        private float timer;
        private int index;

        private int lastFrameCount;
        private float lastTime;

        public FPSCounter()
        {
            this.index = 0;
            this.timer    = 0f;

            this.fpsData    = new float[DataSize];
            this.deltaTimeData = new float[DataSize];
        }

        public float Fps
        {
            get { return this.fpsData[Mathf.Max(this.index - 1, 0)]; }
        }

        public void Update(float deltaTime)
        {
            this.timer += deltaTime;

            if (this.timer >= Frequency)
            {
                var timeSpan   = Time.realtimeSinceStartup - this.lastTime;
                var frameCount = Time.frameCount - this.lastFrameCount;

                this.fpsData[this.index]       = frameCount / timeSpan;
                this.deltaTimeData[this.index] = this.timer;

                this.index = (this.index + 1) % DataSize;

                this.lastFrameCount = Time.frameCount;
                this.lastTime          = Time.realtimeSinceStartup;

                this.timer = 0f;
            }
        }

        /// <summary>
        /// Return average fps for last N seconds.
        /// </summary>
        /// <param name="forLastSec">Last seconds for calculating.</param>
        /// <returns>Average fps for last N seconds.</returns>
        public float GetAverageFps(float forLastSec)
        {
            var fpsSum = 0f;
            var timeSum = 0f;

            var localIndex     = this.index - 1;
            var totalDataCount = 0;

            while (timeSum <= forLastSec)
            {
                if (localIndex < 0)
                {
                    localIndex += DataSize;
                }

                fpsSum  += this.fpsData[localIndex];
                timeSum += this.deltaTimeData[localIndex];

                localIndex--;
                totalDataCount++;

                if (totalDataCount == DataSize)
                {
                    Debug.LogWarning("Too few data to calculate average fps for such long term.");
                    break;
                }
            }

            return fpsSum / totalDataCount;
        }
    }
}