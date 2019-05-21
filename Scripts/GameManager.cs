using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Amazon;
using Amazon.Runtime;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using Amazon.CognitoSync;
using Amazon.CognitoSync.SyncManager;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Internal;
using Amazon.DynamoDBv2.Model;
using Amazon.S3.Internal;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.S3.Model.Internal.MarshallTransformations;
using Amazon.S3.Model.Internal;
using System;
using System.Linq;
using SimpleJSON;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Amazon.S3;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif





#region CHEST CLASS
public enum Etypes { card, weapon, shard };
public enum Ecard_rarity { common, uncommon, rare, exotic };
public enum Echest_rarity { wooden, bronze, silver, gold, immortal };


public class ChestClass
{


    public int[] chest_numbers = new int[5] { 8, 10, 12, 14, 16 };
    public List<string> cardsNames = new List<string>();
    public List<string> rarity_Names = new List<string>();
    public List<KeyValuePair<string, double>> woodenChance = new List<KeyValuePair<string, double>>() { new KeyValuePair<string, double>("Common", 0.79f), new KeyValuePair<string, double>("Uncommon", 0.40f), new KeyValuePair<string, double>("Rare", 0.01f), new KeyValuePair<string, double>("Exotic", 0f) };
    public List<KeyValuePair<string, double>> bronzeChance = new List<KeyValuePair<string, double>>() { new KeyValuePair<string, double>("Common", 0.55f), new KeyValuePair<string, double>("Uncommon", 0.40f), new KeyValuePair<string, double>("Rare", 0.05f), new KeyValuePair<string, double>("Exotic", 0f) };
    public List<KeyValuePair<string, double>> silverChance = new List<KeyValuePair<string, double>>() { new KeyValuePair<string, double>("Common", 0.27f), new KeyValuePair<string, double>("Uncommon", 0.50f), new KeyValuePair<string, double>("Rare", 0.22f), new KeyValuePair<string, double>("Exotic", 0.05f) };
    public List<KeyValuePair<string, double>> goldChance = new List<KeyValuePair<string, double>>() { new KeyValuePair<string, double>("Common", 0.075f), new KeyValuePair<string, double>("Uncommon", 0.27f), new KeyValuePair<string, double>("Rare", 0.60f), new KeyValuePair<string, double>("Exotic", 0.5f) };
    public List<KeyValuePair<string, double>> immortalChance = new List<KeyValuePair<string, double>>() { new KeyValuePair<string, double>("Common", 0.0f), new KeyValuePair<string, double>("Uncommon", 0.1f), new KeyValuePair<string, double>("Rare", 0.75f), new KeyValuePair<string, double>("Exotic", 0.15f) };
    public List<List<KeyValuePair<string, double>>> chances;


    public Dictionary<string,int> prices = new Dictionary<string, int>() {



        { "bronze",6},
        { "silver", 12},
        { "gold", 22},
        { "immortal", 38}






    };






    public string[] commonCards;
    public string[] uncommonCards;
    public string[] rareCards;
    public string[] exoticCards;



}




#endregion




#region Cryptography

public static class CryptographyProvider
{
    public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
    {
        byte[] encryptedBytes = null;


        byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        using (MemoryStream ms = new MemoryStream())
        {
            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;

                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);

                AES.Mode = CipherMode.CBC;

                using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                    cs.Close();
                }
                encryptedBytes = ms.ToArray();
            }
        }

        return encryptedBytes;
    }


    public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
    {
        byte[] decryptedBytes = null;


        byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        using (MemoryStream ms = new MemoryStream())
        {
            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;

                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);

                AES.Mode = CipherMode.CBC;

                using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                    cs.Close();
                }
                decryptedBytes = ms.ToArray();
            }
        }

        return decryptedBytes;
    }


    public static string EncryptText(string input, string password)
    {

        byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);


        passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

        byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

        string result = Convert.ToBase64String(bytesEncrypted);

        return result;
    }



    public static string DecryptText(string input, string password)
    {

        byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

        byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

        string result = Encoding.UTF8.GetString(bytesDecrypted);

        return result;
    }
}



#endregion




#region Database_Amazon

[DynamoDBTable("Players")]
public class PlayerData
{

    [DynamoDBHashKey]
    public string userName { get; set; }
    [DynamoDBProperty]
    public string email { get; set; }
    [DynamoDBProperty]
    public string password { get; set; }
    [DynamoDBProperty("LoadOut1")]
    public List<string> DB_loadout1 { get; set; }
    [DynamoDBProperty("LoadOut2")]
    public List<string> DB_loadout2 { get; set; }
    [DynamoDBProperty("LoadOut3")]
    public List<string> DB_loadout3 { get; set; }
    [DynamoDBProperty("HybridArmy")]
    public List<string> DB_HybridArmy { get; set; }
    [DynamoDBProperty("cards")]
    public Dictionary<string, int> DB_cards { get; set; }
    [DynamoDBProperty("FirstTime")]
    public bool firstTime = false;
    [DynamoDBProperty("HeroLoadOut")]
    public Dictionary<string, string> DB_HeroLoadOut { get; set; }
    [DynamoDBProperty("chests")]
    public Dictionary<string, int> DB_chests { get; set; }
    [DynamoDBProperty("currency")]
    public int DB_currency { get; set; }
    [DynamoDBProperty("token")]
    public int DB_token { get; set; }
    [DynamoDBProperty("xp")]
    public Dictionary<string, int> DB_xp { get; set; }
    [DynamoDBProperty("hybrid")]
    public Dictionary<string, int> DB_hybrid { get; set; }
    [DynamoDBProperty("stats")]
    public Dictionary<string, int> DB_stats { get; set; }
    [DynamoDBProperty("loadOutNames")]
    public Dictionary<string, string> DB_loadOutNames { get; set; }
    [DynamoDBProperty("levelHistory")]
    public string DB_levelHistory { get; set; }
    [DynamoDBProperty("ArmorItems")]
    public Dictionary<string, int> DB_armorItems { get; set; }

}


#endregion






public class GameManager : MonoBehaviour, IAmazonDynamoDB
{
    public GameObject profileBar;
    public Transform t_loadout1;
    public Transform t_loadout2;
    public Transform t_loadout3;
    public Transform deckPlace;
    public Text shardText;
    public Text tokenText;



