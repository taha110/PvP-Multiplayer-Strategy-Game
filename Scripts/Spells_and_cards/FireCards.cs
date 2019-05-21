using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class FireCards : NetworkBehaviour
{

    CardsBase data;
    public string cardName;
    string type;
    string category;
    float damage;
    float radius;
    float perctChance;
    float time;

    float startTime;
    List<GameObject> UnitList = new List<GameObject>();
    string mtag;
    bool played = false;
    float interval;
    bool b_staticBubblePlayed;
    [SyncVar]
    int randomUnit = 0;
    [SyncVar]
    float randomDischarge = 0;
    float perctAttackSpeed;
    float perctSpeed;

    public string Type
    {
        get
        {
            return type;
        }

        set
        {
            type = value;
        }
    }
    public string CardName
    {
        get
        {
            return cardName;
        }

        set
        {
            cardName = value;
        }
    }
    public string Category
    {
        get
        {
            return category;
        }

        set
        {
            category = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        cardName = this.transform.parent.gameObject.name.Substring(0, this.transform.parent.gameObject.name.Length - 7);
        data = new CardsBase(cardName);
        type = data.Type;
        category = data.Category;
        damage = data.Damage;
        radius = data.Radius;
        time = data.Time;
        perctAttackSpeed = data.PerctAttackSpeed;
        perctSpeed = data.PerctSpeed;
        perctChance = data.PerctChance;
        b_staticBubblePlayed = false;
        randomUnit = 0;

        this.transform.parent.localScale = new Vector3(radius, radius, radius);

        startTime = Time.time;
        UnitList = new List<GameObject>();


        if (this.transform.parent.GetComponent<NetPlayerCheck>().IsPlayer1)
        {
            mtag = "Player";
        }
        else
        {
            mtag = "Enemy";
        }


        //interval = time - 0.15f;
        if (cardName.Contains("SandStorm"))
        {
            interval = 1;
            time += 0.5f;
        }
        else if (CardName.Contains("Avalanche"))
        {
            interval = 1;
        }
        else
        {
            interval = time / 1.5f;
        }
        

    }

    void OnTriggerEnter(Collider other)
    {

        if (other.tag.Contains(mtag) && !other.tag.Contains("Base"))
        {
            if (CardName.Contains("LightningBolt") || CardName.Contains("BBQfor1"))
            {
                if (UnitList.Count < 1)
                {
                    UnitList.Add(other.gameObject);
                }
            }
            else
            {
                UnitList.Add(other.gameObject);

                if (CardName.Contains("Discharge"))
                {
                    if (this.transform.parent.GetComponent<NetworkIdentity>().isServer)
                    {
                        randomDischarge = data.getRandom(0f, 1f);
                    }
                    if (randomDischarge < perctChance)
                    {
                        workDischarge(other.gameObject, damage);
                    }
                }
                else if (CardName.Contains("ElectricArc"))
                {
                    workElectric(other.gameObject, true);
                }
            }
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (UnitList.Contains(other.gameObject))
        {
            UnitList.Remove(other.gameObject);

            if (CardName.Contains("Discharge"))
            {
                workDischarge(other.gameObject, 0);
            }
            else if (CardName.Contains("ElectricArc"))
            {
                workElectric(other.gameObject, false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!played)
        {
            if (CardName.Contains("ElectricArc") || CardName.Contains("Discharge"))
            {
                played = true;
            }
            else {
                StartCoroutine(Work());
                played = true;
            }
        }
        if (Time.time > startTime + time)
        {
            foreach (GameObject enemy in UnitList)
            {
                if (enemy != null)
                {
                    if (CardName.Contains("Discharge"))
                    {
                        workDischarge(enemy, 0);
                    }
                    else if (CardName.Contains("ElectricArc") || CardName.Contains("Avalanche"))
                    {
                        workElectric(enemy, false);
                    }
                }
            }

            if (UnitList != null)
            {
                UnitList.Clear();
            }

            Destroy(this.transform.parent.gameObject);
        }

    }

    IEnumerator Work()
    {
        yield return new WaitForSeconds(interval);

        LightningStormCheck();
        StaticbubbleCheck();

        foreach (GameObject _unit in UnitList.ToArray())
        {
            //print(enemy.name);
            if (_unit == null)
            {
                UnitList.Remove(_unit);
            }
            else {
                print(_unit.name);
                data.doDirectDamage(_unit, damage);
            }
        }


    }

    void StaticbubbleCheck()
    {
        if (CardName.Contains("StaticBubble"))
        {
            interval = 1;

            if (!b_staticBubblePlayed && Time.time >= startTime + (time / 2))
            {
                damage = damage * 2;
                b_staticBubblePlayed = true;
            }
        }
    }

    void LightningStormCheck()
    {
        if (CardName.Contains("LightningStorm"))
        {
            interval = 2;
            GameObject[] tempEnemies = GameObject.FindGameObjectsWithTag(mtag);

            UnitList.Clear();

            if (tempEnemies.Length > 0)
            {
                if (this.transform.parent.GetComponent<NetworkIdentity>().isServer)
                {
                    randomUnit = data.getRandom(0, tempEnemies.Length);
                }

                UnitList.Add(tempEnemies[randomUnit]);
            }
        }
    }

    void workDischarge(GameObject _unit, float damage)
    {
        _unit.GetComponent<UnitStats>().ExtraDamage = damage;
    }

    void workElectric(GameObject _unit, bool add)
    {
        data.decreaseAttackSpeed(_unit, perctAttackSpeed, add);
        data.decreaseMoveSpeed(_unit, perctSpeed, add);
        if (!CardName.Contains("Avalanche"))
        {
            data.doDirectDamage(_unit, damage);
        }
    }

}
