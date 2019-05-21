using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyDie : MonoBehaviour
{


    // GameObject us;
    //NavMeshAgent agent;
    float playerAttack;
    float nextShotAttack;

    float initSpeed;
    float percentHP = 0;
    Slider hpSlider;

    Follow follow;

    // Use this for initialization
    void Start()
    {
        // UnitStats us = GetComponent<UnitStats>();
        //agent = GetComponent<NavMeshAgent>();
        // agent.speed = 10;
        percentHP = 100 / this.GetComponent<UnitStats>().hitPoint;
        initSpeed = this.GetComponent<NavMeshAgent>().speed;
        hpSlider = this.transform.Find("Canvas/hpbar/hp").GetComponent<Slider>() as Slider;




    }

    // Update is called once per frame
    void Update()
    {
        hpSlider.value = Mathf.Lerp(hpSlider.value, percentHP* this.GetComponent<UnitStats>().hitPoint, Time.deltaTime * 20);

        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward), Color.cyan);
    }

    
    //OnCollisionEnter
    void OnTriggerEnter(Collider other)
    {
        //print(this.GetComponent<UnitStats>().name);
     /*   RaycastHit hit = new RaycastHit();
        int playerInFront = 0;
        // print(this.GetComponent<UnitStats>().name);
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit))
        {
            playerInFront = hit.collider.gameObject.GetInstanceID();
            //print(playerInFront);
        }
        */

        if (other.tag.Equals("Player"))
        {
            //this.GetComponent<NavMeshAgent>().speed = 0;
            //this.GetComponent<NavMeshAgent>().Stop();
            //other.GetComponent<NavMeshAgent>().Stop();

            nextShotAttack = Time.time + (float)(1 / other.GetComponent<UnitStats>().attackSpeed);
            

            //print("******** OnTriggerEnter : " + this.GetComponent<UnitStats>().name + " , with : " + other.name + " ********");
        }
    }

    //OnCollisionExit
    void OnTriggerExit(Collider other)
    {
       /* //print(this.GetComponent<UnitStats>().name);
        RaycastHit hit = new RaycastHit();
        int playerInFront = 0;
        // print(this.GetComponent<UnitStats>().name);
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit))
        {
            playerInFront = hit.collider.gameObject.GetInstanceID();
            //print(playerInFront);
        }*/

        if (other.tag.Equals("Player"))
        {
            //this.GetComponent<NavMeshAgent>().Resume();

        }

    }

    


    //OnCollisionStay
    void OnTriggerStay(Collider other)
    {
        //print(this.GetComponent<UnitStats>().name);
        /*RaycastHit hit = new RaycastHit();
        int playerInFront = 0;
        // print(this.GetComponent<UnitStats>().name);
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit))
        {
            playerInFront = hit.collider.gameObject.GetInstanceID();
            //print(playerInFront);
        }*/

        if (other.tag.Equals("Player") && this.GetComponent<UnitStats>().range == 1)
        {
            //print("colliding front player");
            Vector3 dir = (other.gameObject.transform.position - gameObject.transform.position).normalized;
            //transform.LookAt(other.transform);

            print("Enemy dir  Z :  " + dir.z);


            if (dir.z > 0 && Time.time > nextShotAttack)
            {

                //print(this.GetComponent<UnitStats>().name + " colliding with " + other.name + " at " + Time.time);

                //print(other.GetComponent<UnitStats>().name + " attack speed " + (1 / other.GetComponent<UnitStats>().attackSpeed));

                nextShotAttack = Time.time + (float)(1 / this.GetComponent<UnitStats>().attackSpeed);

                playerAttack = this.GetComponent<UnitStats>().damage;
                //print(" other name : " + other + "   player attack... : " + playerAttack );


                other.GetComponent<UnitStats>().hitPoint -= playerAttack;

                other.GetComponent<Follow>().showBar = true;
                other.GetComponent<Follow>().InitPopupDamageText(playerAttack.ToString());


                transform.LookAt(other.transform);

                //hpSlider.value -= percentHP * playerAttack;

                //print(this.GetComponent<UnitStats>().name + " stats : hp : " + this.GetComponent<UnitStats>().hitPoint);

                if (other.GetComponent<UnitStats>().hitPoint < 1)
                {
                    //print("Gold Before" + PlayerPrefs.GetInt(GoldManager.Gold));
                    //print("Gold after" + tempGold);
                    //this.GetComponent<NavMeshAgent>().Resume();
                    print(other.GetComponent<UnitStats>().name + " died ");
                    //Destroy(other.gameObject);

                }

            }
        }
    }
}