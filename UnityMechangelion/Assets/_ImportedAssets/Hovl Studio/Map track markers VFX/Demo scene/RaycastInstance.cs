using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastInstance : MonoBehaviour
{
    public Camera Cam;
    public GameObject[] Prefabs;
    private int Prefab;
    private Ray RayMouse;
    private GameObject Instance;
    private float windowDpi;

    //Double-click protection
    private float buttonSaver = 0f;

    void Start()
    {
        if (Screen.dpi < 1) this.windowDpi = 1;
        if (Screen.dpi < 200)
            this.windowDpi = 1;
        else
            this.windowDpi = Screen.dpi / 200f;
        this.Counter(0);
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (this.Cam != null)
            {
                RaycastHit hit;
                var mousePos = Input.mousePosition;
                this.RayMouse = this.Cam.ScreenPointToRay(mousePos);
                if (Physics.Raycast(this.RayMouse.origin, this.RayMouse.direction, out hit, 40))
                {
                    this.Instance                 = Instantiate(this.Prefabs[this.Prefab]);
                    this.Instance.transform.position = hit.point + hit.normal * 0.01f;
                    Destroy(this.Instance, 1.5f);
                }
            }
            else
            {
                Debug.Log("No camera");
            }          
        }

        if ((Input.GetKey(KeyCode.A) || Input.GetAxis("Horizontal") < 0) && this.buttonSaver >= 0.4f)// left button
        {
            this.buttonSaver = 0f;
            this.Counter(-1);
        }
        if ((Input.GetKey(KeyCode.D) || Input.GetAxis("Horizontal") > 0) && this.buttonSaver >= 0.4f)// right button
        {
            this.buttonSaver = 0f;
            this.Counter(+1);
        }
        this.buttonSaver += Time.deltaTime;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10 * this.windowDpi, 5 * this.windowDpi, 400 * this.windowDpi, 20 * this.windowDpi), "Use the keyboard buttons A/<- and D/-> to change prefabs!");
        GUI.Label(new Rect(10 * this.windowDpi, 20 * this.windowDpi, 400 * this.windowDpi, 20 * this.windowDpi), "Use left mouse button for instancing!");
    }

    void Counter(int count)
    {
        this.Prefab += count;
        if (this.Prefab > this.Prefabs.Length - 1)
        {
            this.Prefab = 0;
        }
        else if (this.Prefab < 0)
        {
            this.Prefab = this.Prefabs.Length - 1;
        }
    }
}
