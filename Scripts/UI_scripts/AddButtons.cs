using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.IO;


public class AddButtons : NetworkBehaviour
{

    #region Variables

    CastleStats castle;
    public GameObject gameEnd;


    public int factionNumber;
    public Transform Buttons;
    
    public Button Restart;

    public Image[] images = new Image[6];
    public Button[] buttons = new Button[6];
    public Sprite[] pix = new Sprite[6];
    public string factionName;
    public string[,] unitNames = new string[4, 6]{{"dog","moa","gorilla","rhino","titanoboa","elephant"},
                                                 {"skeleton","zombie","ghost","abomination","shadow","demon"},
                                                 {"fairy","dwarf","elemental","elf","ogre","dragon"},
                                                 {"swordsman","rifleman","artillery","spy","heavy","specops"} };

    public float curTime = 0;
    public BaseUnits units;
    public float gold;
    public Text goldText;
    public DataLoad load;
    public bool once = false;
    public bool timer = false;

    public float unitRandom = 0f;
    public float unitRandom2 = 0f;

    float trainSpeed = 1;
    float costFactor = 1;


    [SyncVar]public string Player1 = "";
    [SyncVar]public string Player2="";
    public string enemy = "";
    public string player = "";

    // for breakers cards ....
    public bool _bool_UnitSpawnDisable = false;


    public float TrainSpeed
    {
        get
        {
            return TrainSpeed;
        }

        set
        {
            TrainSpeed = value;
        }
    }

    public float CostFactor
    {
        get
        {
            return CostFactor;
        }

        set
        {
            CostFactor = value;
        }
    }


    public delegate void LogUnitSpawned(string _name, float GoldRemaining);
    public static event LogUnitSpawned EventLogUnitSpawned;


    //		public delegate void ClickAction(Button b,float time);
    //    	public static event ClickAction onClicked;
    [SyncVar]
    bool spawnHeroes = false;

    #endregion

    #region Network Variables


    [SyncVar]
    public string playerName;
    public NetworkInstanceId playerNetID;



    [SyncVar]
    public int players = 0;

    public delegate void HeroDestChange(bool canChange);
    public static event HeroDestChange EventHeroDestChange;


    public delegate void AddSpawnedUniy();
    public static event AddSpawnedUniy EventAddSpawnedUniy;


    #endregion



    void Awake()
    {


    }

    void Start()
    {
        if (isLocalPlayer)
        {
            goldText = Buttons.transform.Find("Panel/goldplayer").GetComponent<Text>() as Text;
            goldText.gameObject.SetActive(true);
            goldText.text = gold.ToString();
            StartCoroutine(UpdateGold());
        }



    }
    

    public void GetFaction()
    {
        switch (factionName)
        {

            case "Animal":
                factionNumber = 0;
                pix = Resources.LoadAll<Sprite>("Graphics/Units/" + factionName) as Sprite[];
                for (int i = 0; i < images.Length; i++)
                {

                    images[i].sprite = pix[i] as Sprite;
                }

                for (int i = 0; i < buttons.Length; i++)
                {

                    buttons[i].name = unitNames[(int)Factions.Animal, i];
                    buttons[i].GetComponentInChildren<Text>().text = unitNames[(int)Factions.Animal, i];
                }
                break;


            case "Mythical":
                factionNumber = 2;
                pix = Resources.LoadAll<Sprite>("Graphics/Units/" + factionName) as Sprite[];
                for (int i = 0; i < images.Length; i++)
                {

                    images[i].sprite = pix[i] as Sprite;
                }
                for (int i = 0; i < buttons.Length; i++)
                {

                    buttons[i].name = unitNames[(int)Factions.Mythical, i];
                    buttons[i].GetComponentInChildren<Text>().text = unitNames[(int)Factions.Mythical, i];

                }
                break;

            case "Horror":
                factionNumber = 1;
                pix = Resources.LoadAll<Sprite>("Graphics/Units/" + factionName) as Sprite[];
                for (int i = 0; i < images.Length; i++)
                {

                    images[i].sprite = pix[i] as Sprite;
                }
                for (int i = 0; i < buttons.Length; i++)
                {

                    buttons[i].name = unitNames[(int)Factions.Horror, i];
                    buttons[i].GetComponentInChildren<Text>().text = unitNames[(int)Factions.Horror, i];

                }
                break;

            case "Human":
                factionNumber = 3;
                pix = Resources.LoadAll<Sprite>("Graphics/Units/" + factionName) as Sprite[];
                for (int i = 0; i < images.Length; i++)
                {

                    images[i].sprite = pix[i] as Sprite;
                }
                for (int i = 0; i < buttons.Length; i++)
                {

                    buttons[i].name = unitNames[(int)Factions.Human, i];
                    buttons[i].GetComponentInChildren<Text>().text = unitNames[(int)Factions.Human, i];

                }
                break;

            case "Hybrid":
                factionNumber = 4;
                //pix = Resources.LoadAll<Sprite>("Graphics/Units/" + factionName) as Sprite[];

                for (int i = 0; i < images.Length; i++)
                {
                    pix[i] = Resources.Load<Sprite>("Graphics/Units/" + factionName + "/" + GameManager.Instance.hybridArmy[i]) as Sprite;
                }

                for (int i = 0; i < images.Length; i++)
                {
                    images[i].sprite = pix[i] as Sprite;
                }
                for (int i = 0; i < buttons.Length; i++)
                {

                    buttons[i].name = GameManager.Instance.hybridArmy[i];
                    buttons[i].GetComponentInChildren<Text>().text = GameManager.Instance.hybridArmy[i];

                }
                break;

            default:
                break;

        }

    }


