using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using UnityEngine;

public class DemoShooting : MonoBehaviour
{
    public GameObject FirePoint;
    public Camera Cam;
    public float MaxLength;
    public GameObject[] Prefabs;

    private Ray RayMouse;
    private Vector3 direction;
    private Quaternion rotation;

    [Header("GUI")]
    private float windowDpi;
    private int Prefab;
    private GameObject Instance;
    private float hSliderValue = 0.1f;
    private float fireCountdown = 0f;

    //Double-click protection
    private float buttonSaver = 0f;

    //For Camera shake 
    public Animation camAnim;

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
        //Single shoot
        if (Input.GetButtonDown("Fire1"))
        {
            this.camAnim.Play(this.camAnim.clip.name);
            Instantiate(this.Prefabs[this.Prefab], this.FirePoint.transform.position, this.FirePoint.transform.rotation);
        }

        //Fast shooting
        if (Input.GetMouseButton(1) && this.fireCountdown <= 0f)
        {
            Instantiate(this.Prefabs[this.Prefab], this.FirePoint.transform.position, this.FirePoint.transform.rotation);
            this.fireCountdown =  0;
            this.fireCountdown += this.hSliderValue;
        }
        this.fireCountdown -= Time.deltaTime;

        //To change projectiles
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

        //To rotate fire point
        if (this.Cam != null)
        {
            RaycastHit hit;
            var mousePos = Input.mousePosition;
            this.RayMouse = this.Cam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(this.RayMouse.origin, this.RayMouse.direction, out hit, this.MaxLength))
            {
                this.RotateToMouseDirection(this.gameObject, hit.point);
            }
        }
        else
        {
            Debug.Log("No camera");
        }
    }

    //GUI Text
    void OnGUI()
    {
        GUI.Label(new Rect(10 * this.windowDpi, 5 * this.windowDpi, 400 * this.windowDpi, 20 * this.windowDpi), "Use left mouse button to single shoot!");
        GUI.Label(new Rect(10 * this.windowDpi, 25 * this.windowDpi, 400 * this.windowDpi, 20 * this.windowDpi), "Use and hold the right mouse button for quick shooting!");
        GUI.Label(new Rect(10 * this.windowDpi, 45 * this.windowDpi, 400 * this.windowDpi, 20 * this.windowDpi), "Fire rate:");
        this.hSliderValue = GUI.HorizontalSlider(new Rect(70 * this.windowDpi, 50 * this.windowDpi, 100 * this.windowDpi, 20 * this.windowDpi), this.hSliderValue, 0.0f, 1.0f);
        GUI.Label(new Rect(10 * this.windowDpi, 65 * this.windowDpi, 400 * this.windowDpi, 20 * this.windowDpi), "Use the keyboard buttons A/<- and D/-> to change projectiles!");
    }

    // To change prefabs (count - prefab number)
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

    //To rotate fire point
    void RotateToMouseDirection(GameObject obj, Vector3 destination)
    {
        this.direction              = destination - obj.transform.position;
        this.rotation               = Quaternion.LookRotation(this.direction);
        obj.transform.localRotation = Quaternion.Lerp(obj.transform.rotation, this.rotation, 1);
    }
}
