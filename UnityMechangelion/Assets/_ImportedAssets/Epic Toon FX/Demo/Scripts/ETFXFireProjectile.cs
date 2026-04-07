using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace EpicToonFX
{
    public class ETFXFireProjectile : MonoBehaviour
    {
        [SerializeField]
        public GameObject[] projectiles;
        [Header("Missile spawns at attached game object")]
        public Transform spawnPosition;
        [HideInInspector]
        public int currentProjectile = 0;
        public float speed = 500;

        //    MyGUI _GUI;
        ETFXButtonScript selectedProjectileButton;

        void Start()
        {
            this.selectedProjectileButton = GameObject.Find("Button").GetComponent<ETFXButtonScript>();
        }

        RaycastHit hit;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                this.nextEffect();
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                this.nextEffect();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                this.previousEffect();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                this.previousEffect();
            }

            if (Input.GetKeyDown(KeyCode.Mouse0)) //On left mouse down-click
            {
                if (!EventSystem.current.IsPointerOverGameObject()) //Checks if the mouse is not over a UI part
                {
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out this.hit, 100f)) //Finds the point where you click with the mouse
                    {
                        GameObject projectile = Instantiate(this.projectiles[this.currentProjectile], this.spawnPosition.position, Quaternion.identity) as GameObject; //Spawns the selected projectile
                        projectile.transform.LookAt(this.hit.point);                                                                                                   //Sets the projectiles rotation to look at the point clicked
                        projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * this.speed);                                                         //Set the speed of the projectile by applying force to the rigidbody
                    }
                }
            }
            Debug.DrawRay(Camera.main.ScreenPointToRay(Input.mousePosition).origin, Camera.main.ScreenPointToRay(Input.mousePosition).direction * 100, Color.yellow);
        }

        public void nextEffect() //Changes the selected projectile to the next. Used by UI
        {
            if (this.currentProjectile < this.projectiles.Length - 1)
                this.currentProjectile++;
            else
                this.currentProjectile = 0;
            this.selectedProjectileButton.getProjectileNames();
        }

        public void previousEffect() //Changes selected projectile to the previous. Used by UI
        {
            if (this.currentProjectile > 0)
                this.currentProjectile--;
            else
                this.currentProjectile = this.projectiles.Length - 1;
            this.selectedProjectileButton.getProjectileNames();
        }

        public void AdjustSpeed(float newSpeed) //Used by UI to set projectile speed
        {
            this.speed = newSpeed;
        }
    }
}