    public void AddGold(float goldToAdd)
    {

        if (isLocalPlayer)
        {
            Debug.Log("inGold");
            gold = gold + goldToAdd;
            goldText.text =  gold.ToString();
            Debug.Log("outGold");
        }
    }


    IEnumerator changeDest()
    {
        yield return 0.15f;
        EventHeroDestChange(true);

    }

    public void SpawnUnits(string name, Button b)
    {
        if (!GameManager.Instance._bool_unitsSpawnDisable)
        {
            StartCoroutine(GoSpawn(name, b));
            if (EventAddSpawnedUniy != null)
            {
                EventAddSpawnedUniy();
            }
        }

        EventHeroDestChange(false);
        StartCoroutine(changeDest());


    }

    public float GetUnitRandom()
    {
        return Random.Range(0.0f, 1.0f);
    }

    IEnumerator GoSpawn(string name, Button b)
    {

        float coin;
        float time;
        float curTime = 0;
        Image timerImage;
        timerImage = b.gameObject.transform.FindChild("timer").GetComponent<Image>() as Image;
        time = load.unitData[name][UnitAttributes.TrainingTime.GetHashCode()].AsFloat * trainSpeed;
        coin = load.unitData[name][UnitAttributes.Cost.GetHashCode()].AsFloat * costFactor;
        if (gold >= coin)
        {

            EventLogUnitSpawned(name, gold);
            gold = gold - coin;
            goldText.text = gold.ToString();


            while (curTime <= time)
            {
                yield return 0;
                curTime += Time.deltaTime;
                b.enabled = false;
                b.transform.Find("timer").GetComponent<Image>().enabled = true;
                b.transform.Find("timer").GetComponent<Image>().fillAmount -= Time.deltaTime / time;

            }
            b.transform.Find("timer").GetComponent<Image>().fillAmount = 1;
            b.transform.Find("timer").GetComponent<Image>().enabled = false;
            b.enabled = true;
            CmdSpawn(name);
        }


        yield return 0;

    }

    void OnEnable()
    {
        move.goldAdd += AddGold;
        CardMove.EventSpawnCard += SpawnCardNow;
        EconomicCards.EventAddGoldByCard += AddGold;
        EconomicCards.EventReduceCost += EditCostFactor;
        UnitSpawnCards.EventSpawnUnitByCard += SpawnUnitsByCard;
        DecoyCards.EventSpawnDecoyUnitByCard += SpawnDecoyUnitsByCard;
        WallCards.EventSpawnWallByCard += SpawnWallByCard;
        UnitStats.EventBomberCard += SpawnCardNow;
        ModifierCards.EventChangeTrainSpeed += DivTrainSpeed;
        RespawnCards.EventSpawnRespawnUnitByCard += SpawnRespawnUnitsByCard;
        CastleStats.EventGameEnded += EndGame;
    }


