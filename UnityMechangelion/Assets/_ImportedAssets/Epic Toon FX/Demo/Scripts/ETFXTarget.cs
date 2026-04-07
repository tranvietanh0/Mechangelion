using UnityEngine;
using System.Collections;

namespace EpicToonFX
{

public class ETFXTarget : MonoBehaviour
{
    [Header("Effect shown on target hit")]
	public GameObject hitParticle;
    [Header("Effect shown on target respawn")]
	public GameObject respawnParticle;
	private Renderer targetRenderer;
	private Collider targetCollider;

    void Start()
    {
		this.targetRenderer = this.GetComponent<Renderer>();
		this.targetCollider = this.GetComponent<Collider>();
    }

    void SpawnTarget()
    {
		this.targetRenderer.enabled = true;                                                                                        //Shows the target
		this.targetCollider.enabled = true;                                                                                        //Enables the collider
		GameObject respawnEffect = Instantiate(this.respawnParticle, this.transform.position, this.transform.rotation) as GameObject; //Spawns attached respawn effect
		Destroy(respawnEffect, 3.5f);                                                                                              //Removes attached respawn effect after x seconds
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Missile") // If collider is tagged as missile
        {
            if (this.hitParticle)
            {
				//Debug.Log("Target hit!");
				GameObject destructibleEffect = Instantiate(this.hitParticle, this.transform.position, this.transform.rotation) as GameObject; // Spawns attached hit effect
				Destroy(destructibleEffect, 2f);                                                                                               // Removes hit effect after x seconds
				this.targetRenderer.enabled = false;                                                                                           // Hides the target
				this.targetCollider.enabled = false;                                                                                           // Disables target collider
				this.StartCoroutine(this.Respawn());                                                                                              // Sets timer for respawning the target
            }
        }
    }
	
	IEnumerator Respawn()
    {
        yield return new WaitForSeconds(3);
		this.SpawnTarget();
    }
}
}