    #region Slot Variables
    public GameObject slotPanel;
    public Camera slotCam;
    RaycastHit hit;
    public bool canStartSlot = false;
    string slotItemName = "";
    public GameObject itemPanelSlot;
    public GameObject slot1;
    public GameObject slot2;
    public GameObject slot3;
    float time = 0;
    float max = 6f;
    float interval = 1f;
    bool isSpinning = false;
    public enum slotItems : int
    {
        shard_1 = 0,
        shard_3 = 1,
        shard_5 = 2,
        shard_10 = 3,
        shard_30 = 4,
        shard_100 = 5,
        chest_bronze = 6,
        chest_silver = 7,
        chest_gold = 8,
        chest_immortal = 9,
        none = 10,
        token_1 = 11,
        token_3 = 12,
        token_10 = 13


    };
    public List<KeyValuePair<string, double>> slotChances = new List<KeyValuePair<string, double>>()
    {

        new KeyValuePair<string, double>("shard_1",0.18f),
        new KeyValuePair<string, double>("shard_3",0.14f),
        new KeyValuePair<string, double>("shard_5",0.09f),
        new KeyValuePair<string, double>("shard_10",0.05f),
        new KeyValuePair<string, double>("shard_30",0.019f),
        new KeyValuePair<string, double>("shard_100",0.001f),
        new KeyValuePair<string, double>("chest_bronze",0.165f),
        new KeyValuePair<string, double>("chest_silver",0.08f),
        new KeyValuePair<string, double>("chest_gold",0.035f),
        new KeyValuePair<string, double>("chest_immortal",0.02f),
        new KeyValuePair<string, double>("none",0.12f),
        new KeyValuePair<string, double>("token_1",0.096f),
        new KeyValuePair<string, double>("token_3",0.003f),
        new KeyValuePair<string, double>("token_10",0.001f)

        //t-0  s-40  c-45 x-80
    };


    public List<KeyValuePair<string, Vector3>> slotChanceNames = new List<KeyValuePair<string, Vector3>>()
    {

      new KeyValuePair<string, Vector3>("STC",new Vector3(40,0,40)),
        new KeyValuePair<string, Vector3>("SCS",new Vector3(40,135,40)),
        new KeyValuePair<string, Vector3>("STS",new Vector3(40,0,40)),
        new KeyValuePair<string, Vector3>("SSC",new Vector3(40,40,135)),
        new KeyValuePair<string, Vector3>("SST",new Vector3(40,40,0)),
        new KeyValuePair<string, Vector3>("SSS",new Vector3(40,40,40)),
        new KeyValuePair<string, Vector3>("CTC",new Vector3(135,0,135)),
        new KeyValuePair<string, Vector3>("CCS",new Vector3(135,135,40)),
        new KeyValuePair<string, Vector3>("CCT",new Vector3(135,135,0)),
        new KeyValuePair<string, Vector3>("CCC",new Vector3(135,135,135)),
        new KeyValuePair<string, Vector3>("XXX",new Vector3(90,90,90)),
        new KeyValuePair<string, Vector3>("TTS",new Vector3(0,0,40)),
        new KeyValuePair<string, Vector3>("TTC",new Vector3(0,0,135)),
        new KeyValuePair<string, Vector3>("TTT",new Vector3(0,0,0))


    };


    #endregion

    #region Chest Variables

    public Image chest_panel;
    public GameObject[] cards;
    public Button[] chestsAll;
    public GameObject itemPanelChest;
    public GameObject chest;
    public ChestClass chest_obj;
    public Animator chestAnim;
    public GameObject chestOk;
    public Text[] chestNumbers;

    // dummy variable
    public Transform dummyDeckPanel;

    #endregion

    #region Login Variables

    public InputField userName_login;
    public InputField password_login;
    public GameObject load_login;


    #endregion


    #region Option_Panel Variables
    #endregion

    #region Register Variables

    public GUIHandler guiHandler;
    public bool saveResult = true;

    public GameObject matchError;
    public GameObject Show;
    public GameObject Register;

    public InputField email;
    public InputField userName;
    public InputField pass1;
    public InputField pass2;

    public Button regButton;

    #endregion


    #region Amazon variables

    public Dataset playerInfo;
    public CognitoSyncManager syncManager;
    public CognitoAWSCredentials credentials;
    public AmazonDynamoDBClient client;
    public AmazonS3Client s3_client;
    public DynamoDBContext context;
    public AmazonDynamoDBResult SaveResult;
    public IAmazonDynamoDB _client;
    private QueryResponse defaultResponse;

    #endregion


    #region GameManager Variables
    private static GameManager instance;
    public Canvas mainCanvas;

    public static GameManager Instance
    {

        get
        {


            return instance;

        }


    }






    #endregion

    #region User Stats Variables

    public string factionName;
    public List<string> cardDeck = new List<string>();
    public string[] hybridArmy = new string[6];
    public List<string> LoadOut1 = new List<string>();
    public List<string> LoadOut2 = new List<string>();
    public List<string> LoadOut3 = new List<string>();
    public List<string> LoadOutUsed = new List<string>();
    public List<string> Default1 = new List<string>();
    public List<string> Default2 = new List<string>();
    public List<string> Default3 = new List<string>();
    public List<string> gameDeck = new List<string>();
    public List<string> deadUnits = new List<string>();
    public string gameDeckName;
    public bool isLoggedIn = false;
    public PlayerData player;
    public TextAsset cards_stats;
    public Dictionary<string, string> characterLoadOut = new Dictionary<string, string>();
    public List<string> characterItems = new List<string>();
    public bool _bool_unitsSpawnDisable = false;

    public int loginAttempts = 0;
    public int loginExceptions = 0;


    
    public JSONNode armorNames;
    public List<string> allArmorNames;

    public delegate void UserFound(); //Anamta Edit
    public static event UserFound EventUserFound; //Anamta Edit

    #endregion


    #region Unity Functions