    void OnDisable()
    {
        move.goldAdd -= AddGold;
        CardMove.EventSpawnCard -= SpawnCardNow;
        EconomicCards.EventAddGoldByCard -= AddGold;
        EconomicCards.EventReduceCost -= EditCostFactor;
        UnitSpawnCards.EventSpawnUnitByCard -= SpawnUnitsByCard;
        DecoyCards.EventSpawnDecoyUnitByCard -= SpawnDecoyUnitsByCard;
        WallCards.EventSpawnWallByCard -= SpawnWallByCard;
        UnitStats.EventBomberCard -= SpawnCardNow;
        ModifierCards.EventChangeTrainSpeed -= DivTrainSpeed;
        RespawnCards.EventSpawnRespawnUnitByCard -= SpawnRespawnUnitsByCard;

    }



    public void EndGame() {


        //GameObject.Find("Buttons/EndGame").gameObject.SetActive(true);
        //gameEnd.SetActive(true);
        //StartCoroutine(waitToEnd());
        if (isServer)
        {
            RpcSendEndEvent();
        }
        //Time.timeScale = 0;

    }

    IEnumerator waitToEnd() {
        yield return new WaitForSeconds(0.25f);
        Time.timeScale = 0;
        RpcSendEndEvent();

    }

    [ClientRpc]
    void RpcSendEndEvent() {

        //MultiplayerLobbyManager
        if (isServer)
        {
            gameEnd.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            Buttons = GameObject.Find("Buttons").GetComponent<Transform>() as Transform;
            gameEnd = Buttons.transform.FindChild("EndGame").gameObject;
            gameEnd.SetActive(true);
            Time.timeScale = 0;
        }
        gameEnd.SetActive(true);
        Time.timeScale = 0;
    }

    IEnumerator UpdateGold()
    {
        yield return new WaitForSeconds(1);
        gold = gold + 25f;
        goldText.text =  gold.ToString();
        StartCoroutine(UpdateGold());
    }

    void DivTrainSpeed(float speed, bool add)
    {
        if (isLocalPlayer)
        {
            if (add)
            {
                trainSpeed = trainSpeed / speed;
            }
            else
            {
                trainSpeed = trainSpeed * speed;
            }
        }
    }


    void EditCostFactor(float perctCostReduce, bool add)
    {
        if (isLocalPlayer)
        {
            if (add)
            {
                costFactor = costFactor - perctCostReduce;
            }
            else
            {
                costFactor = costFactor + perctCostReduce;
            }
        }
    }


    void Update()
    {


        if (isLocalPlayer)
        {

            for (int i = 0; i < buttons.Length; i++)
            {

                if (buttons[i].enabled && (Input.GetKeyUp((i + 1).ToString()) || Input.GetKeyUp("[" + (i + 1).ToString() + "]")))
                {
                    SpawnUnits(unitNames[factionNumber, i], buttons[i]);

                }
            }
            //if (Input.GetKeyUp("1") || Input.GetKeyUp("[1]"))
            //{
            //    SpawnUnits(unitNames[factionNumber, 0], buttons[0]);

            //}
            //if (Input.GetKeyUp("2") || Input.GetKeyUp("[2]"))
            //{
            //    SpawnUnits(unitNames[factionNumber, 1], buttons[1]);

            //}
            //if (Input.GetKeyUp("3") || Input.GetKeyUp("[3]"))
            //{
            //    SpawnUnits(unitNames[factionNumber, 2], buttons[2]);

            //}
            //if (Input.GetKeyUp("4") || Input.GetKeyUp("[4]"))
            //{
            //    SpawnUnits(unitNames[factionNumber, 3], buttons[3]);

            //}
            //if (Input.GetKeyUp("5") || Input.GetKeyUp("[5]"))
            //{
            //    SpawnUnits(unitNames[factionNumber, 4], buttons[4]);

            //}
            //if (Input.GetKeyUp("6") || Input.GetKeyUp("[6]"))
            //{
            //    SpawnUnits(unitNames[factionNumber, 5], buttons[5]);

            //}

        }



    }

    public void RestartGame() {

        GameObject.Find("MultiplayerLobbyManager").GetComponent<LobbyManager>().EndGameMethods();
    }


    void FixedUpdate()
    {
        if (spawnHeroes)
        {
            if (isServer)
            {
                StartCoroutine(spawnHero());
                StartCoroutine(spawnBases());
                Player1 = GameManager.Instance.player.userName;
                RpcSendName(Player1);
            }
            else{
                Player2 = GameManager.Instance.player.userName;
                CmdSendName(Player2);
            }
            spawnHeroes = false;
            StartCoroutine(DisplayNames());
        }

    }

