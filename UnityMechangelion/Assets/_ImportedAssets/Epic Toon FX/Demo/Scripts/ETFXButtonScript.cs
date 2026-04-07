using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace EpicToonFX
{

public class ETFXButtonScript : MonoBehaviour
{
	public GameObject Button;
	Text MyButtonText;
	string projectileParticleName;		// The variable to update the text component of the button

	ETFXFireProjectile effectScript;		// A variable used to access the list of projectiles
	ETFXProjectileScript projectileScript;

	public float buttonsX;
	public float buttonsY;
	public float buttonsSizeX;
	public float buttonsSizeY;
	public float buttonsDistance;
	
	void Start ()
	{
		this.effectScript = GameObject.Find("ETFXFireProjectile").GetComponent<ETFXFireProjectile>();
		this.getProjectileNames();
		this.MyButtonText      = this.Button.transform.Find("Text").GetComponent<Text>();
		this.MyButtonText.text = this.projectileParticleName;
	}

	void Update ()
	{
		this.MyButtonText.text = this.projectileParticleName;
//		print(projectileParticleName);
	}

	public void getProjectileNames()			// Find and diplay the name of the currently selected projectile
	{
		// Access the currently selected projectile's 'ProjectileScript'
		this.projectileScript       = this.effectScript.projectiles[this.effectScript.currentProjectile].GetComponent<ETFXProjectileScript>();
		this.projectileParticleName = this.projectileScript.projectileParticle.name;	// Assign the name of the currently selected projectile to projectileParticleName
	}

	public bool overButton()		// This function will return either true or false
	{
		Rect button1 = new Rect(this.buttonsX, this.buttonsY, this.buttonsSizeX, this.buttonsSizeY);
		Rect button2 = new Rect(this.buttonsX + this.buttonsDistance, this.buttonsY, this.buttonsSizeX, this.buttonsSizeY);
		
		if(button1.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)) ||
		   button2.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
		{
			return true;
		}
		else
			return false;
	}
}
}