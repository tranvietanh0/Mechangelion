using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EGA_EffectSound : MonoBehaviour
{
    public bool Repeating = true;
    public float RepeatTime = 2.0f;
    public float StartTime = 0.0f;
    public bool RandomVolume;
    public float minVolume = .4f;
    public float maxVolume = 1f;
    private AudioClip clip;

    private AudioSource soundComponent;

    void Start ()
    {
        this.soundComponent = this.GetComponent<AudioSource>();
        this.clip           = this.soundComponent.clip;
        if (this.RandomVolume == true)
        {
            this.soundComponent.volume = Random.Range(this.minVolume, this.maxVolume);
            this.RepeatSound();
        }
        if (this.Repeating == true)
        {
            this.InvokeRepeating("RepeatSound", this.StartTime, this.RepeatTime);
        }
    }

    void RepeatSound()
    {
        this.soundComponent.PlayOneShot(this.clip);
    }
}
