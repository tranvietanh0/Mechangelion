namespace Assets.MobileOptimizedWater.Scripts
{
    using UnityEngine;

    public class AnimationStarter : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Motion animation;

        public void Awake()
        {
            this.animator.Play(this.animation.name);
        }
    }
}
