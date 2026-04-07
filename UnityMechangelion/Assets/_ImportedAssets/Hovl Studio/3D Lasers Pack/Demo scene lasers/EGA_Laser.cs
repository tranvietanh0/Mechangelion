using System.Collections;
using UnityEngine;

public class EGA_Laser : MonoBehaviour
{
    [SerializeField] private int _scaleValue = 0;
    [SerializeField] private float _power = 0.01f;
    [SerializeField] private LayerMask _exceptionLayer;

    public GameObject HitEffect;
    public float HitOffset = 0;

    public float MaxLength;
    private LineRenderer Laser;

    public float MainTextureLength = 1f;
    public float NoiseTextureLength = 1f;
    private Vector4 Length = new Vector4(1, 1, 1, 1);
    private bool LaserSaver = false;
    private bool UpdateSaver = false;

    private ParticleSystem[] Effects;
    private ParticleSystem[] Hit;
    private Coroutine _playCoroutine;

    void Start()
    {
        this.Laser   = this.GetComponent<LineRenderer>();
        this.Effects = this.GetComponentsInChildren<ParticleSystem>();
        this.Hit     = this.HitEffect.GetComponentsInChildren<ParticleSystem>();
    }

    public void Play(float duration)
    {
        this.Stop();
        this.Laser.enabled = true;
        for (int i = 0; i < this.Effects.Length; i++)
        {
            this.Effects[i].Play();
        }
        this._playCoroutine = this.StartCoroutine(this.PlayCoroutine(duration));
    }

    public void Stop()
    {
        if (this._playCoroutine != null)
        {
            this.StopCoroutine(this._playCoroutine);
            this._playCoroutine = null;
        }
        this.Laser.enabled = false;
        for (int i = 0; i < this.Effects.Length; i++)
        {
            this.Effects[i].Stop();
        }
    }

    private IEnumerator PlayCoroutine(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            this.Laser.material.SetTextureScale("_MainTex", new Vector2(this.Length[0], this.Length[1]));
            this.Laser.material.SetTextureScale("_Noise", new Vector2(this.Length[2], this.Length[3]));

            if (this.Laser != null && this.UpdateSaver == false)
            {
                this.Laser.SetPosition(0, this.transform.position);
                RaycastHit hit;
                if (Physics.Raycast(this.transform.position, this.transform.TransformDirection(Vector3.forward), out hit, this.MaxLength, this._exceptionLayer)) //CHANGE THIS IF YOU WANT TO USE LASERRS IN 2D: if (hit.collider != null)
                {
                    this.Laser.SetPosition(1, hit.point);

                    this.HitEffect.transform.position = hit.point + hit.normal * this.HitOffset;
                    this.HitEffect.transform.rotation    = Quaternion.identity;
                    foreach (var AllPs in this.Effects)
                    {
                        if (!AllPs.isPlaying) AllPs.Play();
                    }

                    this.Length[0] = this.MainTextureLength * (Vector3.Distance(this.transform.position, hit.point));
                    this.Length[2] = this.NoiseTextureLength * (Vector3.Distance(this.transform.position, hit.point));
                }
                else
                {
                    var EndPos = this.transform.position + this.transform.forward * this.MaxLength;
                    this.Laser.SetPosition(1, EndPos);
                    this.HitEffect.transform.position = EndPos;
                    foreach (var AllPs in this.Hit)
                    {
                        if (AllPs.isPlaying) AllPs.Stop();
                    }

                    this.Length[0] = this.MainTextureLength * (Vector3.Distance(this.transform.position, EndPos));
                    this.Length[2] = this.NoiseTextureLength * (Vector3.Distance(this.transform.position, EndPos));
                }

                if (this.Laser.enabled == false && this.LaserSaver == false)
                {
                    this.LaserSaver = true;
                    this.Laser.enabled = true;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        this.Stop();
    }

    public void DisablePrepare()
    {
        if (this.Laser != null)
        {
            this.Laser.enabled = false;
        }

        this.UpdateSaver = true;
        if (this.Effects != null)
        {
            foreach (var AllPs in this.Effects)
            {
                if (AllPs.isPlaying) AllPs.Stop();
            }
        }
    }

    public void SetPower(float coeff)
    {
        this._power *= coeff;
    }
}