    void Awake()
    {
        //if (SceneManager.GetActiveScene().buildIndex == 1)
        //{


        //    mainCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        //    mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        //    print("zzzzzzzzzzzzzzz");
        //}

        loginAttempts = 0;
        loginExceptions = 0;

        if (instance != null && instance != this)
        {

            Destroy(gameObject);
        }


        instance = this;


        DontDestroyOnLoad(this.gameObject);
    }


    public void CardsUsed(string name)
    {

        //if (gameDeckName != null)
        //{

        //    switch (gameDeckName)
        //    {
        //        case "LoadOut1":
        if (LoadOutUsed.Contains(name))
        {

        }
        else
        {
            LoadOutUsed.Add(name);
        }
        //            break;

        //        case "LoadOut2":

        //            LoadOutUsed.Add(name);

        //            break;

        //        case "LoadOut3":

        //            LoadOutUsed.Add(name);

        //            break;

        //        default:
        //            break;
        //    }

        //}
    }


    public void ResetCardsUsage()
    {
        //-------------------------------- REMOVING CARDS AGAIN AFTER USAGE NEED REWORK WITH NEW DECK-------------------------------

        //for (int i = 0; i < LoadOutUsed.Count; i++)
        //{
        //    GameManager.Instance.player.DB_cards[LoadOutUsed[i]] -= 1;
        //    if (GameManager.Instance.player.DB_loadout1.Contains(LoadOutUsed[i]))
        //    {

        //        GameManager.Instance.player.DB_loadout1.Remove(LoadOutUsed[i]);
        //        foreach (Transform item in t_loadout1)
        //        {

        //            if (item.name == LoadOutUsed[i])
        //            {

        //                Destroy(item.gameObject);
        //            }
        //        }

        //    }
        //    if (GameManager.Instance.player.DB_loadout2.Contains(LoadOutUsed[i]))
        //    {

        //        GameManager.Instance.player.DB_loadout2.Remove(LoadOutUsed[i]);
        //        foreach (Transform item in t_loadout2)
        //        {

        //            if (item.name == LoadOutUsed[i])
        //            {

        //                Destroy(item.gameObject);
        //            }
        //        }

        //    }
        //    if (GameManager.Instance.player.DB_loadout3.Contains(LoadOutUsed[i]))
        //    {

        //        GameManager.Instance.player.DB_loadout3.Remove(LoadOutUsed[i]);
        //        foreach (Transform item in t_loadout2)
        //        {

        //            if (item.name == LoadOutUsed[i])
        //            {

        //                Destroy(item.gameObject);
        //            }
        //        }

        //    }
        //}

        //LoadOutUsed.Clear();

        //SavePlayerData();


    }


    public void OnEnable()
    {
        CardMove.EventLogCardPlayed += CardsUsed;
    }

    public void OnDisable()
    {
        CardMove.EventLogCardPlayed += CardsUsed;
    }


    void Start()
    {


        PlayerPrefs.SetFloat("firstTime", 1);
        UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.LoggingConfig.LogTo = LoggingOptions.UnityLogger;
        credentials = new CognitoAWSCredentials("eu-west-1:fcc1d703-f707-41db-87ae-0da44ba0264d", RegionEndpoint.EUWest1);
        syncManager = new CognitoSyncManager(credentials, RegionEndpoint.EUWest1);
        client = new AmazonDynamoDBClient(credentials, RegionEndpoint.EUWest1);
        context = new DynamoDBContext(client);
        s3_client = new AmazonS3Client(credentials, RegionEndpoint.EUWest1);
        _client = client;
        player = new PlayerData();
        player.DB_loadout1 = new List<string>();

      
        GetObject();




        hybridArmy = new string[6];
        _bool_unitsSpawnDisable = false;

        loginAttempts = 0;
        loginExceptions = 0;


        chest_obj = new ChestClass();
        var text = Resources.Load("rarity") as TextAsset;

        Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(text.text);
        chest_obj.commonCards = values.Where(x => x.Value == "Common").Select(p => p.Key).ToArray<string>();
        chest_obj.uncommonCards = values.Where(x => x.Value == "Uncommon").Select(p => p.Key).ToArray<string>();
        chest_obj.rareCards = values.Where(x => x.Value == "Rare").Select(p => p.Key).ToArray<string>();
        chest_obj.exoticCards = values.Where(x => x.Value == "Exotic").Select(p => p.Key).ToArray<string>();

        chest_obj.chances = new List<List<KeyValuePair<string, double>>>() { chest_obj.woodenChance, chest_obj.bronzeChance, chest_obj.silverChance, chest_obj.goldChance, chest_obj.immortalChance };

        UnityEngine.Random.seed = System.Environment.TickCount;

        //------------------------------------- TEST DEFAULT ACCOUNT HERE AND TEXT ASSETS

        TextAsset armorText = Resources.Load("armorItemNames") as TextAsset;
        armorNames = JSON.Parse(armorText.text);
        //print(armorNames[0][1]);

        context.LoadAsync<PlayerData>("null", (result) =>
        {


            print(result.Result.DB_loadOutNames["LoadOut1"]);

            player = result.Result;

            allArmorNames = new List<string>(player.DB_armorItems.Keys);
            // print(allArmorNames.Count);
            guiHandler.LoadArmor();
            guiHandler.GetBaseHero();
        });

        //------------------------------------- TEST DEFAULT ACCOUNT HERE AND TEXT ASSETS

    }

    void Update()
    {

        //if (Input.GetKeyUp(KeyCode.Space) && !isSpinning) {

        //    GenSlot();
        //    isSpinning = true;
        //}





        if (guiHandler.current_panel == "SlotMachine_Panel")
        {
            slotPanel.gameObject.SetActive(true);

            if (!canStartSlot)
            {

                if (Input.GetMouseButtonDown(0))
                {


                    Ray ray = slotCam.ScreenPointToRay(Input.mousePosition);


                    if (Physics.Raycast(ray, out hit, 100))
                    {

                        if (hit.transform.name == "slot")
                        {

                            canStartSlot = true;

                            if (player.DB_token == 0)
                            {


                            }
                            else
                            {

                                GenSlot();
                                player.DB_token -= 1;
                                LoadTokenAgain();

                            }

                        }

                    }

                }



            }



        }
        else {

            slotPanel.gameObject.SetActive(false);
        }


    }

