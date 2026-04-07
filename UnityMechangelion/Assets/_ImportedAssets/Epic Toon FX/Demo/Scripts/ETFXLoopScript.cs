using UnityEngine;
using System.Collections;

namespace EpicToonFX
{
	public class ETFXLoopScript : MonoBehaviour
	{
		public GameObject chosenEffect;
		public float loopTimeLimit = 2.0f;
	
		[Header("Spawn without")]
	
		public bool spawnWithoutLight = true;
		public bool spawnWithoutSound = true;

		void Start ()
		{
			this.PlayEffect();
		}

		public void PlayEffect()
		{
			this.StartCoroutine("EffectLoop");
		}

		IEnumerator EffectLoop()
		{
			GameObject effectPlayer = (GameObject) Instantiate(this.chosenEffect, this.transform.position, this.transform.rotation);
		
			if(this.spawnWithoutLight = true && effectPlayer.GetComponent<Light>())
			{
				effectPlayer.GetComponent<Light>().enabled = false;
				//Destroy(gameObject.GetComponent<Light>());

			}
		
			if(this.spawnWithoutSound = true && effectPlayer.GetComponent<AudioSource>())
			{
				effectPlayer.GetComponent<AudioSource>().enabled = false;
				//Destroy(gameObject.GetComponent<AudioSource>());
			}
				
			yield return new WaitForSeconds(this.loopTimeLimit);

			Destroy (effectPlayer);
			this.PlayEffect();
		}
	}
}