    [ClientRpc]
    void RpcSendName(string name)
    {
        Player1 = name;
    }

    [Command]
    void CmdSendName(string name)
    {
        Player2 = name;
    }


    IEnumerator DisplayNames()
    {
        yield return new WaitForSeconds(0.5f);
        Buttons = GameObject.Find("Buttons").GetComponent<Transform>() as Transform;
        Buttons.transform.FindChild("Name_enemy").gameObject.GetComponentInChildren<Text>().text = Player1;
        Buttons.transform.FindChild("Name_player").gameObject.GetComponentInChildren<Text>().text = Player2;


    }

    IEnumerator spawnBases()
    {
        yield return new WaitForSeconds(0.5f);
        CmdSpawnBases();

    }

    [Command]
    void CmdSpawnBases()
    {
        if (hasAuthority)
        {
            GameObject eBase = Instantiate(Resources.Load("EnemyBase")) as GameObject;
            NetworkServer.Spawn(eBase);
            GameObject pBase = Instantiate(Resources.Load("PlayerBase")) as GameObject;
            NetworkServer.Spawn(pBase);
        }
        else
        {
            //GameObject eBase1 = Instantiate(Resources.Load("EnemyBase")) as GameObject;
            //NetworkServer.SpawnWithClientAuthority(eBase1, this.gameObject);
            //GameObject pBase1 = Instantiate(Resources.Load("PlayerBase")) as GameObject;
            //NetworkServer.SpawnWithClientAuthority(pBase1, this.gameObject);
        }
    }


    void LateUpdate()
    {




    }


    //---------------------------------------------Networking-----------------------------//




    [Command]
    void CmdGetPlayerName()
    {

        playerName = "";
        playerNetID = this.GetComponent<NetworkIdentity>().netId;
        playerName = "player " + playerNetID;
        this.transform.name = playerName;

    }

    [Command]
    void CmdAddPlayer()
    {

        players = players + 1;

    }


    public void OnClientSceneChanged(NetworkConnection conn)
    {

        Debug.Log("Connected");

    }


    public override void OnStartClient()
    {
        if (!hasAuthority)
        {

            Debug.Log("cleint");

        }
        spawnHeroes = true;

    }

    [ClientRpc]
    void RpcSpawnHeroHelper()
    {
        spawnHeroes = true;
    }

    [Command]
    void CmdSpawnHeros(string name)
    {
        //GameObject hero = Instantiate(Resources.Load("units/Hero/" + name), new Vector3(100, 0, 62), Quaternion.identity) as GameObject;
        //GameObject hero1 = Instantiate(Resources.Load("units/Hero/" + name), new Vector3(125, 0, 62), Quaternion.identity) as GameObject;

        if (hasAuthority)
        {
            GameObject hero = Instantiate(Resources.Load("units/Hero/" + name), new Vector3(100, 0, 62), Quaternion.identity) as GameObject;
            hero.tag = "EnemyHero";
            hero.GetComponent<HeroNavMesh>().targetTag = "Player";
            hero.GetComponent<Renderer>().material.color = Color.red;
            if (hasAuthority)
            {

                hero.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.green;
            }
            else {
                hero.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;
            }
            NetworkServer.Spawn(hero);
            RpcSendHero(hero);
            CmdSendHero(hero);
        }
        else
        {
            GameObject hero1 = Instantiate(Resources.Load("units/Hero/" + name), new Vector3(125, 0, 62), Quaternion.identity) as GameObject;
            hero1.tag = "PlayerHero";
            hero1.GetComponent<HeroNavMesh>().targetTag = "Enemy";
            hero1.GetComponent<Renderer>().material.color = Color.blue;
            if (hasAuthority)
            {

                hero1.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.green;
            }
            else {
                hero1.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;
            }
            NetworkServer.SpawnWithClientAuthority(hero1, this.gameObject);
            RpcSendHero1(hero1);
            CmdSendHero1(hero1);

        }
        //NetworkServer.SpawnWithClientAuthority(hero, this.gameObject);
        //NetworkServer.SpawnWithClientAuthority(hero1, this.gameObject);

        //RpcSendHeroValsToClient(hero1, hero);

    }