    void FixedUpdate()
    {




    }


    public void BuyChestWithShards(string name) {

        print("i are clickzed");
        //  int number = chest_obj.prices[name].Value;
        int number = player.DB_currency - chest_obj.prices[name];
        if (number < 0)
        {


            print("i are clickzed");



        }
        else {
            player.DB_chests[name] += 1;
            player.DB_currency -= chest_obj.prices[name];
            LoadChestAgain();
            LoadCurrencyAgain();
            SavePlayerData();
            print("i are clickzed");

        }



        print("i are clickzed");


    }


    


    public void GenChest(string chestName)
    {

        if (player.DB_chests[chestName] == 0)
        {


        }
        else
        {
            player.DB_chests[chestName] -= 1;
            LoadChestAgain();
            StartCoroutine(GenChestHelper(chestName));

        }
    }

    IEnumerator GenChestHelper(string chestName)
    {

        for (int i = 0; i < chestsAll.Length; i++)
        {
            yield return false;
            chestsAll[i].interactable = false;
        }

        chestName = EventSystem.current.currentSelectedGameObject.name;

        int number = (int)(Echest_rarity)Enum.Parse(typeof(Echest_rarity), chestName);

        mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;


        double diceRoll = UnityEngine.Random.value;

        double cumulative = 0.0;


        for (int j = 0; j < chest_obj.chest_numbers[number]; j++)
        {

            yield return null;

            for (int i = 0; i < chest_obj.chances[number].Count; i++)
            {
                yield return null;
                cumulative += chest_obj.chances[number][i].Value;
                if (diceRoll < cumulative)
                {
                    chest_obj.rarity_Names.Add(chest_obj.chances[number][i].Key);

                    break;
                }

            }
        }




        for (int i = 0; i < chest_obj.rarity_Names.Count; i++)
        {

            yield return null;

            if (chest_obj.rarity_Names[i] == "Common")
            {
                string _cname = chest_obj.commonCards[UnityEngine.Random.Range(0, chest_obj.commonCards.Length)];
                chest_obj.cardsNames.Add(_cname);
                player.DB_cards[_cname] += 1;
            }
            else if (chest_obj.rarity_Names[i] == "Uncommon")
            {
                string _cname = chest_obj.uncommonCards[UnityEngine.Random.Range(0, chest_obj.uncommonCards.Length)];
                chest_obj.cardsNames.Add(_cname);
                player.DB_cards[_cname] += 1;
            }
            else if (chest_obj.rarity_Names[i] == "Rare")
            {

                string _cname = chest_obj.rareCards[UnityEngine.Random.Range(0, chest_obj.rareCards.Length)];
                chest_obj.cardsNames.Add(_cname);
                player.DB_cards[_cname] += 1;
            }
            else if (chest_obj.rarity_Names[i] == "Exotic")
            {
                string _cname = chest_obj.exoticCards[UnityEngine.Random.Range(0, chest_obj.exoticCards.Length)];
                chest_obj.cardsNames.Add(_cname);
                player.DB_cards[_cname] += 1;

            }



        }




        itemPanelChest.SetActive(true);
        chest.SetActive(true);



        yield return new WaitForSeconds(chestAnim.GetCurrentAnimatorStateInfo(0).length);

        chestOk.SetActive(true);


        for (int i = 0; i < chest_obj.cardsNames.Count; i++)
        {


            cards[i].GetComponent<Renderer>().material.mainTexture = Resources.Load<Texture>("cardImages/" + chest_obj.cardsNames[i]) as Texture;
            cards[i].SetActive(true);

        }




        yield return 0;

    }







    public void CloseChest()
    {

        chest_panel.raycastTarget = true;
        mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        itemPanelChest.SetActive(false);
        chest.SetActive(false);
        for (int i = 0; i < chest_obj.cardsNames.Count; i++)
        {



            cards[i].SetActive(false);


        }

        SavePlayerData();

        chest_obj.cardsNames.Clear();
        chest_obj.rarity_Names.Clear();

        chestOk.SetActive(false);

        for (int i = 0; i < chestsAll.Length; i++)
        {

            chestsAll[i].interactable = true;
        }

        UpdateCardsValueAfterChest();

    }


    public void UpdateCardsValueAfterChest()
    {

        LoadCardsAgain();
    }


    public void GenSlot()
    {

        //slot1.gameObject.transform.Rotate(new Vector3(100 * Time.deltaTime, 0.0f, 0.0f));

        StartCoroutine(GenSlotHelper());

    }

    IEnumerator GenSlotHelper()
    {


        double diceRoll = UnityEngine.Random.value;

        double cumulative = 0.0;

     

        int number = 0;

        for (int j = 0; j < slotChances.Count; j++)
        {

            yield return null;

            cumulative += slotChances[j].Value;
            if (diceRoll < cumulative)
            {

                slotItemName = slotChances[j].Key;

                break;
            }


        }


        print(slotItemName);
        number = (int)((slotItems)Enum.Parse(typeof(slotItems), slotItemName));

        // print(slotChanceNames[number].Key);
        // print(slotChanceNames[number].Value.x +","+ slotChanceNames[number].Value.y + "," + slotChanceNames[number].Value.z);
        float x, y, z;
        y = slotChanceNames[number].Value.y;
        x = slotChanceNames[number].Value.x;
        z = slotChanceNames[number].Value.z;

        StartCoroutine(wheel1(x));
        StartCoroutine(wheel2(y));
        StartCoroutine(wheel3(z));

        print(x + " " + y + " " + z);


        yield return 0;

    }

    IEnumerator wheel1(float x)
    {


        float time = 0;
        time += Time.deltaTime;

        while (time < 6)
        {
            yield return null;
            slot1.gameObject.transform.Rotate(new Vector3(-500 * Time.deltaTime, 0, 0.0f), Space.World);
            time += Time.deltaTime;
        }


        yield return 0;

        slot1.gameObject.transform.eulerAngles = new Vector3(x, -180f, 90f);


    }

