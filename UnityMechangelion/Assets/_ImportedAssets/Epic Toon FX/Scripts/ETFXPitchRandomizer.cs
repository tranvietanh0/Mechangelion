using UnityEngine;
using System.Collections;

namespace EpicToonFX
{

	public class ETFXPitchRandomizer : MonoBehaviour
	{
	
		public float randomPercent = 10;
	
		void Start ()
		{
			this.transform.GetComponent<AudioSource>().pitch *= 1 + Random.Range(-this.randomPercent / 100, this.randomPercent / 100);
		}
	}
}