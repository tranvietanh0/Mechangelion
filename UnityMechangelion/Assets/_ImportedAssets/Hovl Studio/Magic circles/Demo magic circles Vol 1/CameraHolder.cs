using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    //camera holder
    public Transform Holder;
    public Vector3 cameraPos = new Vector3(0, 0, 0);
    public float currDistance = 5.0f;
    public float xRotate = 250.0f;
    public float yRotate = 120.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
    public float prevDistance;
    private float x = 0.0f;
    private float y = 0.0f;

    [Header("GUI")]
    private float windowDpi;
    public GameObject[] Prefabs;
    private int Prefab;
    private GameObject Instance;
    private float StartColor;
    private float HueColor;
    public Texture HueTexture;

    void Start()
    {
        if (Screen.dpi < 1) this.windowDpi = 1;
        if (Screen.dpi < 200)
            this.windowDpi = 1;
        else
            this.windowDpi = Screen.dpi / 200f;
        var angles = this.transform.eulerAngles;
        this.x = angles.y;
        this.y    = angles.x;
        this.Counter(0);
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(5 * this.windowDpi, 5 * this.windowDpi, 110 * this.windowDpi, 35 * this.windowDpi), "Previous effect"))
        {
            this.Counter(-1);
        }
        if (GUI.Button(new Rect(120 * this.windowDpi, 5 * this.windowDpi, 110 * this.windowDpi, 35 * this.windowDpi), "Play again"))
        {
            this.Counter(0);
        }
        if (GUI.Button(new Rect(235 * this.windowDpi, 5 * this.windowDpi, 110 * this.windowDpi, 35 * this.windowDpi), "Next effect"))
        {
            this.Counter(+1);
        }

        this.StartColor = this.HueColor;
        this.HueColor      = GUI.HorizontalSlider(new Rect(5 * this.windowDpi, 45 * this.windowDpi, 340 * this.windowDpi, 35 * this.windowDpi), this.HueColor, 0, 1);
        GUI.DrawTexture(new Rect(5 * this.windowDpi, 65 * this.windowDpi, 340 * this.windowDpi, 15 * this.windowDpi), this.HueTexture, ScaleMode.StretchToFill, false, 0);
        if (this.HueColor != this.StartColor)
        {
            int i = 0;
            foreach (var ps in this.particleSystems)
            {
                var main = ps.main;
                Color colorHSV = Color.HSVToRGB(this.HueColor + this.H * 0, this.svList[i].S, this.svList[i].V);
                main.startColor = new Color(colorHSV.r, colorHSV.g, colorHSV.b, this.svList[i].A);
                i++;
            }
        }
    }

    private ParticleSystem[] particleSystems = new ParticleSystem[0];
    private List<SVA> svList = new List<SVA>();
    private float H;

    public struct SVA
    {
        public float S;
        public float V;
        public float A;
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
        if (this.Instance != null)
        {
            Destroy(this.Instance);
        }
        this.Instance        = Instantiate(this.Prefabs[this.Prefab]);
        this.particleSystems = this.Instance.GetComponentsInChildren<ParticleSystem>(); //Get color from current instance 
        this.svList.Clear();
        foreach (var ps in this.particleSystems)
        {
            Color baseColor = ps.main.startColor.color;
            SVA baseSVA = new SVA();
            Color.RGBToHSV(baseColor, out this.H, out baseSVA.S, out baseSVA.V);
            baseSVA.A = baseColor.a;
            this.svList.Add(baseSVA);
        }
    }

    void LateUpdate()
    {
        if (this.currDistance < 2)
        {
            this.currDistance = 2;
        }
        this.currDistance -= Input.GetAxis("Mouse ScrollWheel") * 2;
        if (this.Holder && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
        {
            var pos = Input.mousePosition;
            float dpiScale = 1;
            if (Screen.dpi < 1) dpiScale = 1;
            if (Screen.dpi < 200) dpiScale = 1;
            else dpiScale = Screen.dpi / 200f;
            if (pos.x < 380 * dpiScale && Screen.height - pos.y < 250 * dpiScale) return;
            Cursor.visible   =  false;
            Cursor.lockState =  CursorLockMode.Locked;
            this.x           += (float)(Input.GetAxis("Mouse X") * this.xRotate * 0.02);
            this.y           -= (float)(Input.GetAxis("Mouse Y") * this.yRotate * 0.02);
            this.y              =  ClampAngle(this.y, this.yMinLimit, this.yMaxLimit);
            var rotation = Quaternion.Euler(this.y, this.x, 0);
            var position = rotation * new Vector3(0, 0, -this.currDistance) + this.Holder.position + this.cameraPos;
            this.transform.rotation = rotation;
            this.transform.position    = position;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (this.prevDistance != this.currDistance)
        {
            this.prevDistance = this.currDistance;
            var rot = Quaternion.Euler(this.y, this.x, 0);
            var po  = rot * new Vector3(0, 0, -this.currDistance) + this.Holder.position + this.cameraPos;
            this.transform.rotation = rot;
            this.transform.position    = po;
        }
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
        {
            angle += 360;
        }
        if (angle > 360)
        {
            angle -= 360;
        }
        return Mathf.Clamp(angle, min, max);
    }
}