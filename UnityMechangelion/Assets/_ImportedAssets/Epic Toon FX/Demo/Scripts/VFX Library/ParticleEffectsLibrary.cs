using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleEffectsLibrary : MonoBehaviour {
	public static ParticleEffectsLibrary GlobalAccess;
	void Awake () {
		GlobalAccess = this;

		this.currentActivePEList = new List<Transform> ();

		this.TotalEffects = this.ParticleEffectPrefabs.Length;

		this.CurrentParticleEffectNum = 1;

		// Warn About Lengths of Arrays not matching
		if (this.ParticleEffectSpawnOffsets.Length != this.TotalEffects) {
			Debug.LogError ("ParticleEffectsLibrary-ParticleEffectSpawnOffset: Not all arrays match length, double check counts.");
		}
		if (this.ParticleEffectPrefabs.Length != this.TotalEffects) {
			Debug.LogError ("ParticleEffectsLibrary-ParticleEffectPrefabs: Not all arrays match length, double check counts.");
		}

		// Setup Starting PE Name String
		this.effectNameString = this.ParticleEffectPrefabs [this.CurrentParticleEffectIndex].name + " (" + this.CurrentParticleEffectNum.ToString() + " of " + this.TotalEffects.ToString() + ")";
	}

	// Stores total number of effects in arrays - NOTE: All Arrays must match length.
	public int TotalEffects = 0;
	public int CurrentParticleEffectIndex = 0;
	public int CurrentParticleEffectNum = 0;
//	public string[] ParticleEffectDisplayNames;
	public Vector3[] ParticleEffectSpawnOffsets;
	// How long until Particle Effect is Destroyed - 0 = never
	public float[] ParticleEffectLifetimes;
	public GameObject[] ParticleEffectPrefabs;

	// Storing for deleting if looping particle effect
	#pragma warning disable 414
	private string effectNameString = "";
	#pragma warning disable 414
	private List<Transform> currentActivePEList;

	void Start () {
	}

	public string GetCurrentPENameString() {
		return this.ParticleEffectPrefabs [this.CurrentParticleEffectIndex].name + " (" + this.CurrentParticleEffectNum.ToString() + " of " + this.TotalEffects.ToString() + ")";
	}

	public void PreviousParticleEffect() {
		// Destroy Looping Particle Effects
		if (this.ParticleEffectLifetimes [this.CurrentParticleEffectIndex] == 0) {
			if (this.currentActivePEList.Count > 0) {
				for (int i = 0; i < this.currentActivePEList.Count; i++) {
					if (this.currentActivePEList [i] != null) {
						Destroy (this.currentActivePEList [i].gameObject);
					}
				}
				this.currentActivePEList.Clear ();
			}
		}

		// Select Previous Particle Effect
		if (this.CurrentParticleEffectIndex > 0) {
			this.CurrentParticleEffectIndex -= 1;
		} else {
			this.CurrentParticleEffectIndex = this.TotalEffects - 1;
		}
		this.CurrentParticleEffectNum = this.CurrentParticleEffectIndex + 1;

		// Update PE Name String
		this.effectNameString = this.ParticleEffectPrefabs [this.CurrentParticleEffectIndex].name + " (" + this.CurrentParticleEffectNum.ToString() + " of " + this.TotalEffects.ToString() + ")";
	}
	public void NextParticleEffect() {
		// Destroy Looping Particle Effects
		if (this.ParticleEffectLifetimes [this.CurrentParticleEffectIndex] == 0) {
			if (this.currentActivePEList.Count > 0) {
				for (int i = 0; i < this.currentActivePEList.Count; i++) {
					if (this.currentActivePEList [i] != null) {
						Destroy (this.currentActivePEList [i].gameObject);
					}
				}
				this.currentActivePEList.Clear ();
			}
		}

		// Select Next Particle Effect
		if (this.CurrentParticleEffectIndex < this.TotalEffects - 1) {
			this.CurrentParticleEffectIndex += 1;
		} else {
			this.CurrentParticleEffectIndex = 0;
		}
		this.CurrentParticleEffectNum = this.CurrentParticleEffectIndex + 1;

		// Update PE Name String
		this.effectNameString = this.ParticleEffectPrefabs [this.CurrentParticleEffectIndex].name + " (" + this.CurrentParticleEffectNum.ToString() + " of " + this.TotalEffects.ToString() + ")";
	}

	private Vector3 spawnPosition = Vector3.zero;
	public void SpawnParticleEffect(Vector3 positionInWorldToSpawn) {
		// Spawn Currently Selected Particle Effect
		this.spawnPosition = positionInWorldToSpawn + this.ParticleEffectSpawnOffsets[this.CurrentParticleEffectIndex];
		GameObject newParticleEffect = GameObject.Instantiate(this.ParticleEffectPrefabs[this.CurrentParticleEffectIndex], this.spawnPosition, this.ParticleEffectPrefabs[this.CurrentParticleEffectIndex].transform.rotation) as GameObject;
		newParticleEffect.name = "PE_" + this.ParticleEffectPrefabs[this.CurrentParticleEffectIndex];
		// Store Looping Particle Effects Systems
		if (this.ParticleEffectLifetimes [this.CurrentParticleEffectIndex] == 0) {
			this.currentActivePEList.Add (newParticleEffect.transform);
		}
		this.currentActivePEList.Add(newParticleEffect.transform);
		// Destroy Particle Effect After Lifetime expired
		if (this.ParticleEffectLifetimes [this.CurrentParticleEffectIndex] != 0) {
			Destroy(newParticleEffect, this.ParticleEffectLifetimes[this.CurrentParticleEffectIndex]);
		}
	}
}
