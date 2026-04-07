using System.Collections.Generic;
using UnityEngine;

public class ArrowRenderer : MonoBehaviour
{
    public float height        = 0.5f;
    public float segmentLength = 0.5f;
    public float fadeDistanceA = 1.5f;
    public float fadeDistanceB = 1.5f;
    public float speed         = 1f;

    private float alphaMultiplier       = 0f;
    private float alphaMultiplier_speed = 4f;
    private bool  alphaOk               = true;

    //[SerializeField] GameObject arrowPrefab;
    [SerializeField] GameObject segmentPrefab;

    [Space] [SerializeField] Vector3 start;
    [SerializeField]         Vector3 end;
    [SerializeField]         Vector3 upwards = Vector3.up;

    [Space(15)] [SerializeField] private  float     segmentScale = 1f;
    [SerializeField]             internal Transform target;

    internal Transform _transform;

    //Transform arrow;

    private List<Transform>    segments;
    private List<MeshRenderer> renderers;

    private MaterialPropertyBlock mpb;

    private const float END_MAGIC_NUMBER = 2.857143f;

    internal void Init()
    {
        this._transform = this.transform;

        int childCount = this._transform.childCount;

        if (childCount > 0)
        {
            this.segments  = new();
            this.renderers = new();

            foreach (Transform t in this._transform)
            {
                this.segments.Add(t);
                this.renderers.Add(t.GetComponent<MeshRenderer>());
            }
        }
        else
        {
            this.segments  = new List<Transform>(10);
            this.renderers = new List<MeshRenderer>(10);
        }

        this.mpb = new MaterialPropertyBlock();
        this.gameObject.SetActive(false);
    }

    internal void ShowUp()
    {
        this.alphaMultiplier       = 0f;
        this.alphaMultiplier_speed = 4f;
        this.alphaOk               = false;

        if (this.target != null) this.end = (this.target.position - this._transform.position) * END_MAGIC_NUMBER;

        this.gameObject.SetActive(true);
        this.UpdateSegments();
    }

    internal void HideDown()
    {
        if (this.gameObject.activeSelf)
        {
            this.alphaMultiplier_speed = -5f;
            this.alphaOk               = false;
        }
    }

    public void SetPositions(Vector3 start0, Vector3 end0)
    {
        this.start = start0;
        this.end   = end0;
        this.UpdateSegments();
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            RedAlert.SomethingChanged();

            Init();
            ShowUp();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            HideDown();
        }*/

        this.UpdateSegments();
        if (!this.alphaOk)
        {
            this.alphaMultiplier += this.alphaMultiplier_speed * Time.deltaTime;

            if (this.alphaMultiplier_speed > 0f)
            {
                if (this.alphaMultiplier >= 1f)
                {
                    this.alphaMultiplier = 1f;
                    this.alphaOk         = true;
                }
            }
            else
            {
                if (this.alphaMultiplier <= 0f)
                {
                    this.alphaMultiplier = 0f;
                    this.alphaOk         = true;
                    this.gameObject.SetActive(false);
                }
            }
        }
    }

    private void UpdateSegments()
    {
        //Debug.DrawLine(start, end, Color.yellow);

        float distance     = Vector3.Distance(this.start, this.end);
        float radius       = this.height / 2f + distance * distance / (8f * this.height);
        float diff         = radius - this.height;
        float angle        = 2f * Mathf.Acos(diff / radius);
        float length       = angle * radius;
        float segmentAngle = this.segmentLength / radius * Mathf.Rad2Deg;

        Vector3 center = new Vector3(0, -diff, distance / 2f);
        Vector3 left   = Vector3.zero;
        Vector3 right  = new Vector3(0, 0, distance);

        int segmentsCount = (int)(length / this.segmentLength) + 1;

        this.CheckSegments(segmentsCount);

        float offset = Time.time * this.speed * segmentAngle;
        Vector3 firstSegmentPos =
            Quaternion.Euler(Mathf.Repeat(offset, segmentAngle), 0f, 0f) * (left - center) + center;

        float fadeStartDistance = (Quaternion.Euler(segmentAngle / 2f, 0f, 0f) * (left - center) + center).z;

        for (int i = 0; i < segmentsCount; i++)
        {
            Vector3 pos = Quaternion.Euler(segmentAngle * i, 0f, 0f) * (firstSegmentPos - center) + center;
            this.segments[i].localPosition = pos;
            this.segments[i].localRotation = Quaternion.FromToRotation(Vector3.up, pos - center);
            this.segments[i].localScale    = new Vector3(this.segmentScale, this.segmentScale, this.segmentScale);

            MeshRenderer rend = this.renderers[i];

            if (!rend) continue;

            Color currentColor = rend.sharedMaterial.color;
            //currentColor.a = GetAlpha(pos.z - left.z, right.z - fadeDistance - pos.z, fadeStartDistance);
            currentColor.a = GetAlpha(pos.z - this.fadeDistanceA - left.z, right.z - this.fadeDistanceB - pos.z, fadeStartDistance)
                * this.alphaMultiplier;

            this.mpb.SetColor("_Color", currentColor);
            rend.SetPropertyBlock(this.mpb);
            //rend.material.color = currentColor;
        }

        //transform.LookAt(target, MainCamera.inst.transform.position - transform.position/*upwards*/);
        this._transform.LookAt(this.target, this.upwards);
    }

    private void CheckSegments(int segmentsCount)
    {
        while (this.segments.Count < segmentsCount)
        {
            Transform segment = Instantiate(this.segmentPrefab, this._transform).transform;
            this.segments.Add(segment);
            this.renderers.Add(segment.GetComponent<MeshRenderer>());
        }

        for (int i = 0; i < this.segments.Count; i++)
        {
            GameObject segment = this.segments[i].gameObject;
            if (segment.activeSelf != i < segmentsCount) segment.SetActive(i < segmentsCount);
        }
    }

    private static float GetAlpha(float distance0, float distance1, float distanceMax)
    {
        return Mathf.Clamp01(Mathf.Clamp01(distance0 / distanceMax) + Mathf.Clamp01(distance1 / distanceMax) - 1f);
    }
}