    IEnumerator wheel2(float y)
    {


        float time = 0;
        time += Time.deltaTime;

        while (time < 7)
        {
            yield return null;
            slot2.gameObject.transform.Rotate(new Vector3(-500 * Time.deltaTime, 0, 0.0f), Space.World);
            time += Time.deltaTime;
        }



        yield return 0;
        slot2.gameObject.transform.eulerAngles = new Vector3(y, -180f, 90f);

    }


    IEnumerator wheel3(float z)
    {


        float time = 0;
        time += Time.deltaTime;

        while (time < 8)
        {
            yield return null;
            slot3.gameObject.transform.Rotate(new Vector3(-500 * Time.deltaTime, 0, 0.0f), Space.World);
            time += Time.deltaTime;
        }



        yield return 0;

        slot3.gameObject.transform.eulerAngles = new Vector3(z, -180f, 90f);

        yield return new WaitForSeconds(2);
      
        itemPanelSlot.SetActive(true);
        itemPanelSlot.GetComponentInChildren<Text>().text = "YOU RECIEVED " + slotItemName;
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        UpdateItemsAfterSlot();
    }



    public void CloseSlot()
    {

        canStartSlot = false;
        itemPanelSlot.SetActive(false);
        mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;

    }

    public void UpdateItemsAfterSlot()
    {

        if (slotItemName.Contains("chest"))
        {
            string name = slotItemName.Substring(6, slotItemName.Length - 6);
            player.DB_chests[name] += 1;
            LoadChestAgain();
        }
        else if (slotItemName.Contains("shard"))
        {
            player.DB_currency += int.Parse(slotItemName.Substring(6,slotItemName.Length - 6));
            LoadCurrencyAgain();
           

        }
        else if (slotItemName.Contains("token"))
        {
            player.DB_token += int.Parse(slotItemName.Substring(6, slotItemName.Length - 6));
            LoadTokenAgain();
            
        }
        else if (slotItemName.Contains("none"))
        {

        }
        print(slotItemName);
        SavePlayerData();
    }



    private void GetObject()
    {
        string unit = null;
        string hero = null;
        string card = null;

        s3_client.GetObjectAsync("game.data", "cards.json", (responseObj) =>
        {

            var response = responseObj.Response;

            if (response.ResponseStream != null)
            {
                using (StreamReader reader = new StreamReader(response.ResponseStream))
                {
                    card = reader.ReadToEnd();
                }


            }

            string fileName = @Application.dataPath + "/Resources/cards.json";


            if (!File.Exists(fileName))
            {

                using (StreamWriter writer = new StreamWriter(fileName, false))
                {
                    writer.Write(CryptographyProvider.EncryptText(card, "90abc"));
                }

#if UNITY_EDITOR

                AssetDatabase.Refresh();
#endif

            }
            else
            {

                using (StreamWriter writer = new StreamWriter(fileName, false))
                {
                    writer.Write(CryptographyProvider.EncryptText(card, "90abc"));
                }
#if UNITY_EDITOR

                AssetDatabase.Refresh();
#endif
            }

        });


        s3_client.GetObjectAsync("game.data", "hero.json", (responseObj) =>
        {

            var response = responseObj.Response;

            if (response.ResponseStream != null)
            {
                using (StreamReader reader = new StreamReader(response.ResponseStream))
                {
                    hero = reader.ReadToEnd();
                }

#if UNITY_EDITOR

                AssetDatabase.Refresh();
#endif
            }

            string fileName = @Application.dataPath + "/Resources/hero.json";


            if (!File.Exists(fileName))
            {

                using (StreamWriter writer = new StreamWriter(fileName, false))
                {
                    writer.Write(CryptographyProvider.EncryptText(hero, "90abc"));
                }
#if UNITY_EDITOR

                AssetDatabase.Refresh();
#endif
            }
            else
            {

                using (StreamWriter writer = new StreamWriter(fileName, false))
                {
                    writer.Write(CryptographyProvider.EncryptText(hero, "90abc"));
                }
#if UNITY_EDITOR

                AssetDatabase.Refresh();
#endif
            }

        });

        s3_client.GetObjectAsync("game.data", "unit.json", (responseObj) =>
        {

            var response = responseObj.Response;

            if (response.ResponseStream != null)
            {
                using (StreamReader reader = new StreamReader(response.ResponseStream))
                {
                    unit = reader.ReadToEnd();
                }
#if UNITY_EDITOR

                AssetDatabase.Refresh();
#endif

            }

            string fileName = @Application.dataPath + "/Resources/unit.json";


            if (!File.Exists(fileName))
            {

                using (StreamWriter writer = new StreamWriter(fileName, false))
                {
                    writer.Write(CryptographyProvider.EncryptText(unit, "90abc"));
                }
#if UNITY_EDITOR

                AssetDatabase.Refresh();
#endif
            }
            else
            {

                using (StreamWriter writer = new StreamWriter(fileName, false))
                {
                    writer.Write(CryptographyProvider.EncryptText(unit, "90abc"));
                }
#if UNITY_EDITOR

                AssetDatabase.Refresh();
#endif
            }

        });

#if UNITY_EDITOR

        AssetDatabase.Refresh();
#endif


    }



    public void GetFormData()
    {

        player.email = email.text;
        player.userName = userName.text;
        player.password = pass2.text;


        StartCoroutine(IRegisterCheck());
    }


    IEnumerator IRegisterCheck()
    {
        bool check = false;

        Register.SetActive(false);

        Show.SetActive(true);

        Show.GetComponentInChildren<Text>().text = "Trying To Register....";

        yield return null;


        TestHelper(new QueryRequest(), null);


        yield return null;


       
        yield return 0;
    }










    void CheckItemSample()
    {
        CheckItemHelper(new PutItemRequest(), null);
    }

