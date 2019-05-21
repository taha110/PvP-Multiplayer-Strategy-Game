using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class CastleStats : NetworkBehaviour
{

    [SyncVar]
    public float hitPoint;
    [SyncVar]
    public float damage;
    [SyncVar]
    public float attackSpeed;

    public float killGold;
    public float range;

    public bool isAlive = true;

    private float totalHealth;

    public GameObject KillGoldPrefab;


    bool once;

    float delayTimer = 0.25f;

    float percentHP = 0;
    public Slider hpSlider;
    public DataLoad load;


    public delegate void GameEnded();
    [SyncEvent]
    public static event GameEnded EventGameEnded;

    void Awake() {
        hpSlider = this.transform.Find("Canvas/hpbar/hp").GetComponent<Slider>() as Slider;

        totalHealth = hitPoint;
        once = true;
        percentHP = 100 / hitPoint;
    }

    // Use this for initialization
    void Start()
    {

    }

    public float getTotalHealth() {
        return totalHealth;
    }

    // Update is called once per frame
    void Update()
    {
        hpSlider.value = Mathf.Lerp(hpSlider.value, percentHP * hitPoint, Time.deltaTime * 20);

        if (hitPoint <= 0)
        {
            EventGameEnded();
            if (once)
            {
                delayTimer += Time.time;

                GameObject KillGoldTemp = Instantiate(KillGoldPrefab, transform.position, Quaternion.Euler(new Vector3(0, 0, 0))) as GameObject;

                KillGoldTemp.GetComponent<Text>().text = killGold.ToString();
                KillGoldTemp.GetComponent<Animator>().SetTrigger("ShowDamagePopop");
                Destroy(KillGoldTemp.gameObject, 2f);

                isAlive = false;
                once = false;


            }
           
            if (this.name.Contains("Base") || (this.gameObject.tag.Equals("Enemy") && !this.GetComponentInChildren<move>().isShooting()))
            {
                Destroy(this.gameObject);
            }
            else if (this.name.Contains("Base") || (this.gameObject.tag.Equals("Player") && !this.GetComponentInChildren<move>().isShooting()))
            {
                Destroy(this.gameObject);
            }

        }
    }
}
