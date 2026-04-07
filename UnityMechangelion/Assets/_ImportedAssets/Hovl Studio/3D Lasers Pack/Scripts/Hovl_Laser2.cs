using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using UnityEngine;

public class Hovl_Laser2 : MonoBehaviour
{
    public float laserScale = 1;
    public Color laserColor = new Vector4(1,1,1,1);
    public GameObject HitEffect;
    public GameObject FlashEffect;
    public float HitOffset = 0;

    public float MaxLength;

    private bool UpdateSaver = false;
    private ParticleSystem laserPS;
    private ParticleSystem[] Flash;
    private ParticleSystem[] Hit;
    private Material laserMat;
    private int particleCount;
    private ParticleSystem.Particle[] particles;
    private Vector3[] particlesPositions;
    private float dissovleTimer = 0;
    private bool startDissovle = false;

    void Start()
    {
        this.laserPS  = this.GetComponent<ParticleSystem>();
        this.laserMat = this.GetComponent<ParticleSystemRenderer>().material;
        this.Flash    = this.FlashEffect.GetComponentsInChildren<ParticleSystem>();
        this.Hit      = this.HitEffect.GetComponentsInChildren<ParticleSystem>();
        this.laserMat.SetFloat("_Scale", this.laserScale);
    }

    void Update()
    {
        if (this.laserPS != null && this.UpdateSaver == false)
        {
            //Set start laser point
            this.laserMat.SetVector("_StartPoint", this.transform.position);
            //Set end laser point
            RaycastHit hit;
            if (Physics.Raycast(this.transform.position, this.transform.TransformDirection(Vector3.forward), out hit, this.MaxLength))
            {
                this.particleCount = Mathf.RoundToInt(hit.distance / (2 * this.laserScale));
                if (this.particleCount < hit.distance / (2 * this.laserScale))
                {
                    this.particleCount += 1;
                }
                this.particlesPositions = new Vector3[this.particleCount];
                this.AddParticles();

                this.laserMat.SetFloat("_Distance", hit.distance);
                this.laserMat.SetVector("_EndPoint", hit.point);
                if (this.Hit != null)
                {
                    this.HitEffect.transform.position = hit.point + hit.normal * this.HitOffset;
                    this.HitEffect.transform.LookAt(hit.point);
                    foreach (var AllHits in this.Hit)
                    {
                        if (!AllHits.isPlaying) AllHits.Play();
                    }
                    foreach (var AllFlashes in this.Flash)
                    {
                        if (!AllFlashes.isPlaying) AllFlashes.Play();
                    }
                }
            }
            else
            {
                //End laser position if doesn't collide with object
                var EndPos   = this.transform.position + this.transform.forward * this.MaxLength;
                var distance = Vector3.Distance(EndPos, this.transform.position);
                this.particleCount = Mathf.RoundToInt(distance / (2 * this.laserScale));
                if (this.particleCount < distance / (2 * this.laserScale))
                {
                    this.particleCount += 1;
                }
                this.particlesPositions = new Vector3[this.particleCount];
                this.AddParticles();

                this.laserMat.SetFloat("_Distance", distance);
                this.laserMat.SetVector("_EndPoint", EndPos);
                if (this.Hit != null)
                {
                    this.HitEffect.transform.position = EndPos;
                    foreach (var AllPs in this.Hit)
                    {
                        if (AllPs.isPlaying) AllPs.Stop();
                    }
                }
            }          
        }

        if (this.startDissovle)
        {
            this.dissovleTimer += Time.deltaTime;
            this.laserMat.SetFloat("_Dissolve", this.dissovleTimer*5);
        }
    }

    void AddParticles()
    {
        //Old particles settings
        /*
        var normalDistance = particleCount;
        var sh = LaserPS.shape;
        sh.radius = normalDistance;
        sh.position = new Vector3(0,0, normalDistance);
        LaserPS.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, particleCount + 1) });
        */

        this.particles = new ParticleSystem.Particle[this.particleCount];

        for (int i = 0; i < this.particleCount; i++)
        {
            this.particlesPositions[i]    = new Vector3(0f, 0f, 0f) + new Vector3(0f, 0f, i * 2 * this.laserScale);
            this.particles[i].position    = this.particlesPositions[i];
            this.particles[i].startSize3D = new Vector3(0.001f, 0.001f, 2 * this.laserScale);
            this.particles[i].startColor  = this.laserColor;
        }
        this.laserPS.SetParticles(this.particles, this.particles.Length);
    }

    public void DisablePrepare()
    {
        this.transform.parent = null;
        this.dissovleTimer    = 0;
        this.startDissovle    = true;
        this.UpdateSaver         = true;
        if (this.Flash != null && this.Hit != null)
        {
            foreach (var AllHits in this.Hit)
            {
                if (AllHits.isPlaying) AllHits.Stop();
            }
            foreach (var AllFlashes in this.Flash)
            {
                if (AllFlashes.isPlaying) AllFlashes.Stop();
            }
        }
    }
}
 