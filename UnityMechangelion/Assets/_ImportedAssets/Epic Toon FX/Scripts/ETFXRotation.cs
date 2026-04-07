using UnityEngine;
using System.Collections;
 
namespace EpicToonFX
{
    public class ETFXRotation : MonoBehaviour
    {
 
        [Header("Rotate axises by degrees per second")]
        public Vector3 rotateVector = Vector3.zero;
 
        public enum spaceEnum { Local, World };
        public spaceEnum rotateSpace;

        public bool rotate = false;
 
        // Update is called once per frame
        void Update()
        {
            if (this.rotate)
            {
                if (this.rotateSpace == spaceEnum.Local) this.transform.Rotate(this.rotateVector * Time.deltaTime);
                if (this.rotateSpace == spaceEnum.World) this.transform.Rotate(this.rotateVector * Time.deltaTime, Space.World);
            }
        }
    }
}