    [ClientRpc]
    void RpcSendHeroValsToClient(GameObject hero1, GameObject hero)
    {

        hero1.tag = "PlayerHero";
        hero1.GetComponent<HeroNavMesh>().targetTag = "Enemy";
        hero1.GetComponent<Renderer>().material.color = Color.blue;

        hero.tag = "EnemyHero";
        hero.GetComponent<HeroNavMesh>().targetTag = "Player";
        hero.GetComponent<Renderer>().material.color = Color.red;
    }

    [ClientRpc]
    void RpcSendHero1(GameObject hero1)
    {

        hero1.tag = "PlayerHero";
        //hero1.GetComponent<HeroNavMesh>().targetTag = "Enemy";
        hero1.GetComponent<Renderer>().material.color = Color.blue;
        if (hasAuthority)
        {

            hero1.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.green;
        }
        else {
            hero1.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;
        }

    }
    [Command]
    void CmdSendHero1(GameObject hero1)
    {

        hero1.tag = "PlayerHero";
        // hero1.GetComponent<HeroNavMesh>().targetTag = "Enemy";
        hero1.GetComponent<Renderer>().material.color = Color.blue;
        if (hasAuthority)
        {

            hero1.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.green;
        }
        else {
            hero1.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;
        }

    }
    [ClientRpc]
    void RpcSendHero(GameObject hero)
    {

        hero.tag = "EnemyHero";
        //hero.GetComponent<HeroNavMesh>().targetTag = "Player";
        hero.GetComponent<Renderer>().material.color = Color.red;
        if (hasAuthority)
        {

            hero.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.green;
        }
        else {
            hero.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;
        }
    }
    [Command]
    void CmdSendHero(GameObject hero)
    {
        hero.tag = "EnemyHero";
        //hero.GetComponent<HeroNavMesh>().targetTag = "Player";
        hero.GetComponent<Renderer>().material.color = Color.red;
        if (hasAuthority)
        {

            hero.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.green;
        }
        else {
            hero.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;
        }

    }




    IEnumerator spawnHero()
    {
        yield return new WaitForSeconds(7f);
        CmdSpawnHeros("Hero1");
    }

    //bool clientConnected = false;
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        CmdGetPlayerName();

        if (isLocalPlayer)
        {

            load = GameObject.Find("Data").GetComponent<DataLoad>();
            gold = 2000;
            Buttons = GameObject.Find("Buttons").GetComponent<Transform>() as Transform;
            //Restart = GameObject.Find("Restart").GetComponent<Button>() as Button;
            //Restart.onClick.AddListener(() => RestartLevel());
            //factionName = PlayerPrefs.GetString("factionName");
            factionName = GameManager.Instance.factionName;
            gameEnd = Buttons.transform.FindChild("EndGame").gameObject;


            if (isServer)
            {
                
                Player1 = GameManager.Instance.player.userName;
               

            }
            else if (!isServer)
            {
                

            }


            for (int i = 0; i < buttons.Length; i++)
            {

                buttons[i] = Buttons.transform.Find("Panel/FactionButtons/unit" + (i + 1)).GetComponent<Button>() as Button;
                images[i] = Buttons.transform.Find("Panel/FactionButtons/unit" + (i + 1)).GetComponent<Image>() as Image;
                Button b = buttons[i];
                b.onClick.AddListener(() => SpawnUnits(b.gameObject.name, b));
                //b.onClick.AddListener(() => StopSpawning(b));
            }
            
            GetFaction();
        }