    void CheckItemHelper(PutItemRequest request, Dictionary<string, AttributeValue> lastKeyEvaluated)
    {
        request.TableName = "Players";

        request = new PutItemRequest
        {
            TableName = "Players",

            Item = new Dictionary<string, AttributeValue>() {

                
               //{"userName" , new AttributeValue { S = (player.userName).ToLower() } },
               //{"password" , new AttributeValue { S = player.password } },
               //{"email" , new AttributeValue { S = player.email } },
               //{"LoadOut1" , new AttributeValue {SS = new List<string>{ "null" } } },
               //{"LoadOut2" , new AttributeValue {SS = new List<string>{ "null" } } },
               //{"LoadOut3" , new AttributeValue {SS = new List<string>{ "null" } } },
               //{"HybridArmy" , new AttributeValue { SS = new List<string>{ "null" } } },
               {"userName" , new AttributeValue { S = player.userName.ToLower() } },
               {"password" , new AttributeValue { S = player.password } },
               {"email" , new AttributeValue { S = player.email } },
               {"LoadOut1" , new AttributeValue {SS = new List<string>{ "null" } } },
               {"LoadOut2" , new AttributeValue {SS = new List<string>{ "null" } } },
               {"LoadOut3" , new AttributeValue {SS = new List<string>{ "null" } } },
               {"HybridArmy" , new AttributeValue { SS =new List<string>{ "null" }  } },
               {"cards" , new AttributeValue { M = defaultResponse.Items[0]["cards"].M } },
               {"FirstTime" , new AttributeValue { BOOL = defaultResponse.Items[0]["FirstTime"].BOOL } },
               {"HeroLoadOut" , new AttributeValue { M = defaultResponse.Items[0]["HeroLoadOut"].M   } },
               {"chests" , new AttributeValue { M = defaultResponse.Items[0]["chests"].M  } },
               {"currency" , new AttributeValue {  N = defaultResponse.Items[0]["currency"].N } },
               {"token" , new AttributeValue {  N = defaultResponse.Items[0]["token"].N } },
               {"xp" , new AttributeValue { M = defaultResponse.Items[0]["xp"].M  } },
               {"hybrid" , new AttributeValue { M = defaultResponse.Items[0]["hybrid"].M  } },
               {"stats" , new AttributeValue { M = defaultResponse.Items[0]["stats"].M  } },
               {"loadOutNames" , new AttributeValue {M = defaultResponse.Items[0]["loadOutNames"].M } },
               {"levelHistory" , new AttributeValue {S = null} },
               {"ArmorItems" , new AttributeValue { M = defaultResponse.Items[0]["ArmorItems"].M  } },

    //[DynamoDBProperty("chests")]
    //public Dictionary<string, int> DB_chests { get; set; }
    //[DynamoDBProperty("currency")]
    //public int DB_currency { get; set; }
    //[DynamoDBProperty("token")]
    //public int DB_token { get; set; }
    //[DynamoDBProperty("xp")]
    //public Dictionary<string, int> DB_xp { get; set; }
    //[DynamoDBProperty("hybrid")]
    //public Dictionary<string, int> DB_hybrid { get; set; }


},

            ConditionExpression = "attribute_not_exists(userName)"



        };




        client.PutItemAsync(request, (result) =>
        {

            if (result.Exception == null)
            {

                StartCoroutine(IRegisterDone(true));

            }
            else
            {


                StartCoroutine(IRegisterFail());

            }



        });

    }



    IEnumerator IRegisterFail()
    {



        Show.GetComponentInChildren<Text>().text = "Player Already Registered...";
        yield return new WaitForSeconds(3);
        Register.SetActive(true);
        Show.SetActive(false);

        yield return 0;

    }


    IEnumerator IRegisterDone(bool value)
    {


        if (!value)
        {
            Show.GetComponentInChildren<Text>().text = "Registration Failed ...";
            yield return new WaitForSeconds(3);
            Register.SetActive(true);

            Show.SetActive(false);

        }
        else
        {

            Show.GetComponentInChildren<Text>().text = "Registration Successfull ,Returning to Login...";
            yield return new WaitForSeconds(7);
            guiHandler.SetActive("Login_Panel");



        }
        yield return 0;
    }




    void CheckLogin()
    {
        player.userName = userName_login.text;
        player.password = password_login.text;

        load_login.GetComponentInChildren<Text>().text = "Logging In ......";
        load_login.SetActive(true);

        CheckLoginHelper(new QueryRequest(), null);

    }

    void CheckLoginHelper(QueryRequest request, Dictionary<string, AttributeValue> lastKeyEvaluated)
    {
        try
        {
            request.TableName = "Players";

            request.KeyConditions = new Dictionary<string, Condition>()
            {
                {
                    "userName",  new Condition()
                    {
                        ComparisonOperator = "EQ",
                        AttributeValueList = new List<AttributeValue>()
                        {
                            new AttributeValue { S = player.userName }
                        }
                    }
                }

            };



            client.QueryAsync(request, (result) =>
            {

                if (result.Response.Items.Count == 0)
                {

                    StartCoroutine(LoginNotFound());

                }
                else
                {
                    if (result.Response.Items[0]["password"].S == player.password)
                    {

                        StartCoroutine(LoginFound());
                        context.LoadAsync<PlayerData>(player.userName, (response) =>
                        {

                            player = response.Result;


                        });
                    }
                    else
                    {

                        StartCoroutine(LoginNotFound());
                    }
                }

            });
        }
        catch (Amazon.Runtime.Internal.HttpErrorResponseException e)
        {

            print(e);
            print("in catch");
            CheckLogin();
           
        }

    }

    void TestHelper(QueryRequest request, Dictionary<string, AttributeValue> lastKeyEvaluated)
    {

        request.TableName = "Players";

        request.KeyConditions = new Dictionary<string, Condition>()
            {
                {
                    "userName",  new Condition()
                    {
                        ComparisonOperator = "EQ",
                        AttributeValueList = new List<AttributeValue>()
                        {
                            new AttributeValue { S = player.userName }
                        }
                    }
                }

            };



        client.QueryAsync(request, (result) =>
        {



            if (result.Exception != null)
            {

                StartCoroutine(IRegisterFail());

            }
            else
            {


                RegisterComplete(new QueryRequest(), null);

            }


        });


    }


    public void RegisterComplete(QueryRequest request, Dictionary<string, AttributeValue> lastKeyEvaluated)
    {


        request.TableName = "Players";

        request.KeyConditions = new Dictionary<string, Condition>()
            {
                {
                    "userName",  new Condition()
                    {
                        ComparisonOperator = "EQ",
                        AttributeValueList = new List<AttributeValue>()
                        {
                            new AttributeValue { S = "null" }
                        }
                    }
                }

            };




        client.QueryAsync(request, (result) =>
        {

            defaultResponse = result.Response;
            CheckItemSample();

        });
    }




