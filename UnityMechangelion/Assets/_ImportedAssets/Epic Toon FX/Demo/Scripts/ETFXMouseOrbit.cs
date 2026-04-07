using UnityEngine;
using System.Collections;

namespace EpicToonFX
{

public class ETFXMouseOrbit : MonoBehaviour
{
    public Transform target;
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
    public float distanceMin = .5f;
    public float distanceMax = 15f;
    public float smoothTime = 2f;
    float rotationYAxis = 0.0f;
    float rotationXAxis = 0.0f;
    float velocityX = 0.0f;
    float velocityY = 0.0f;
    // Use this for initialization
    void Start()
    {
        Vector3 angles = this.transform.eulerAngles;
        this.rotationYAxis = angles.y;
        this.rotationXAxis    = angles.x;
        // Make the rigid body not change rotation
        if (this.GetComponent<Rigidbody>())
        {
            this.GetComponent<Rigidbody>().freezeRotation = true;
        }
    }
    void LateUpdate()
    {
        if (this.target)
        {
            if (Input.GetMouseButton(1))
            {
                this.velocityX += this.xSpeed * Input.GetAxis("Mouse X") * this.distance * 0.02f;
                this.velocityY += this.ySpeed * Input.GetAxis("Mouse Y") * 0.02f;
            }
            this.rotationYAxis += this.velocityX;
            this.rotationXAxis -= this.velocityY;
            this.rotationXAxis    =  ClampAngle(this.rotationXAxis, this.yMinLimit, this.yMaxLimit);
            //Quaternion fromRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
            Quaternion toRotation = Quaternion.Euler(this.rotationXAxis, this.rotationYAxis, 0);
            Quaternion rotation   = toRotation;

            this.distance = Mathf.Clamp(this.distance - Input.GetAxis("Mouse ScrollWheel") * 5, this.distanceMin, this.distanceMax);
            RaycastHit hit;
            if (Physics.Linecast(this.target.position, this.transform.position, out hit))
            {
                this.distance -= hit.distance;
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -this.distance);
            Vector3 position    = rotation * negDistance + this.target.position;

            this.transform.rotation = rotation;
            this.transform.position = position;
            this.velocityX          = Mathf.Lerp(this.velocityX, 0, Time.deltaTime * this.smoothTime);
            this.velocityY             = Mathf.Lerp(this.velocityY, 0, Time.deltaTime * this.smoothTime);
        }
    }
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
}