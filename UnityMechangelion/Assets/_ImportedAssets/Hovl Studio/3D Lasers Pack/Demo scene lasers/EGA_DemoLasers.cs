using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using UnityEngine;

public class EGA_DemoLasers : MonoBehaviour
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
    private EGA_Laser LaserScript;

    //Double-click protection
    private float buttonSaver = 0f;

    void Start ()
    {
        //LaserEndPoint = new Vector3(0, 0, 0);
        if (Screen.dpi < 1) this.windowDpi = 1;
        if (Screen.dpi < 200)
            this.windowDpi = 1;
        else
            this.windowDpi = Screen.dpi / 200f;
        this.Counter(0);
    }

    void Update()
    {
        //Enable lazer
        if (Input.GetMouseButtonDown(0))
        {
            Destroy(this.Instance);
            this.Instance                  = Instantiate(this.Prefabs[this.Prefab], this.FirePoint.transform.position, this.FirePoint.transform.rotation);
            this.Instance.transform.parent = this.transform;
            this.LaserScript               = this.Instance.GetComponent<EGA_Laser>();
        }

        //Disable lazer prefab
        if (Input.GetMouseButtonUp(0))
        {
            this.LaserScript.DisablePrepare();
            Destroy(this.Instance,1);
        }

        //To change lazers
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
        

        //Current fire point
        if (this.Cam != null)
        {
            RaycastHit hit; //DELATE THIS IF YOU WANT USE LASERS IN 2D
            var mousePos = Input.mousePosition;
            this.RayMouse = this.Cam.ScreenPointToRay(mousePos);
            //ADD THIS IF YOU WANNT TO USE LASERS IN 2D: RaycastHit2D hit = Physics2D.Raycast(RayMouse.origin, RayMouse.direction, MaxLength);
            if (Physics.Raycast(this.RayMouse.origin, this.RayMouse.direction, out hit, this.MaxLength)) //CHANGE THIS IF YOU WANT TO USE LASERRS IN 2D: if (hit.collider != null)
            {
                this.RotateToMouseDirection(this.gameObject, hit.point);
                //LaserEndPoint = hit.point;
            }
            else
            {
                var pos = this.RayMouse.GetPoint(this.MaxLength);
                this.RotateToMouseDirection(this.gameObject, pos);
                //LaserEndPoint = pos;
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
        GUI.Label(new Rect(10 * this.windowDpi, 5 * this.windowDpi, 400 * this.windowDpi, 20 * this.windowDpi), "Use the keyboard buttons A/<- and D/-> to change lazers!");
        GUI.Label(new Rect(10 * this.windowDpi, 20 * this.windowDpi, 400 * this.windowDpi, 20 * this.windowDpi), "Use left mouse button for shooting!");
    }

    //To change prefabs (count - prefab number)
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
    void RotateToMouseDirection (GameObject obj, Vector3 destination)
    {
        this.direction              = destination - obj.transform.position;
        this.rotation               = Quaternion.LookRotation(this.direction);     
        obj.transform.localRotation = Quaternion.Lerp(obj.transform.rotation, this.rotation, 1);
    }
}