    IEnumerator LoginFound()
    {
        isLoggedIn = true;

        load_login.GetComponentInChildren<Text>().text = "Login Successful , Retrieving Player Data.......";

        //context.LoadAsync<PlayerData>("null", (result) =>
        //{

        //    player = result.Result;


        //});


        yield return new WaitForSeconds(8);

        load_login.SetActive(false);

        guiHandler.SetActive("Menu_Panel");

        guiHandler.LoadLoginData();

        profileBar.SetActive(true);

        if (!loginAttempts.Equals(null))
        {
            loginAttempts += 1;
        }

        yield return null;


        yield return 0;

    }


    IEnumerator LoginNotFound()
    {
        loginExceptions += 1;
        load_login.GetComponentInChildren<Text>().text = "Username or Password Incorrect .......";

        yield return new WaitForSeconds(4);

        load_login.SetActive(false);


        yield return 0;

    }




    private void PrintItem(Dictionary<string, AttributeValue> attributeList)
    {
        foreach (var kvp in attributeList)
        {
            string attributeName = kvp.Key;
            AttributeValue value = kvp.Value;



        }

    }




    public void LoginManual()
    {


        CheckLogin();



    }


    public void ForgetPassword()
    {

        player.userName = userName_login.text;

        load_login.GetComponentInChildren<Text>().text = "Sending Email ......";
        load_login.SetActive(true);

        ForgetCheck();



    }

    void ForgetCheck()
    {


        ForgetCheckHelper(new QueryRequest(), null);
    }

