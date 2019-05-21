using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HeroStats : NetworkBehaviour
{
    public string Name;
    [SyncVar]
    public float hitPoint;
    [SyncVar]
    public float damage;
    [SyncVar]
    public double attackSpeed;
    [SyncVar]
    public double movementSpeed;
    public float magicalResistance;
    public float aura;
    public float physicalResistance;
    [SyncVar]
    public float uniqueChance;


    public float range;

    private float lineOfSight = 5;

    public string[] slots = new string[5];

    private float totalHP;
    private float totalMS;

    public float heroBaseDamage;

    public Slider hpSlider;

    public double MovementSpeed
    {
        get
        {
            return movementSpeed;
        }

        set
        {
            movementSpeed = value;
        }
    }
    public float HitPoints
    {
        get
        {
            return hitPoint;
        }

        set
        {
            hitPoint = value;
        }
    }
    public float Damage
    {
        get
        {
            return damage;
        }

        set
        {
            damage = value;
        }
    }
    public double AttackSpeed
    {
        get
        {
            return attackSpeed;
        }

        set
        {
            attackSpeed = value;
        }
    }
    public float Range
    {
        get
        {
            return range;
        }

        set
        {
            range = value;
        }
    }
    public float Aura
    {
        get
        {
            return aura;
        }

        set
        {
            aura = value;
        }
    }
    public float MagicalResistance
    {
        get
        {
            return magicalResistance;
        }

        set
        {
            magicalResistance = value;
        }
    }
    public float PhysicalResistance
    {
        get
        {
            return physicalResistance;
        }

        set
        {
            physicalResistance = value;
        }
    }
    public float UniqueChance
    {
        get
        {
            return uniqueChance;
        }

        set
        {
            uniqueChance = value;
        }
    }
    public string[] Slots
    {
        get
        {
            return slots;
        }

        set
        {
            slots = value;
        }
    }
    public float LineOfSight
    {
        get
        {
            return lineOfSight;
        }

        set
        {
            lineOfSight = value;
        }
    }

    [SyncVar]
    public bool over = false;
    public bool played = false;

    [SyncVar]
    public float ExtraDamage = 0;
    [SyncVar]
    internal float perctExtraDamage = 1;


    public float randomChance = 1f;
    [SyncVar]
    public float criticalHitPerct = 1f;  //should be SyncVar
    public float unitRandom = 0;
    public float perctMiss = 0;
    public float missRandom = 1;

    DataLoad load;

    int key = 0;

    public bool onHost;


    public delegate void HeroDied();
    public static event HeroDied EventHeroDied;

    public float getTotalHealth()
    {
        return totalHP;
    }

    public float getTotalMoveSpeed()
    {
        return totalMS;
    }


    // Use this for initialization
    void Start()
    {

        load = GameObject.Find("Data").GetComponent<DataLoad>();

        if (this.GetComponent<NetworkIdentity>().isServer && this.tag.Contains("Enemy"))
        {
            sendRpcSlots(slots, true);
        }
        else if (!this.GetComponent<NetworkIdentity>().isServer && this.tag.Contains("Player"))
        {
            sendCmdSlots(slots, true);
        }

        hitPoint = 150;

        onHost = this.GetComponent<NetworkIdentity>().isServer;
        //StartCoroutine(FillAndUpdateData());

    }

    IEnumerator FillAndUpdateData()
    {

        yield return new WaitForSeconds(0.5f);

        over = false;
        played = true;

        FillBaseData();
        heroBaseDamage = damage;
        UpdateStats();

        AddWeapon();
        AddOffhand();

        totalHP = hitPoint;
        totalMS = (float)movementSpeed;

        this.GetComponent<NavMeshAgent>().speed = (float)movementSpeed;

        this.transform.FindChild("AuraDisplay").gameObject.AddComponent<DisplayHeroAura>().enabled = true;
        this.gameObject.AddComponent<HeroFollow>().enabled = true;
    }

    [Client]
    void sendCmdSlots(string[] slots, bool _over)
    {
        CmdSyncSlots(slots, true);
    }

    [Server]
    void sendRpcSlots(string[] slots, bool _over)
    {
        RpcSyncSlots(slots, true);
    }

    [ClientRpc]
    void RpcSyncSlots(string[] slots, bool _over)
    {
        this.Slots = slots;
        over = true;
    }

    [Command]
    void CmdSyncSlots(string[] slots, bool _over)
    {
        this.Slots = slots;
        over = true;
    }

    // Update is called once per frame
    void Update()
    {

        if (over && !played)
        {

            over = false;
            played = true;

            StartCoroutine(FillAndUpdateData());

        }

        this.GetComponent<NavMeshAgent>().speed = (float)movementSpeed;

        if (hitPoint <= 0)
        {
            if (this.GetComponent<NetworkIdentity>().isServer && this.tag.Contains("Enemy"))
            {
                EventHeroDied();
            }
            else if (!this.GetComponent<NetworkIdentity>().isServer && this.tag.Contains("Player"))
            {
                EventHeroDied();
            }
            Destroy(this.gameObject, 0.3f);

        }
    }

    void FillBaseData()
    {
        Name = load.heroData[key]["Properties"][HeroAttributes.name.GetHashCode()];
        hitPoint = load.heroData[key]["Properties"][HeroAttributes.hp.GetHashCode()].AsFloat;
        damage = load.heroData[key]["Properties"][HeroAttributes.damage.GetHashCode()].AsFloat;
        attackSpeed = load.heroData[key]["Properties"][HeroAttributes.attackSpeed.GetHashCode()].AsFloat;
        movementSpeed = load.heroData[key]["Properties"][HeroAttributes.movementSpeed.GetHashCode()].AsFloat;
        magicalResistance = load.heroData[key]["Properties"][HeroAttributes.magicalResistance.GetHashCode()].AsFloat;
        aura = load.heroData[key]["Properties"][HeroAttributes.auraSize.GetHashCode()].AsFloat;
        physicalResistance = load.heroData[key]["Properties"][HeroAttributes.physicalResistance.GetHashCode()].AsFloat;
        uniqueChance = load.heroData[key]["Properties"][HeroAttributes.uniqueChance.GetHashCode()].AsFloat;
    }

    void UpdateStats()
    {
        float tempAS = 0;
        float tempMS = 0;
        float WeaponUC = load.heroData[slots[HeroSlots.weapon.GetHashCode()]][HeroSlots.weapon.GetHashCode()][HeroAttributes.uniqueChance.GetHashCode()].AsFloat;
        uniqueChance = WeaponUC;
        for (int i = 0; i < slots.Length; i++)
        {
            hitPoint += load.heroData[slots[i]][i][HeroAttributes.hp.GetHashCode()].AsFloat;
            damage += load.heroData[slots[i]][i][HeroAttributes.damage.GetHashCode()].AsFloat;
            tempAS += load.heroData[slots[i]][i][HeroAttributes.attackSpeed.GetHashCode()].AsFloat;
            tempMS += load.heroData[slots[i]][i][HeroAttributes.movementSpeed.GetHashCode()].AsFloat;
            magicalResistance += load.heroData[slots[i]][i][HeroAttributes.magicalResistance.GetHashCode()].AsFloat / 100;
            aura += load.heroData[slots[i]][i][HeroAttributes.auraSize.GetHashCode()].AsFloat;
            physicalResistance += load.heroData[slots[i]][i][HeroAttributes.physicalResistance.GetHashCode()].AsFloat / 100;
            if (i != HeroSlots.weapon.GetHashCode())
            {
                uniqueChance += WeaponUC * load.heroData[slots[i]][i][HeroAttributes.uniqueChance.GetHashCode()].AsFloat;
            }
        }

        attackSpeed = attackSpeed * (1 + (tempAS / 100));
        movementSpeed = movementSpeed * (1 + (tempMS / 100));
    }

    void AddWeapon()
    {
        //this.gameObject.AddComponent<UnitStats>().enabled = true;
        this.transform.FindChild("arrow parent").gameObject.AddComponent<HeroMove>().enabled = true;
        switch (slots[HeroSlots.weapon.GetHashCode()])
        {
            case "Assassin":
                this.transform.FindChild("arrow parent").gameObject.AddComponent<HeroWeaponAssassin>().enabled = true;
                break;
            case "Marauder":
                this.transform.FindChild("arrow parent").gameObject.AddComponent<HeroWeaponMarauder>().enabled = true;
                break;
            case "Tactician":
                this.transform.FindChild("arrow parent").gameObject.AddComponent<HeroWeaponTactician>().enabled = true;
                break;
            case "Fearmongerer":
                this.transform.FindChild("arrow parent").gameObject.AddComponent<HeroWeaponFearmongerer>().enabled = true;
                break;
            case "Bludgeoner":
                this.transform.FindChild("arrow parent").gameObject.AddComponent<HeroWeaponBludgeoner>().enabled = true;
                break;
            case "Bloodletter":
                this.transform.FindChild("arrow parent").gameObject.AddComponent<HeroWeaponBloodletter>().enabled = true;
                break;
            case "Slayer":
                this.transform.FindChild("arrow parent").gameObject.AddComponent<HeroWeaponSlayer>().enabled = true;
                break;
            case "Frontliner":
                this.transform.FindChild("arrow parent").gameObject.AddComponent<HeroWeaponFrontliner>().enabled = true;
                break;
            case "Arsonist":
                this.transform.FindChild("arrow parent").gameObject.AddComponent<HeroWeaponArsonist>().enabled = true;
                break;
            case "Damned":
                this.transform.FindChild("arrow parent").gameObject.AddComponent<HeroWeaponDamned>().enabled = true;
                break;
            case "Nightingale":
                this.transform.FindChild("arrow parent").gameObject.AddComponent<HeroWeaponNightingale>().enabled = true;
                break;
            case "Absorber":
                this.transform.FindChild("arrow parent").gameObject.AddComponent<HeroWeaponAbsorber>().enabled = true;
                break;
            case "Beginner":
                this.transform.FindChild("arrow parent").gameObject.AddComponent<HeroWeaponBeginner>().enabled = true;
                break;
        }

    }

    void AddOffhand()
    {
        //this.gameObject.AddComponent<UnitStats>().enabled = true;
        switch (slots[HeroSlots.offHand.GetHashCode()])
        {
            case "Assassin":
                this.gameObject.AddComponent<HeroAuraAssassin>().enabled = true;
                break;
            case "Marauder":
                this.gameObject.AddComponent<HeroAuraMarauder>().enabled = true;
                break;
            case "Tactician":
                this.gameObject.AddComponent<HeroAuraTactician>().enabled = true;
                break;
            case "Fearmongerer":
                this.gameObject.AddComponent<HeroAuraFearmongerer>().enabled = true;
                break;
            case "Bludgeoner":
                //this.gameObject.AddComponent<HeroAuraBludgenore>().enabled = true;
                break;
            case "Bloodletter":
                this.gameObject.AddComponent<HeroAuraBloodletter>().enabled = true;
                break;
            case "Slayer":
                this.gameObject.AddComponent<HeroAuraSlayer>().enabled = true;
                break;
            case "Frontliner":
                this.gameObject.AddComponent<HeroAuraFrontliner>().enabled = true;
                break;
            case "Arsonist":
                this.gameObject.AddComponent<HeroAuraArsonist>().enabled = true;
                break;
            case "Damned":
                this.gameObject.AddComponent<HeroAuraDamned>().enabled = true;
                break;
            case "Nightingale":
                this.gameObject.AddComponent<HeroAuraNightingale>().enabled = true;
                break;
            case "Absorber":
                this.gameObject.AddComponent<HeroAuraAbsorber>().enabled = true;
                break;
            case "Beginner":
                this.gameObject.AddComponent<HeroAuraBeginner>().enabled = true;
                break;
        }

    }

}