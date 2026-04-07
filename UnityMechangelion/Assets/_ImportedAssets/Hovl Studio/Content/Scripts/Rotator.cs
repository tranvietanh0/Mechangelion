using System.Collections;
using UnityEngine;

public class Rotator : MonoBehaviour
{
	public float x = 0f;
	public float y = 0f;
	public float z = 0f;
	void OnEnable()
    {
		this.InvokeRepeating("Rotate", 0f, 0.0167f);
	}
	void OnDisable()
    {
		this.CancelInvoke();
	}
	void Rotate()
    {
		this.transform.localEulerAngles += new Vector3(this.x, this.y, this.z);
	}
}