    void ForgetCheckHelper(QueryRequest request, Dictionary<string, AttributeValue> lastKeyEvaluated)
    {
        request.TableName = "Players";

        request.KeyConditions = new Dictionary<string, Condition>()
            {
                {
                    "userName",  new Condition()
                    {
                        ComparisonOperator = "EQ",
                        AttributeValueList = new List<AttributeValue>()
                        {
                            new AttributeValue { S = player.userName }
                        }
                    }
                }

            };



        client.QueryAsync(request, (result) =>
        {

            if (result.Response.Items.Count == 0)
            {

                StartCoroutine(ForgetFail());

            }
            else
            {
                MailMessage message = new MailMessage();

                message.To.Add(result.Response.Items[0]["email"].S);


                message.Subject = "Password Recovery";

                message.Body = "PASSWORD For UserName " + player.userName + " :<br> " + result.Response.Items[0]["password"].S;

                message.IsBodyHtml = true;


                message.From = new MailAddress("saad.khan@adamjeegroup.com", "saad");


                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);

                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                smtp.EnableSsl = true;

                ServicePointManager.ServerCertificateValidationCallback =
                        delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                        { return true; };



                smtp.UseDefaultCredentials = false;



                smtp.Credentials = new NetworkCredential("saad.khan@adamjeegroup.com", "linpus#4") as ICredentialsByHost;

                smtp.Timeout = 20000;


                smtp.Send(message);

                StartCoroutine(ForgetSucceed());
            }

        });

    }




    IEnumerator ForgetFail()
    {

        load_login.GetComponentInChildren<Text>().text = "Username Not Found .......";

        yield return new WaitForSeconds(4);

        load_login.SetActive(false);



        yield return 0;

    }


    IEnumerator ForgetSucceed()
    {
        load_login.GetComponentInChildren<Text>().text = "Email Sent.......";

        yield return new WaitForSeconds(4);

        load_login.SetActive(false);


        yield return 0;

    }



    public void SavePlayerData()
    {

        context.SaveAsync<PlayerData>(player, (result) =>
        {




        });

    }

    public void LoadCardsAgain()
    {



        guiHandler.LoadCardsAgain();




    }

    public void LoadCurrencyAgain()
    {


        shardText.text = player.DB_currency.ToString();

    }


    public void LoadTokenAgain()
    {

        tokenText.text =player.DB_token.ToString();


    }


    public void LoadHybridAgain()
    {




    }

    public void LoadStatsAgain()
    {





    }

    public void LoadArmorAgain()
    {



    }

    public void LoadChestAgain()
    {
        chestNumbers[0].text = player.DB_chests["wooden"].ToString();
        chestNumbers[1].text = player.DB_chests["bronze"].ToString();
        chestNumbers[2].text = player.DB_chests["silver"].ToString();
        chestNumbers[3].text = player.DB_chests["gold"].ToString();
        chestNumbers[4].text = player.DB_chests["immortal"].ToString();


    }


    #endregion

    #region Interfaces

    public void BatchGetItemAsync(Dictionary<string, KeysAndAttributes> requestItems, ReturnConsumedCapacity returnConsumedCapacity, AmazonServiceCallback<BatchGetItemRequest, BatchGetItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).BatchGetItemAsync(requestItems, returnConsumedCapacity, callback, options);
    }

    public void BatchGetItemAsync(Dictionary<string, KeysAndAttributes> requestItems, AmazonServiceCallback<BatchGetItemRequest, BatchGetItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).BatchGetItemAsync(requestItems, callback, options);
    }

    public void BatchGetItemAsync(BatchGetItemRequest request, AmazonServiceCallback<BatchGetItemRequest, BatchGetItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).BatchGetItemAsync(request, callback, options);
    }

    public void BatchWriteItemAsync(Dictionary<string, List<WriteRequest>> requestItems, AmazonServiceCallback<BatchWriteItemRequest, BatchWriteItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).BatchWriteItemAsync(requestItems, callback, options);
    }

    public void BatchWriteItemAsync(BatchWriteItemRequest request, AmazonServiceCallback<BatchWriteItemRequest, BatchWriteItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).BatchWriteItemAsync(request, callback, options);
    }

    public void CreateTableAsync(string tableName, List<KeySchemaElement> keySchema, List<AttributeDefinition> attributeDefinitions, ProvisionedThroughput provisionedThroughput, AmazonServiceCallback<CreateTableRequest, CreateTableResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).CreateTableAsync(tableName, keySchema, attributeDefinitions, provisionedThroughput, callback, options);
    }

    public void CreateTableAsync(CreateTableRequest request, AmazonServiceCallback<CreateTableRequest, CreateTableResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).CreateTableAsync(request, callback, options);
    }

    public void DeleteItemAsync(string tableName, Dictionary<string, AttributeValue> key, AmazonServiceCallback<DeleteItemRequest, DeleteItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).DeleteItemAsync(tableName, key, callback, options);
    }

    public void DeleteItemAsync(string tableName, Dictionary<string, AttributeValue> key, ReturnValue returnValues, AmazonServiceCallback<DeleteItemRequest, DeleteItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).DeleteItemAsync(tableName, key, returnValues, callback, options);
    }

    public void DeleteItemAsync(DeleteItemRequest request, AmazonServiceCallback<DeleteItemRequest, DeleteItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).DeleteItemAsync(request, callback, options);
    }

    public void DeleteTableAsync(string tableName, AmazonServiceCallback<DeleteTableRequest, DeleteTableResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).DeleteTableAsync(tableName, callback, options);
    }

    public void DeleteTableAsync(DeleteTableRequest request, AmazonServiceCallback<DeleteTableRequest, DeleteTableResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).DeleteTableAsync(request, callback, options);
    }

    public void DescribeLimitsAsync(DescribeLimitsRequest request, AmazonServiceCallback<DescribeLimitsRequest, DescribeLimitsResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).DescribeLimitsAsync(request, callback, options);
    }

    public void DescribeTableAsync(string tableName, AmazonServiceCallback<DescribeTableRequest, DescribeTableResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).DescribeTableAsync(tableName, callback, options);
    }

    public void DescribeTableAsync(DescribeTableRequest request, AmazonServiceCallback<DescribeTableRequest, DescribeTableResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).DescribeTableAsync(request, callback, options);
    }

    public void GetItemAsync(string tableName, Dictionary<string, AttributeValue> key, AmazonServiceCallback<GetItemRequest, GetItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).GetItemAsync(tableName, key, callback, options);
    }

    public void GetItemAsync(string tableName, Dictionary<string, AttributeValue> key, bool consistentRead, AmazonServiceCallback<GetItemRequest, GetItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).GetItemAsync(tableName, key, consistentRead, callback, options);
    }

    public void GetItemAsync(GetItemRequest request, AmazonServiceCallback<GetItemRequest, GetItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).GetItemAsync(request, callback, options);
    }

    public void ListTablesAsync(string exclusiveStartTableName, AmazonServiceCallback<ListTablesRequest, ListTablesResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).ListTablesAsync(exclusiveStartTableName, callback, options);
    }

    public void ListTablesAsync(string exclusiveStartTableName, int limit, AmazonServiceCallback<ListTablesRequest, ListTablesResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).ListTablesAsync(exclusiveStartTableName, limit, callback, options);
    }

    public void ListTablesAsync(int limit, AmazonServiceCallback<ListTablesRequest, ListTablesResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).ListTablesAsync(limit, callback, options);
    }

    public void ListTablesAsync(ListTablesRequest request, AmazonServiceCallback<ListTablesRequest, ListTablesResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).ListTablesAsync(request, callback, options);
    }

    public void PutItemAsync(string tableName, Dictionary<string, AttributeValue> item, AmazonServiceCallback<PutItemRequest, PutItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).PutItemAsync(tableName, item, callback, options);
    }

    public void PutItemAsync(string tableName, Dictionary<string, AttributeValue> item, ReturnValue returnValues, AmazonServiceCallback<PutItemRequest, PutItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).PutItemAsync(tableName, item, returnValues, callback, options);
    }

    public void PutItemAsync(PutItemRequest request, AmazonServiceCallback<PutItemRequest, PutItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).PutItemAsync(request, callback, options);
    }

    public void QueryAsync(QueryRequest request, AmazonServiceCallback<QueryRequest, QueryResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).QueryAsync(request, callback, options);
    }

    public void ScanAsync(string tableName, List<string> attributesToGet, AmazonServiceCallback<ScanRequest, ScanResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).ScanAsync(tableName, attributesToGet, callback, options);
    }

    public void ScanAsync(string tableName, Dictionary<string, Condition> scanFilter, AmazonServiceCallback<ScanRequest, ScanResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).ScanAsync(tableName, scanFilter, callback, options);
    }

    public void ScanAsync(string tableName, List<string> attributesToGet, Dictionary<string, Condition> scanFilter, AmazonServiceCallback<ScanRequest, ScanResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).ScanAsync(tableName, attributesToGet, scanFilter, callback, options);
    }

    public void ScanAsync(ScanRequest request, AmazonServiceCallback<ScanRequest, ScanResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).ScanAsync(request, callback, options);
    }

    public void UpdateItemAsync(string tableName, Dictionary<string, AttributeValue> key, Dictionary<string, AttributeValueUpdate> attributeUpdates, AmazonServiceCallback<UpdateItemRequest, UpdateItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).UpdateItemAsync(tableName, key, attributeUpdates, callback, options);
    }

    public void UpdateItemAsync(string tableName, Dictionary<string, AttributeValue> key, Dictionary<string, AttributeValueUpdate> attributeUpdates, ReturnValue returnValues, AmazonServiceCallback<UpdateItemRequest, UpdateItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).UpdateItemAsync(tableName, key, attributeUpdates, returnValues, callback, options);
    }

    public void UpdateItemAsync(UpdateItemRequest request, AmazonServiceCallback<UpdateItemRequest, UpdateItemResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).UpdateItemAsync(request, callback, options);
    }

    public void UpdateTableAsync(string tableName, ProvisionedThroughput provisionedThroughput, AmazonServiceCallback<UpdateTableRequest, UpdateTableResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).UpdateTableAsync(tableName, provisionedThroughput, callback, options);
    }

    public void UpdateTableAsync(UpdateTableRequest request, AmazonServiceCallback<UpdateTableRequest, UpdateTableResponse> callback, AsyncOptions options = null)
    {
        ((IAmazonDynamoDB)client).UpdateTableAsync(request, callback, options);
    }

    public void Dispose()
    {
        ((IAmazonDynamoDB)client).Dispose();
    }


    #endregion


}