        //StartCoroutine(spawnHero());
        //if (clientConnected)
        //{
        //    StartCoroutine(spawnHero());
        //    //clientConnected = false;
        //}

    }




    public void RestartLevel()
    {
        NetworkManager.singleton.StopHost();
        NetworkManager.singleton.StopServer();
        NetworkManager.singleton.StopMatchMaker();
        NetworkManager.singleton.StopClient();
        SceneManager.LoadScene("Ui_final");

        Destroy(NetworkManager.singleton.gameObject);
    }




    void SpawnRespawnUnitsByCard(string name)
    {
        if (isLocalPlayer)
        {
            CmdSpawn(name);
        }
    }

    // ============    decoy unit spawn start =================//  




    void SpawnDecoyUnitsByCard(string name)
    {
        if (isLocalPlayer)
        {
            CmdDecoySpawn(name);
        }
    }

    // ============   decoy unit spawn

    [Command]
    public void CmdDecoySpawn(string name)
    {
        Debug.Log("IM INSIDE SPAWN");
        Debug.Log(name);


        if (!hasAuthority)
        {

            GameObject unit;
            unit = Instantiate(Resources.Load("units/" + name, typeof(GameObject)), new Vector3(128f, 0.5f, Random.Range(61f, 68f)), Quaternion.identity) as GameObject;
            unit.gameObject.tag = "Player";
            unit.GetComponent<PlayerNavMesh>().UnitTag = "Enemy";
            unit.GetComponent<PlayerNavMesh>().enabled = true;
            unit.GetComponent<UnitStats>().isDecoy = true;
            //		NetworkServer.Spawn(unit);
            Debug.Log("here");

            unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;

            NetworkServer.Spawn(unit);
            RpcDecoyTurnToEnemyWithoutAuthority(unit);

        }
        else {

            GameObject unit;
            unit = Instantiate(Resources.Load("units/" + name, typeof(GameObject)), new Vector3(97f, 0.5f, Random.Range(61f, 68f)), Quaternion.identity) as GameObject;
            unit.gameObject.tag = "Enemy";
            unit.GetComponent<PlayerNavMesh>().UnitTag = "Player";
            unit.GetComponent<PlayerNavMesh>().enabled = true;
            unit.GetComponent<UnitStats>().isDecoy = true;

            unit.GetComponent<Renderer>().material.color = Color.red;
            // dummy
            if (hasAuthority)
            {
                unit.GetComponent<Renderer>().material.color = Color.cyan;

                unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.green;
            }
            else {
                unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;

            }
            // dummy
            unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;
            NetworkServer.Spawn(unit);
            RpcDecoyTurnToEnemy(unit);
        }
    }

    // ============   decoy unit spawn

    [ClientRpc]
    public void RpcDecoyTurnToEnemy(GameObject unit)
    {

        unit.gameObject.tag = "Enemy";
        unit.GetComponent<PlayerNavMesh>().UnitTag = "Player";
        unit.GetComponent<PlayerNavMesh>().enabled = true;
        unit.GetComponent<UnitStats>().isDecoy = true;

        unit.GetComponent<Renderer>().material.color = Color.red;
        unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;

        if (hasAuthority)
        {
            unit.GetComponent<Renderer>().material.color = Color.cyan;

            unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.green;
        }
        else {
            unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;

        };
        Debug.Log("here");

    }


    // ============   decoy unit spawn

    [ClientRpc]
    public void RpcDecoyTurnToEnemyWithoutAuthority(GameObject unit)
    {
        unit.gameObject.tag = "Player";
        unit.GetComponent<PlayerNavMesh>().UnitTag = "Enemy";
        unit.GetComponent<PlayerNavMesh>().enabled = true;
        unit.GetComponent<UnitStats>().isDecoy = true;


        if (hasAuthority)
        {
            unit.GetComponent<Renderer>().material.color = Color.cyan;

            unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.green;
        }
        else {
            unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;

        };
        Debug.Log("here");

    }

    // ============   decoy unit spawn ends =================//  



    void SpawnUnitsByCard(string name)
    {
        if (isLocalPlayer)
        {
            CmdSpawn(name);
        }
    }

    // ============   wall spawn start =================//  
    void SpawnWallByCard(string name, Vector3 wallPos)
    {
        if (isLocalPlayer)
        {
            CmdWallSpawn(name, wallPos);
        }
    }
    // ============   wall spawn
    [Command]
    public void CmdWallSpawn(string name, Vector3 wallPos)
    {
        Debug.Log("IM INSIDE wall SPAWN");
        Debug.Log(name);


        if (!hasAuthority)
        {
            GameObject wall;
            wall = Instantiate(Resources.Load("units/" + name, typeof(GameObject)), new Vector3(wallPos.x, 0.08333331f, 64f), Quaternion.identity) as GameObject;
            //wall = Instantiate(Resources.Load("units/" + name, typeof(GameObject)), wallPos, Quaternion.identity) as GameObject;
            wall.gameObject.tag = "Player";
            wall.transform.Find("Cube").GetComponent<Renderer>().material.color = Color.gray;

            if (hasAuthority)
            {
                wall.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.green;
            }
            else {
                wall.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;
            }
            NetworkServer.Spawn(wall);

        }
        else {

            GameObject wall;
            wall = Instantiate(Resources.Load("units/" + name, typeof(GameObject)), new Vector3(wallPos.x, 0.08333331f, 64f), Quaternion.identity) as GameObject;
            //wall = Instantiate(Resources.Load("units/" + name, typeof(GameObject)), wallPos, Quaternion.identity) as GameObject;
            wall.gameObject.tag = "Enemy";
            wall.transform.Find("Cube").GetComponent<Renderer>().material.color = Color.gray;
            wall.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;
            NetworkServer.Spawn(wall);
            RpcWallTurnToEnemy(wall);
        }
    }
    // ============   wall spawn
    [ClientRpc]
    public void RpcWallTurnToEnemy(GameObject wall)
    {

        wall.gameObject.tag = "Enemy";
        wall.transform.Find("Cube").GetComponent<Renderer>().material.color = Color.gray;
        wall.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;

        if (hasAuthority)
        {

            wall.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.green;
        }
        else {
            wall.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;

        }

    }
    // ============   wall spawn ends =================//  

    [Command]
    public void CmdSpawn(string name)
    {
        Debug.Log("IM INSIDE SPAWN");
        Debug.Log(name);


        if (!hasAuthority)
        {
            //unitRandom2 = GetUnitRandom();
            unitRandom = GetUnitRandom();

            GameObject unit;
            unit = Instantiate(Resources.Load("units/" + name, typeof(GameObject)), new Vector3(128f, 0.5f, Random.Range(61f, 68f)), Quaternion.identity) as GameObject;
            unit.gameObject.tag = "Player";
            unit.GetComponent<PlayerNavMesh>().UnitTag = "Enemy";
            unit.GetComponent<PlayerNavMesh>().enabled = true;
            //unit.GetComponent<UnitStats>().unitRandom = unitRandom2;
            //Debug.Log(" unit stats random 2 Value ------------- :   " + unitRandom2);

            //		NetworkServer.Spawn(unit);
            Debug.Log("here");
            if (hasAuthority)
            {

                unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.green;
            }
            else {
                unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;
            }
            NetworkServer.Spawn(unit);
            RpcTurnToEnemyWithoutAuthority(unit);


        }
        else {
            unitRandom = GetUnitRandom();

            GameObject unit;
            unit = Instantiate(Resources.Load("units/" + name, typeof(GameObject)), new Vector3(97f, 0.5f, Random.Range(61f, 68f)), Quaternion.identity) as GameObject;
            unit.gameObject.tag = "Enemy";
            unit.GetComponent<PlayerNavMesh>().UnitTag = "Player";
            unit.GetComponent<PlayerNavMesh>().enabled = true;
            //unit.GetComponent<UnitStats>().unitRandom = unitRandom;
            //Debug.Log(" unit stats random Value -----has Auth-------- :   " + unitRandom);
            unit.GetComponent<Renderer>().material.color = Color.red;
            unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;
            NetworkServer.Spawn(unit);
            RpcTurnToEnemy(unit);
        }

        //		if(hasAuthority){
        //
        //			NetworkServer.Spawn(unit);
        //
        //		}else{
        //
        //
        //			NetworkServer.Spawn(unit);
        //
        //
        //		}


    }

    [ClientRpc]
    public void RpcTurnToEnemy(GameObject unit)
    {

        unit.gameObject.tag = "Enemy";
        unit.GetComponent<PlayerNavMesh>().UnitTag = "Player";
        unit.GetComponent<PlayerNavMesh>().enabled = true;
        unit.GetComponent<Renderer>().material.color = Color.red;
        unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;
        unit.GetComponent<UnitStats>().unitRandom = unitRandom;
        Debug.Log(" unit stats random 2 Value ------RPC Auth------- :   " + unitRandom);
        //unit.GetComponent<NetPlayerCheck>().isPlayer1 = false;
        //unit.GetComponent<NetPlayerCheck>().chkPlayer1 = 4;

        if (hasAuthority)
        {

            unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.green;
            unit.GetComponent<NetPlayerCheck>().IsPlayer1 = true;
            unit.GetComponent<NetPlayerCheck>().chkPlayer1 = 1;
            unit.GetComponent<NetPlayerCheck>().unitOwner = true;
            unit.GetComponent<NetPlayerCheck>().chk2Player1 = 3;

        }
        else {
            unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;

            unit.GetComponent<NetPlayerCheck>().IsPlayer1 = false;
            unit.GetComponent<NetPlayerCheck>().chkPlayer1 = 2;
            unit.GetComponent<NetPlayerCheck>().unitOwner = false;
            unit.GetComponent<NetPlayerCheck>().chk2Player1 = 4;

        }
        Debug.Log("here");

    }

    [ClientRpc]
    public void RpcTurnToEnemyWithoutAuthority(GameObject unit)
    {

        unit.gameObject.tag = "Player";
        unit.GetComponent<PlayerNavMesh>().UnitTag = "Enemy";
        unit.GetComponent<PlayerNavMesh>().enabled = true;
        unit.GetComponent<UnitStats>().unitRandom = unitRandom;
        Debug.Log(" unit stats random Value -------RPC without Auth------ :   " + unitRandom);
        //		NetworkServer.Spawn(unit);
        Debug.Log("here");
        unit.GetComponent<NetPlayerCheck>().IsPlayer1 = true;
        //unit.GetComponent<NetPlayerCheck>().chkPlayer1 = 3;

        if (hasAuthority)
        {

            unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.green;
            unit.GetComponent<NetPlayerCheck>().unitOwner = false;
            unit.GetComponent<NetPlayerCheck>().chk2Player1 = 6;
        }
        else {
            unit.transform.Find("Canvas/hpbar/hp").GetComponent<Image>().color = Color.red;
            unit.GetComponent<NetPlayerCheck>().unitOwner = true;
            unit.GetComponent<NetPlayerCheck>().chk2Player1 = 5;

        }

    }

    void SpawnCardNow(Vector3 pos, string _name, string cName)
    {
        if (!isLocalPlayer)
        {
            print("Exiting spawncardnow =  !local player ");
            return;
        }
        else {
            //print(card.name);
            if (_name.Equals("ReversalHelper"))
            {
                CmdSpawnHelp(pos, _name, cName);
            }
            else
            {
                CmdSpawnNow(pos, _name);
                print("Exiting spawncardnow = is local player ");
            }
        }
    }

    [Command]
    void CmdSpawnHelp(Vector3 pos, string _name, string cName)
    {
        GameObject cardz = Instantiate(Resources.Load("Cards/" + _name), pos, Quaternion.identity) as GameObject;

        if (hasAuthority)
        {
            //GameObject cardz = Instantiate(card, card.transform.position, Quaternion.identity) as GameObject;
            cardz.GetComponent<NetPlayerCheck>().IsPlayer1 = true;
            cardz.GetComponent<ReversalHelper>().cName = cName;


            NetworkServer.Spawn(cardz);

        }
        else
        {
            cardz.GetComponent<NetPlayerCheck>().IsPlayer1 = false;
            cardz.GetComponent<ReversalHelper>().cName = cName;

            NetworkServer.SpawnWithClientAuthority(cardz, this.gameObject);

        }
    }


    [Command]
    void CmdSpawnNow(Vector3 pos, string name)
    {
        //card =Instantiate(Resources.Load("Cards/Cube"),card.transform.position,Quaternion.identity) as GameObject;
        //NetworkServer.Spawn(card);
        GameObject cardz = Instantiate(Resources.Load("Cards/" + name), pos, Quaternion.identity) as GameObject;

        if (hasAuthority)
        {
            //GameObject cardz = Instantiate(card, card.transform.position, Quaternion.identity) as GameObject;
            cardz.GetComponent<NetPlayerCheck>().IsPlayer1 = true;

            //if (cardz.name.Contains("Reversal"))
            //{
            //    NetworkServer.SpawnWithClientAuthority(cardz, connectionToClient);
            //}
            //else
            //{
            NetworkServer.Spawn(cardz);
            //}

            print("add button - cmdspawnnow - has authority :  " + hasAuthority);
        }
        else
        {
            print("add button - cmdspawnnow - has authority :  " + hasAuthority);

            //GameObject cardz = Instantiate(card, card.transform.position, Quaternion.identity) as GameObject;
            cardz.GetComponent<NetPlayerCheck>().IsPlayer1 = false;

            NetworkServer.SpawnWithClientAuthority(cardz, this.gameObject);

            //NetworkServer.Spawn(cardz);

            //RpcChangePlayerTag(cardz);
            //NetworkServer.Spawn(cardz);
        }



    }

    //[ClientRpc]
    //void RpcChangePlayerTag(GameObject card) {
    //    card.GetComponent<NetPlayerCheck>().IsPlayer1 = false;
    //}


    //---------------------------------------------Networking-----------------------------//
}