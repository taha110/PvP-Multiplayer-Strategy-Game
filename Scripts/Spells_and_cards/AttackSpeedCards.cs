using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackSpeedCards : MonoBehaviour
{

    CardsBase data;
    public string cardName;
    string type;
    string category;
    float attackSpeed;
    float timesDamage;
    float radius;
    float time;

    float startTime;
    List<GameObject> UnitList = new List<GameObject>();
    string mtag;


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
        attackSpeed = data.PerctAttackSpeed;
        timesDamage = data.TimesDamage;
        radius = data.Radius;
        time = data.Time;

        this.transform.parent.localScale = new Vector3(radius, radius, radius);

        startTime = Time.time;
        UnitList = new List<GameObject>();

        if (this.transform.parent.GetComponent<NetPlayerCheck>().IsPlayer1)
        {
            mtag = "Enemy";
        }
        else
        {
            mtag = "Player";
        }

        if (cardName.Contains("AttackingGamble"))
        {
            timesDamage = data.getRandom(data.PerctDamage, data.PerctDamageMax);
            time = data.getRandom(2, data.Time);
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains(mtag) && !other.tag.Contains("Base"))
        {
            UnitList.Add(other.gameObject);
            Work(other.gameObject, true);
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (UnitList.Contains(other.gameObject))
        {
            UnitList.Remove(other.gameObject);
            Work(other.gameObject, false);

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > startTime + time)
        {
            foreach (GameObject _unit in UnitList.ToArray())
            {
                //print(enemy.name);
                if (_unit == null)
                {
                    UnitList.Remove(_unit);
                }
                else {
                    Work(_unit, false);
                }
            }
            if (UnitList != null)
            {
                UnitList.Clear();
            }

            Destroy(this.transform.parent.gameObject);
        }

    }

    void Work(GameObject _unit, bool add)
    {
        data.increaseAttackSpeed(_unit, attackSpeed, add);
        data.increaseDamage(_unit, timesDamage, add);
    }
}
