using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HittedObject : MonoBehaviour {

    public float startHealth = 100;
    private float health;
    public Image healthBar;
	// Use this for initialization
	void Start () {
		this.health = this.startHealth;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TakeDamage(float amount)
    {
		this.health               -= amount;
		this.healthBar.fillAmount =  this.health / this.startHealth;
        if(this.health <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
