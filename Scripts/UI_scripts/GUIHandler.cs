using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
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
using System;
using System.Text.RegularExpressions;
using SimpleJSON;

public static class StringExtensions
{
    public static bool Contains(this String str, String substring,
                                StringComparison comp)
    {
        if (substring == null)
            throw new ArgumentNullException("substring",
                                            "substring cannot be null.");
        else if (!Enum.IsDefined(typeof(StringComparison), comp))
            throw new ArgumentException("comp is not a member of StringComparison",
                                        "comp");

        return str.IndexOf(substring, comp) >= 0;
    }
}


public class GUIHandler : MonoBehaviour, IAmazonDynamoDB
{
    GameManager manager;

    #region Variables
    public bool DataLoaded = false;
    public List<string> profileNames = new List<string>();
    public string current_panel = "Menu_Panel";
    public string previous_panel = "Menu_Panel";
    private GameObject Canvas;
    public bool loggedInSuccessfull = false;
    public GameObject selectedPanel;
    public Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();
    public Camera renderCamera;
    public UnityEvent eventCurrent = new UnityEvent();
    public Event mouse = new Event();
    public PointerEventData pointerData = new PointerEventData(EventSystem.current);
    public BaseEventData eventData = new BaseEventData(EventSystem.current);
    public EventSystem event_current;



    #endregion



    #region  Login_Panel variables
    [Header("---------Login Variables---------")]
    public Button login_button;
    public InputField password;
    public InputField userName_login;
    PlayerPrefs userName_saved;
    PlayerPrefs userPassword_saved;
    public Toggle rememberMe;
    [Space(10)]
    #endregion



    #region Register_Panel variables
    [Header("---------Register Variables---------")]

    public GameObject matchError;
    public GameObject Show;
    public GameObject Register;

    public InputField email;
    public InputField userName;
    public InputField pass1;
    public InputField pass2;

    public Button regButton;

    #endregion



    #region  Menu_Panel variables
    [Space(10)]
    [Header("---------Menu Variables---------")]
    public Button profile_button;
    public Button exit_button;

    #endregion



    #region  MatchMaking_Panel variables
    [Header("-------MatchMaking Variables---------")]
    public Transform army;
    public Button[] armyButtons = new Button[5];
    public Button preArmy;
    public static bool armySelected;
    public static string armyName;
    public Transform deckTitle;
    public Button[] deckButtons = new Button[6];
    public Button preDeck;
    public static bool deckSelected;
    public static string deckName;

    public Sprite shineArmy;
    public Sprite normalArmy;
    public Text warning;
    #endregion



    #region  Profile_Panel variables
    [Space(10)]
    [Header("---------Profile Variables---------")]
    public Button deck_button;
    public Button hybridArmy_button;
    public Button backMenu_button;
    [Space(10)]
    #endregion



    #region  DeckALL_Panel variables

    [Header("---------Deck Variables---------")]

    public Transform cardHandler;
    public Transform search;
    public InputField searchbar;
    public Sprite[] pix;
    public Transform deck;
    public Transform deckPanelSelection;
    public string currentLoadout;

    public Transform playerDeckHandler;
    public Transform LoadOut1;
    public Transform LoadOut2;
    public Transform LoadOut3;
    public Transform[] playerDecks;

    public List<string> cardList = new List<string>();
    public string method;



    public Text selectText;
    public Text loadoutCustomName;
    public InputField editName;
    public Text[] buttonText;



    #endregion



    #region  HybridArmy_Panel variables
    [Header("--------HybridArmy Variables----------")]

    public Sprite shine;
    public Sprite normal;

    //tier1
    public Transform Tier1;
    public Button[] tier1units = new Button[4];
    public Button preTier1;
    public static bool tier1Selected;
    public static string tier1Name = "";
    //tier2
    public Transform Tier2;
    public Button[] tier2units = new Button[4];
    public Button preTier2;
    public static bool tier2Selected;
    public static string tier2Name = "";
    //tier3
    public Transform Tier3;
    public Button[] tier3units = new Button[4];
    public Button preTier3;
    public static bool tier3Selected;
    public static string tier3Name = "";
    //tier4
    public Transform Tier4;
    public Button[] tier4units = new Button[4];
    public Button preTier4;
    public static bool tier4Selected;
    public static string tier4Name = "";
    //tier5
    public Transform Tier5;
    public Button[] tier5units = new Button[4];
    public Button preTier5;
    public static bool tier5Selected;
    public static string tier5Name = "";
    //tier6
    public Transform Tier6;
    public Button[] tier6units = new Button[4];
    public Button preTier6;
    public static bool tier6Selected;
    public static string tier6Name = "";
    //--end of tiers

    #endregion


  


    #region Unity Functions





    public void Awake()
    {
        manager = GameManager.Instance;

        //aspectRatio.options[0] = aspectRatio.options.SingleOrDefault(x=> x.text == "4:3");

        playerDecks = new Transform[3] { LoadOut1, LoadOut2, LoadOut3 };


        Canvas = GameObject.Find("/Canvas");

        foreach (Transform child in Canvas.transform)
        {

            if (child.name != "Background" && child.name != "Profile_Bar" && child.name != "Bottom_Panel")
            {
                panels.Add(child.name, child.transform.gameObject);
            }
        }



        //if (!loggedInSuccessfull)
        //{
        //    SetActive("Login_Panel");
        //    //selectedPanel = panels["Login_Panel"].gameObject;
        //}
        //else
        //{
        //    SetActive("Menu_Panel");
        //    // LoadCards();
        //}


        //LoadCards();

        
        //tier1
        int t1 = 0;
        foreach (Transform item in Tier1)
        {

            Button b;
            tier1units[t1] = item.gameObject.GetComponent<Button>() as Button;
            b = tier1units[t1];
            t1 = t1 + 1;
            b.onClick.AddListener(() => HighlightTier1(b));


        }
        //tier2
        int t2 = 0;
        foreach (Transform item in Tier2)
        {

            Button b;
            tier2units[t2] = item.gameObject.GetComponent<Button>() as Button;
            b = tier2units[t2];
            t2 = t2 + 1;
            b.onClick.AddListener(() => HighlightTier2(b));


        }
        //tier3
        int t3 = 0;
        foreach (Transform item in Tier3)
        {

            Button b;
            tier3units[t3] = item.gameObject.GetComponent<Button>() as Button;
            b = tier3units[t3];
            t3 = t3 + 1;
            b.onClick.AddListener(() => HighlightTier3(b));


        }
        //tier4
        int t4 = 0;
        foreach (Transform item in Tier4)
        {

            Button b;
            tier4units[t4] = item.gameObject.GetComponent<Button>() as Button;
            b = tier4units[t4];
            t4 = t4 + 1;
            b.onClick.AddListener(() => HighlightTier4(b));


        }
        //tier5
        int t5 = 0;
        foreach (Transform item in Tier5)
        {

            Button b;
            tier5units[t5] = item.gameObject.GetComponent<Button>() as Button;
            b = tier5units[t5];
            t5 = t5 + 1;
            b.onClick.AddListener(() => HighlightTier5(b));


        }
        //tier6
        int t6 = 0;
        foreach (Transform item in Tier6)
        {

            Button b;
            tier6units[t6] = item.gameObject.GetComponent<Button>() as Button;
            b = tier6units[t6];
            t6 = t6 + 1;
            b.onClick.AddListener(() => HighlightTier6(b));


        }
        //end of tiers

        //MatchMaking data 

        int m = 0;
        foreach (Transform item in army)
        {
            Button b;
            armyButtons[m] = item.gameObject.GetComponent<Button>() as Button;
            b = armyButtons[m];
            m = m + 1;
            b.onClick.AddListener(() => HighlightArmy(b));
        }

        int d = 0;
        foreach (Transform item in deckTitle)
        {
            Button b;
            deckButtons[d] = item.gameObject.GetComponent<Button>() as Button;
            b = deckButtons[d];
            d = d + 1;
            b.onClick.AddListener(() => HighlightDeck(b));
        }


       // GetBaseHero();

    }


    public void Start()
    {

        userName_login.text = PlayerPrefs.GetString("Username");
        password.text = PlayerPrefs.GetString("Password");


        

    }



    void CheckLogin()
    {
        
        CheckLoginHelper(new QueryRequest(), null);

    }

    void CheckLoginHelper(QueryRequest request, Dictionary<string, AttributeValue> lastKeyEvaluated)
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



           GameManager.Instance.client.QueryAsync(request, (result) =>
            {

                print(result.Response.Items[0]["cards"].M["Rage"].N);

            });

        }

    public void Update()
    {




        if (selectedPanel.name == "Login_Panel"|| selectedPanel == null)
        {


            if (Input.GetKey(KeyCode.Tab))
            {



                EventSystem.current.SetSelectedGameObject(password.gameObject);


            }






        }





        if (Input.GetKeyUp(KeyCode.Escape))
        {



            if (current_panel == null || current_panel == "Menu_Panel")
            {




            }
            else
            {

                if (profileNames.Contains(current_panel))
                {


                    SetActive(previous_panel);

                    previous_panel = "Menu_Panel";

                }
                else
                {

                    SetActive(previous_panel);
                }
                //GameObject.Find(current_panel).SetActive(false);
                //GameObject.Find(previous_panel).SetActive(true);



            }




        }







        if (Input.GetMouseButtonDown(0))
        {


            Vector3 value;
            RaycastHit hit;
            Ray raycam = LoadOutCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(raycam, out hit, 100))
            {


                if (hit.collider.tag == "Player")
                {


                    keepRotating = true;



                }

            }

        }


        if (Input.GetMouseButtonUp(0))
        {

            keepRotating = false;

        }


        RotateCharacter(keepRotating);

    }

    void RotateCharacter(bool keepRotating)
    {

        if (keepRotating)
        {
            Character.transform.Rotate(0, -Input.GetAxis("Mouse X") * 15, 0);
        }

    }


    void OnEnable()
    {

        Draggable.countCards += AddCards;
        SelectItem.OnArmorSelected += ItemClicked;
        SelectItem.OnArmorShowSelected += ItemShowDesc;

    }

    void OnDisable()
    {



        Draggable.countCards -= AddCards;
        SelectItem.OnArmorSelected -= ItemClicked;
        SelectItem.OnArmorShowSelected -= ItemShowDesc;
    }

    void OnUpdate() {


        if (selectedPanel.name == "SlotMachine_Panel") {

            GameManager.Instance.mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        }

    }

    public void ChangeScene(string sceneName)
    {

        if (preArmy == null || preDeck == null)
        {

            warning.gameObject.SetActive(true);

        }
        else
        {

            SceneManager.LoadScene(sceneName);

        }
    }

    #endregion



    #region GUI Functions
    public void SetActive(string name)
    {
        previous_panel = current_panel;
        current_panel = name;

        if (name == "SlotMachine_Panel")
        {


            manager.mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;

        }
        else {


            manager.mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        }


        foreach (var item in panels)
        {
            if (item.Key != name)
            {

                item.Value.SetActive(false);

            }
            else
            {

                item.Value.SetActive(true);
                selectedPanel = item.Value.gameObject;
            }
        }

    }

    public void SetUnActive(string name)
    {

        selectedPanel.gameObject.SetActive(false);
        selectedPanel = panels[name].gameObject;
        selectedPanel.SetActive(true);


    }

    #endregion



    #region Deck Functions


    public void SearchCard() {

        
      
        if (searchbar.text == null)
        {

            foreach (Transform child in cardHandler)
            {
               
                child.gameObject.SetActive(true);
            }




        }
        else
        {

            foreach (Transform child in cardHandler)
            {
              
                child.gameObject.SetActive(false);
            }


            StringComparison comp = StringComparison.OrdinalIgnoreCase;
            for (int i = 0; i < cardList.Count; i++)
            {
                string name = cardList[i];
                if (name.Contains(searchbar.text, comp))
                {

                    cardHandler.transform.Find(name).gameObject.SetActive(true);

                }
            }


        }

    }

    public void LoadCards()
    {


        pix = Resources.LoadAll<Sprite>("cardImages") as Sprite[];


        for (int i = 0; i < pix.Length; i++)
        {
            //if (GameManager.Instance.LoadOut1.Contains(pix[i].name) || GameManager.Instance.LoadOut2.Contains(pix[i].name) || GameManager.Instance.LoadOut3.Contains(pix[i].name))
            //{



            //}
            //else
            //{
                GameObject image = Instantiate(Resources.Load("cardNewPrefab")) as GameObject;
            //if (GameManager.Instance.player.DB_cards[(pix[i].name)] == 0)
            //{


            //}
            //else
            //{
                string name = pix[i].name;
                cardList.Add(name);
                image.GetComponent<Image>().sprite = pix[i] as Sprite;
                image.transform.gameObject.name = name;
                
                int number = GameManager.Instance.player.DB_cards[name];
                image.GetComponentInChildren<Text>().text = number.ToString();
                image.transform.SetParent(cardHandler.transform, false);
                //}
            //}
        }

        DataLoaded = false;

    }


    public void LoadCardsAgain()
    {

        foreach (Transform t in cardHandler)
        {
            if (!cardList.Contains(t.name))
            {
                GameObject image = Instantiate(Resources.Load("cardNewPrefab")) as GameObject;
                if (GameManager.Instance.player.DB_cards[t.name] == 0)
                {


                }
                else
                {


                    image.GetComponent<Image>().sprite = Resources.Load<Sprite>("cardImages/" + t.name) as Sprite;
                    image.transform.gameObject.name = name;

                    int number = GameManager.Instance.player.DB_cards[name];
                    image.GetComponentInChildren<Text>().text = number.ToString();
                    image.transform.SetParent(cardHandler.transform, false);
                }
            }
        }
    }


    public void AddCards()
    {



        StartCoroutine(CAddCards());

    }


    IEnumerator CAddCards()
    {
        if (currentLoadout == "LoadOut1")
        {

            GameManager.Instance.LoadOut1.Clear();
        }
        else if (currentLoadout == "LoadOut2")
        {

            GameManager.Instance.LoadOut2.Clear();

        }
        else if (currentLoadout == "LoadOut3")
        {

            GameManager.Instance.LoadOut3.Clear();

        }


        if (currentLoadout == "LoadOut1")
        {

            if (LoadOut1.transform.childCount == 0) { }
            else
            {

                foreach (Transform item in LoadOut1.transform)
                {


                    yield return 0;

                    GameManager.Instance.LoadOut1.Add(item.name);

                }
            }
        }
        else if (currentLoadout == "LoadOut2")
        {

            if (LoadOut2.transform.childCount == 0) { }
            else
            {

                foreach (Transform item in LoadOut2.transform)
                {


                    yield return 0;
                    Debug.Log(item.name);
                    GameManager.Instance.LoadOut2.Add(item.name);

                }
            }

        }
        if (currentLoadout == "LoadOut3")
        {

            if (LoadOut3.transform.childCount == 0) { }
            else
            {

                foreach (Transform item in LoadOut3.transform)
                {


                    yield return 0;
                    Debug.Log(item.name);
                    GameManager.Instance.LoadOut3.Add(item.name);

                }
            }
        }


        if (currentLoadout == "LoadOut1")
        {
            if (GameManager.Instance.LoadOut1.Count == 8)
            {
                yield return 0;

            }
            else
            {

            }
        }
        if (currentLoadout == "LoadOut2")
        {
            if (GameManager.Instance.LoadOut2.Count == 8)
            {
                yield return 0;

            }
            else
            {

            }
        }
        if (currentLoadout == "LoadOut3")
        {
            if (GameManager.Instance.LoadOut3.Count == 8)
            {
                yield return 0;

            }
            else
            {

            }
        }

    }



    public void ResetAll()
    {

        //StartCoroutine(CResetAll());

        switch (currentLoadout)
        {
            case "LoadOut1":
                manager.LoadOut1.Clear();
                manager.player.DB_loadout1 = manager.LoadOut1;
                manager.SavePlayerData();


                foreach (Transform item in deckPanelSelection)
                {
                    Destroy(item.gameObject);
                }

                break;

            default:
                break;
        }


    }


    IEnumerator CResetAll()
    {



        yield return 0;
        if (currentLoadout == "LoadOut1")
        {
            while (LoadOut1.transform.childCount != 0)
            {
                yield return 0;

                foreach (Transform item in LoadOut1.transform)
                {

                    yield return 0;
                    item.transform.parent = null;
                    item.transform.SetParent(cardHandler, true);

                    yield return 0;

                }

            }
            GameManager.Instance.LoadOut1.Clear();
        }

        else if (currentLoadout == "LoadOut2")
        {
            while (LoadOut2.transform.childCount != 0)
            {
                yield return 0;

                foreach (Transform item in LoadOut2.transform)
                {

                    yield return 0;
                    item.transform.parent = null;
                    item.transform.SetParent(cardHandler, true);

                    yield return 0;

                }

            }
            GameManager.Instance.LoadOut2.Clear();
        }

        else if (currentLoadout == "LoadOut3")
        {
            while (LoadOut3.transform.childCount != 0)
            {
                yield return 0;

                foreach (Transform item in LoadOut3.transform)
                {

                    yield return 0;
                    item.transform.parent = null;
                    item.transform.SetParent(cardHandler, true);

                    yield return 0;

                }

            }
            GameManager.Instance.LoadOut3.Clear();
        }
        yield return 0;
    }



    public void SelectedPanel()
    {




        for (int i = 0; i < playerDecks.Length; i++)
        {
            if (playerDecks[i].gameObject.name == EventSystem.current.currentSelectedGameObject.name)
            {

                playerDecks[i].transform.parent.gameObject.SetActive(true);
                currentLoadout = playerDecks[i].gameObject.name;
            }
            else
            {

                playerDecks[i].transform.parent.gameObject.SetActive(false);
            }
        }

    }




    public void SaveDecks()
    {

        print("zzz");
        StartCoroutine(SaveDecksHelper());


    }



    IEnumerator SaveDecksHelper()
    {


        PlayerData playerRetrieved = null;
        GameManager.Instance.context.LoadAsync<PlayerData>(GameManager.Instance.player.userName, (result) =>
        {
            if (result.Exception == null)
            {
                playerRetrieved = result.Result as PlayerData;

                if (currentLoadout == "LoadOut1")
                {
                    if (GameManager.Instance.LoadOut1.Count == 0)
                    {

                        playerRetrieved.DB_loadout1 = new List<string> { "null" };
                    }
                    else
                    {

                        playerRetrieved.DB_loadout1 = new List<string>();
                        playerRetrieved.DB_loadout1 = GameManager.Instance.LoadOut1;

                    }
                }
                else if (currentLoadout == "LoadOut2")
                {
                    if (GameManager.Instance.LoadOut2.Count == 0)
                    {

                        playerRetrieved.DB_loadout2 = new List<string> { "null" };
                    }
                    else
                    {

                        playerRetrieved.DB_loadout2 = new List<string>();
                        playerRetrieved.DB_loadout2 = GameManager.Instance.LoadOut2;

                    }
                }
                else if (currentLoadout == "LoadOut3")
                {
                    if (GameManager.Instance.LoadOut3.Count == 0)
                    {

                        playerRetrieved.DB_loadout3 = new List<string> { "null" };
                    }
                    else
                    {

                        playerRetrieved.DB_loadout3 = new List<string>();
                        playerRetrieved.DB_loadout3 = GameManager.Instance.LoadOut3;

                    }
                }
                GameManager.Instance.context.SaveAsync<PlayerData>(playerRetrieved, (res) =>
            {
                if (res.Exception == null)
                    print("loadout updated");
            });
            }
        });
        yield return null;

        print("zzz");


        yield return 0;
    }



    public void ShowCards(string name) {


        selectText.gameObject.SetActive(false);
        deck.gameObject.SetActive(true);
        loadoutCustomName.text = "";
        switch (name)
        {

            case "LoadOut1" :
                currentLoadout = "LoadOut1";

                foreach (Transform item in deckPanelSelection)
                {
                    Destroy(item.gameObject);
                }

                if (manager.LoadOut1.Contains("null") || manager.LoadOut1 == null) {  }
                else
                {
                  
                    for (int i = 0; i < manager.LoadOut1.Count; i++)
                    {

                        var pix = Resources.Load<Sprite>("cardImages/" + GameManager.Instance.LoadOut1[i]) as Sprite;
                        GameObject image = Instantiate(Resources.Load("cardNewPrefabWC")) as GameObject;
                        image.GetComponent<Image>().sprite = pix as Sprite;
                        image.transform.gameObject.name = pix.name;
                        image.transform.SetParent(deckPanelSelection, false);
                    }
                }
                loadoutCustomName.text = manager.player.DB_loadOutNames[currentLoadout];

                break;

            case "LoadOut2":

                currentLoadout = "LoadOut2";

                foreach (Transform item in deckPanelSelection)
                {
                    Destroy(item.gameObject);
                }
                if (manager.LoadOut2.Contains("null") || manager.LoadOut2 == null) { }
                else
                {
                    for (int i = 0; i < manager.LoadOut2.Count; i++)
                    {

                        var pix = Resources.Load<Sprite>("cardImages/" + manager.LoadOut2[i]) as Sprite;
                        GameObject image = Instantiate(Resources.Load("cardNewPrefabWC")) as GameObject;
                        image.GetComponent<Image>().sprite = pix as Sprite;
                        image.transform.gameObject.name = pix.name;
                        image.transform.SetParent(deckPanelSelection, false);
                    }
                }
                loadoutCustomName.text = manager.player.DB_loadOutNames[currentLoadout];

                break;

            case "LoadOut3":

                currentLoadout = "LoadOut3";

                foreach (Transform item in deckPanelSelection)
                {
                    Destroy(item.gameObject);
                }
                if (manager.LoadOut3.Contains("null") || manager.LoadOut3 == null) { }
                else
                {
                    for (int i = 0; i < manager.LoadOut3.Count; i++)
                    {

                        var pix = Resources.Load<Sprite>("cardImages/" + manager.LoadOut3[i]) as Sprite;
                        GameObject image = Instantiate(Resources.Load("cardNewPrefabWC")) as GameObject;
                        image.GetComponent<Image>().sprite = pix as Sprite;
                        image.transform.gameObject.name = pix.name;
                        image.transform.SetParent(deckPanelSelection, false);
                    }
                }
                loadoutCustomName.text = manager.player.DB_loadOutNames[currentLoadout];
                break;

            default:
                break;
        }




    }


    public void ChangeName() {

        //print("hi");
        if (currentLoadout.Contains("Default1") || currentLoadout.Contains("Default2") || currentLoadout.Contains("Default3")) { }
        else
        {

            editName.gameObject.SetActive(true);
            loadoutCustomName.gameObject.SetActive(false);
        }
    }

    public void StartChangeName() {

        switch (currentLoadout)
        {

            case "LoadOut1":

                manager.player.DB_loadOutNames[currentLoadout] = editName.text;
                buttonText[0].text = editName.text;
                loadoutCustomName.text = editName.text;
                loadoutCustomName.gameObject.SetActive(true);
                editName.gameObject.SetActive(false);
                manager.SavePlayerData();
                break;

            case "LoadOut2":

                manager.player.DB_loadOutNames[currentLoadout] = editName.text;
                buttonText[1].text = editName.text;
                loadoutCustomName.text = editName.text;
                loadoutCustomName.gameObject.SetActive(true);
                editName.gameObject.SetActive(false);
                manager.SavePlayerData();
                break;


            case "LoadOut3":

                manager.player.DB_loadOutNames[currentLoadout] = editName.text;
                buttonText[2].text = editName.text;
                loadoutCustomName.text = editName.text;
                loadoutCustomName.gameObject.SetActive(true);
                editName.gameObject.SetActive(false);
                manager.SavePlayerData();
                break;

            default:
                break;
        }

    }


    public void BackFromDeckBuilding() {


        switch (currentLoadout)
        {

            case "LoadOut1":
                manager.LoadOut1 = manager.gameDeck;
                manager.player.DB_loadout1 = manager.gameDeck;
                ShowCards("LoadOut1");
                SetActive("DeckSelectionPanel");
                current_panel = "LoadOut1";
                manager.SavePlayerData();


                for (int i = 0; i < manager.LoadOut1.Count; i++)
                {

                    var pix = Resources.Load<Sprite>("cardImages/" + manager.LoadOut1[i]) as Sprite;
                    GameObject image = Instantiate(Resources.Load("cardNewPrefabWC")) as GameObject;
                    image.GetComponent<Image>().sprite = pix as Sprite;
                    image.transform.gameObject.name = pix.name;
                    image.transform.SetParent(deckPanelSelection, false);


                }

                break;

            case "LoadOut2":
                manager.LoadOut2 = manager.gameDeck;
                manager.player.DB_loadout2 = manager.gameDeck;
                ShowCards("LoadOut2");
                SetActive("DeckSelectionPanel");
                current_panel = "LoadOut2";
                manager.SavePlayerData();


                for (int i = 0; i < manager.LoadOut2.Count; i++)
                {

                    var pix = Resources.Load<Sprite>("cardImages/" + manager.LoadOut2[i]) as Sprite;
                    GameObject image = Instantiate(Resources.Load("cardNewPrefabWC")) as GameObject;
                    image.GetComponent<Image>().sprite = pix as Sprite;
                    image.transform.gameObject.name = pix.name;
                    image.transform.SetParent(deckPanelSelection, false);


                }


                break;

            case "LoadOut3":
                manager.LoadOut3 = manager.gameDeck;
                manager.player.DB_loadout3 = manager.gameDeck;
                ShowCards("LoadOut3");
                SetActive("DeckSelectionPanel");
                current_panel = "LoadOut3";
                manager.SavePlayerData();

                for (int i = 0; i < manager.LoadOut3.Count; i++)
                {

                    var pix = Resources.Load<Sprite>("cardImages/" + manager.LoadOut3[i]) as Sprite;
                    GameObject image = Instantiate(Resources.Load("cardNewPrefabWC")) as GameObject;
                    image.GetComponent<Image>().sprite = pix as Sprite;
                    image.transform.gameObject.name = pix.name;
                    image.transform.SetParent(deckPanelSelection, false);


                }

                break;

            default:
                break;
        }




    }

    public void EditButton() {

        SetActive("Deck_Panel");


        switch (currentLoadout)
        {
            case "LoadOut1":

                manager.gameDeck = manager.LoadOut1;

                if (manager.LoadOut1.Contains("null") || manager.LoadOut1 == null) { }
                else
                {

                    for (int i = 0; i < manager.LoadOut1.Count; i++)
                    {

                        var pix = Resources.Load<Sprite>("cardImages/" + manager.LoadOut1[i]) as Sprite;
                        GameObject image = Instantiate(Resources.Load("cardNewPrefabWC")) as GameObject;
                        image.GetComponent<Image>().sprite = pix as Sprite;
                        image.transform.gameObject.name = pix.name;
                        image.transform.SetParent(deck, false);


                    }

                }

                break;

            case "LoadOut2":

                manager.gameDeck = manager.LoadOut2;

                if (manager.LoadOut1.Contains("null") || manager.LoadOut1 == null) { }
                else
                {

                    for (int i = 0; i < manager.LoadOut2.Count; i++)
                    {

                        var pix = Resources.Load<Sprite>("cardImages/" + manager.LoadOut2[i]) as Sprite;
                        GameObject image = Instantiate(Resources.Load("cardNewPrefabWC")) as GameObject;
                        image.GetComponent<Image>().sprite = pix as Sprite;
                        image.transform.gameObject.name = pix.name;
                        image.transform.SetParent(deck, false);


                    }


                }
                break;

            case "LoadOut3":

                manager.gameDeck = manager.LoadOut3;


                if (manager.LoadOut1.Contains("null") || manager.LoadOut1 == null) { }
                else
                {
                    for (int i = 0; i < manager.LoadOut3.Count; i++)
                    {

                        var pix = Resources.Load<Sprite>("cardImages/" + manager.LoadOut3[i]) as Sprite;
                        GameObject image = Instantiate(Resources.Load("cardNewPrefabWC")) as GameObject;
                        image.GetComponent<Image>().sprite = pix as Sprite;
                        image.transform.gameObject.name = pix.name;
                        image.transform.SetParent(deck, false);


                    }

                }

                break;

            default:
                break;
        }


    }

    #endregion



    #region Hybrid Army Functions

    //tier1
    public void HighlightTier1(Button b)
    {


        if (!tier1Selected)
        {
            tier1Name = b.name;
            tier1Selected = true;
            b.gameObject.GetComponent<Image>().sprite = shine as Sprite;
            preTier1 = b;
            GameManager.Instance.hybridArmy[0] = (b.name);
            for (int i = 0; i < tier1units.Length; i++)
            {
                if (tier1units[i] != preTier1)
                {
                    tier1units[i].GetComponent<Image>().sprite = normal as Sprite;
                }
            }
            
        }
        else
        {

            if (preTier1.name == b.name)
            {

                GameManager.Instance.hybridArmy[0] = "";
                b.gameObject.GetComponent<Image>().sprite = normal as Sprite;
                tier1Selected = false;
                preTier1 = null;
                tier1Name = "";

            }
            else
            {
                GameManager.Instance.hybridArmy[0] = "";
                tier1Name = b.name;
                tier1Selected = true;
                b.gameObject.GetComponent<Image>().sprite = shine as Sprite;
                preTier1 = b;
                GameManager.Instance.hybridArmy[0] = (b.name);
                for (int i = 0; i < tier1units.Length; i++)
                {
                    if (tier1units[i] != preTier1)
                    {
                        tier1units[i].GetComponent<Image>().sprite = normal as Sprite;
                    }
                }


            }

        }
        print(tier1Selected);
    }


    //tier2
    public void HighlightTier2(Button b)
    {


        if (!tier2Selected)
        {
            tier2Name = b.name;
            tier2Selected = true;
            b.gameObject.GetComponent<Image>().sprite = shine as Sprite;
            preTier2 = b;
            GameManager.Instance.hybridArmy[1] = (b.name);
            for (int i = 0; i < tier2units.Length; i++)
            {
                if (tier2units[i] != preTier2)
                {
                    tier2units[i].GetComponent<Image>().sprite = normal as Sprite;
                }
            }
        }
        else
        {

            if (preTier2.name == b.name)
            {

                GameManager.Instance.hybridArmy[1] = "";
                b.gameObject.GetComponent<Image>().sprite = normal as Sprite;
                tier2Selected = false;
                preTier2 = null;
                tier1Name = "";

            }
            else
            {
                GameManager.Instance.hybridArmy[1] = "";
                tier2Name = b.name;
                tier2Selected = true;
                b.gameObject.GetComponent<Image>().sprite = shine as Sprite;
                preTier2 = b;
                GameManager.Instance.hybridArmy[1] = (b.name);
                for (int i = 0; i < tier2units.Length; i++)
                {
                    if (tier2units[i] != preTier2)
                    {
                        tier2units[i].GetComponent<Image>().sprite = normal as Sprite;
                    }
                }


            }

        }

    }


    //tier3
    public void HighlightTier3(Button b)
    {


        if (!tier3Selected)
        {
            tier3Name = b.name;
            tier3Selected = true;
            b.gameObject.GetComponent<Image>().sprite = shine as Sprite;
            preTier3 = b;
            GameManager.Instance.hybridArmy[2] = (b.name);
            for (int i = 0; i < tier3units.Length; i++)
            {
                if (tier3units[i] != preTier3)
                {
                    tier3units[i].GetComponent<Image>().sprite = normal as Sprite;
                }
            }
        }
        else
        {

            if (preTier3.name == b.name)
            {

                GameManager.Instance.hybridArmy[2] = "";
                b.gameObject.GetComponent<Image>().sprite = normal as Sprite;
                tier3Selected = false;
                preTier3 = null;
                tier3Name = "";

            }
            else
            {
                GameManager.Instance.hybridArmy[2] = "";
                tier3Name = b.name;
                tier3Selected = true;
                b.gameObject.GetComponent<Image>().sprite = shine as Sprite;
                preTier3 = b;
                GameManager.Instance.hybridArmy[2] = (b.name);
                for (int i = 0; i < tier3units.Length; i++)
                {
                    if (tier3units[i] != preTier3)
                    {
                        tier3units[i].GetComponent<Image>().sprite = normal as Sprite;
                    }
                }


            }

        }

    }



    //tier4
    public void HighlightTier4(Button b)
    {


        if (!tier4Selected)
        {
            tier4Name = b.name;
            tier4Selected = true;
            b.gameObject.GetComponent<Image>().sprite = shine as Sprite;
            preTier4 = b;
            GameManager.Instance.hybridArmy[3] = (b.name);
            for (int i = 0; i < tier4units.Length; i++)
            {
                if (tier4units[i] != preTier4)
                {
                    tier4units[i].GetComponent<Image>().sprite = normal as Sprite;
                }
            }
        }
        else
        {

            if (preTier4.name == b.name)
            {

                GameManager.Instance.hybridArmy[3] = "";
                b.gameObject.GetComponent<Image>().sprite = normal as Sprite;
                tier4Selected = false;
                preTier4 = null;
                tier4Name = "";

            }
            else
            {
                GameManager.Instance.hybridArmy[3] = "";
                tier4Name = b.name;
                tier4Selected = true;
                b.gameObject.GetComponent<Image>().sprite = shine as Sprite;
                preTier4 = b;
                GameManager.Instance.hybridArmy[3] = (b.name);
                for (int i = 0; i < tier4units.Length; i++)
                {
                    if (tier4units[i] != preTier4)
                    {
                        tier4units[i].GetComponent<Image>().sprite = normal as Sprite;
                    }
                }


            }

        }

    }


    //tier5
    public void HighlightTier5(Button b)
    {


        if (!tier5Selected)
        {
            tier5Name = b.name;
            tier5Selected = true;
            b.gameObject.GetComponent<Image>().sprite = shine as Sprite;
            preTier5 = b;
            GameManager.Instance.hybridArmy[4] = (b.name);
            for (int i = 0; i < tier5units.Length; i++)
            {
                if (tier5units[i] != preTier5)
                {
                    tier5units[i].GetComponent<Image>().sprite = normal as Sprite;
                }
            }
        }
        else
        {

            if (preTier5.name == b.name)
            {

                GameManager.Instance.hybridArmy[4] = "";
                b.gameObject.GetComponent<Image>().sprite = normal as Sprite;
                tier5Selected = false;
                preTier5 = null;
                tier5Name = "";

            }
            else
            {
                GameManager.Instance.hybridArmy[4] = "";
                tier5Name = b.name;
                tier5Selected = true;
                b.gameObject.GetComponent<Image>().sprite = shine as Sprite;
                preTier5 = b;
                GameManager.Instance.hybridArmy[4] = (b.name);
                for (int i = 0; i < tier5units.Length; i++)
                {
                    if (tier5units[i] != preTier5)
                    {
                        tier5units[i].GetComponent<Image>().sprite = normal as Sprite;
                    }
                }


            }

        }

    }



    //tier6
    public void HighlightTier6(Button b)
    {


        if (!tier6Selected)
        {
            tier6Name = b.name;
            tier6Selected = true;
            b.gameObject.GetComponent<Image>().sprite = shine as Sprite;
            preTier6 = b;
            GameManager.Instance.hybridArmy[5] = (b.name);
            for (int i = 0; i < tier6units.Length; i++)
            {
                if (tier6units[i] != preTier6)
                {
                    tier6units[i].GetComponent<Image>().sprite = normal as Sprite;
                }
            }
        }
        else
        {

            if (preTier6.name == b.name)
            {

                GameManager.Instance.hybridArmy[5] = "";
                b.gameObject.GetComponent<Image>().sprite = normal as Sprite;
                tier6Selected = false;
                preTier6 = null;
                tier6Name = "";

            }
            else
            {
                GameManager.Instance.hybridArmy[5] = "";
                tier6Name = b.name;
                tier6Selected = true;
                b.gameObject.GetComponent<Image>().sprite = shine as Sprite;
                preTier6 = b;
                GameManager.Instance.hybridArmy[5] = (b.name);
                for (int i = 0; i < tier6units.Length; i++)
                {
                    if (tier6units[i] != preTier6)
                    {
                        tier6units[i].GetComponent<Image>().sprite = normal as Sprite;
                    }
                }


            }

        }


    }


    public void SaveDB_HybridArmy()
    {


        StartCoroutine(SaveDB_HybridArmyHelper());


    }



    IEnumerator SaveDB_HybridArmyHelper()
    {



        PlayerData playerRetrieved = null;
        GameManager.Instance.context.LoadAsync<PlayerData>(GameManager.Instance.player.userName, (result) =>
        {
            if (result.Exception == null)
            {
                playerRetrieved = result.Result as PlayerData;

                if (GameManager.Instance.hybridArmy.Length == 0)
                {

                    playerRetrieved.DB_HybridArmy = new List<string> { "null" };
                }
                else
                {

                    playerRetrieved.DB_HybridArmy = new List<string>();
                    playerRetrieved.DB_HybridArmy = new List<string>(GameManager.Instance.hybridArmy);

                }

            }

            GameManager.Instance.context.SaveAsync<PlayerData>(playerRetrieved, (res) =>
            {
                if (res.Exception == null)
                    print("loadout updated");
            });

        }, null);



        yield return 0;


    }








    #endregion



    #region MatchMaking Functions

    public void HighlightArmy(Button b)
    {


        if (!armySelected)
        {
            armyName = b.name;
            armySelected = true;
            b.gameObject.GetComponent<Image>().sprite = shineArmy as Sprite;
            preArmy = b;
            GameManager.Instance.factionName = b.name;
            for (int i = 0; i < armyButtons.Length; i++)
            {
                if (armyButtons[i] != preArmy)
                {
                    armyButtons[i].GetComponent<Image>().sprite = normalArmy as Sprite;
                }
            }
        }
        else
        {

            if (preArmy.name == b.name)
            {

                GameManager.Instance.factionName = "";
                b.gameObject.GetComponent<Image>().sprite = normalArmy as Sprite;
                armySelected = false;
                preArmy = null;
                armyName = "";

            }
            else
            {

                armyName = b.name;
                armySelected = true;
                b.gameObject.GetComponent<Image>().sprite = shineArmy as Sprite;
                preArmy = b;
                GameManager.Instance.factionName = b.name;
                for (int i = 0; i < armyButtons.Length; i++)
                {
                    if (armyButtons[i] != preArmy)
                    {
                        armyButtons[i].GetComponent<Image>().sprite = normalArmy as Sprite;
                    }
                }


            }

        }

    }

    public void HighlightDeck(Button b)
    {



        if (!deckSelected)
        {
            deckName = b.name;
            deckSelected = true;
            b.gameObject.GetComponent<Image>().sprite = shineArmy as Sprite;
            preDeck = b;

            for (int i = 0; i < deckButtons.Length; i++)
            {
                if (deckButtons[i] != preDeck)
                {
                    deckButtons[i].GetComponent<Image>().sprite = normalArmy as Sprite;
                }
            }
        }
        else
        {

            if (preDeck.name == b.name)
            {


                b.gameObject.GetComponent<Image>().sprite = normalArmy as Sprite;
                deckSelected = false;
                preDeck = null;
                deckName = "";

            }
            else
            {

                deckName = b.name;
                deckSelected = true;
                b.gameObject.GetComponent<Image>().sprite = shineArmy as Sprite;
                preDeck = b;

                for (int i = 0; i < deckButtons.Length; i++)
                {
                    if (deckButtons[i] != preDeck)
                    {
                        deckButtons[i].GetComponent<Image>().sprite = normalArmy as Sprite;
                    }
                }


            }

        }

    }



    public void SelectGameDeck()
    {

        switch (EventSystem.current.currentSelectedGameObject.gameObject.name)
        {

            case "LoadOut1":



                GameManager.Instance.gameDeck = GameManager.Instance.LoadOut1;
                GameManager.Instance.gameDeckName = "LoadOut1";

                break;

            case "LoadOut2":


                GameManager.Instance.gameDeck = GameManager.Instance.LoadOut2;
                GameManager.Instance.gameDeckName = "LoadOut2";

                break;

            case "LoadOut3":


                GameManager.Instance.gameDeck = GameManager.Instance.LoadOut3;
                GameManager.Instance.gameDeckName = "LoadOut3";

                break;

            case "Default1":


                GameManager.Instance.gameDeck = GameManager.Instance.Default1;
               

                break;


            case "Default2":


                GameManager.Instance.gameDeck = GameManager.Instance.Default2;

                break;


            case "Default3":


                GameManager.Instance.gameDeck = GameManager.Instance.Default3;

                break;

            default:
                break;


        }
    }

    #endregion



    #region Register Functions




    public void MatchPasswords()
    {

        if (pass1.text == pass2.text)
        {

            matchError.SetActive(false);
        }
        else
        {

            matchError.SetActive(true);
        }

    }








    #endregion




    #region Menu_Panel Functions


    public void Exit_Game()
    {



        Application.Quit();



    }










    #endregion




    #region Interfaces
    public void BatchGetItemAsync(Dictionary<string, KeysAndAttributes> requestItems, ReturnConsumedCapacity returnConsumedCapacity, AmazonServiceCallback<BatchGetItemRequest, BatchGetItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void BatchGetItemAsync(Dictionary<string, KeysAndAttributes> requestItems, AmazonServiceCallback<BatchGetItemRequest, BatchGetItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void BatchGetItemAsync(BatchGetItemRequest request, AmazonServiceCallback<BatchGetItemRequest, BatchGetItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void BatchWriteItemAsync(Dictionary<string, List<WriteRequest>> requestItems, AmazonServiceCallback<BatchWriteItemRequest, BatchWriteItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void BatchWriteItemAsync(BatchWriteItemRequest request, AmazonServiceCallback<BatchWriteItemRequest, BatchWriteItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void CreateTableAsync(string tableName, List<KeySchemaElement> keySchema, List<AttributeDefinition> attributeDefinitions, ProvisionedThroughput provisionedThroughput, AmazonServiceCallback<CreateTableRequest, CreateTableResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void CreateTableAsync(CreateTableRequest request, AmazonServiceCallback<CreateTableRequest, CreateTableResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void DeleteItemAsync(string tableName, Dictionary<string, AttributeValue> key, AmazonServiceCallback<DeleteItemRequest, DeleteItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void DeleteItemAsync(string tableName, Dictionary<string, AttributeValue> key, ReturnValue returnValues, AmazonServiceCallback<DeleteItemRequest, DeleteItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void DeleteItemAsync(DeleteItemRequest request, AmazonServiceCallback<DeleteItemRequest, DeleteItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void DeleteTableAsync(string tableName, AmazonServiceCallback<DeleteTableRequest, DeleteTableResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void DeleteTableAsync(DeleteTableRequest request, AmazonServiceCallback<DeleteTableRequest, DeleteTableResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void DescribeLimitsAsync(DescribeLimitsRequest request, AmazonServiceCallback<DescribeLimitsRequest, DescribeLimitsResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void DescribeTableAsync(string tableName, AmazonServiceCallback<DescribeTableRequest, DescribeTableResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void DescribeTableAsync(DescribeTableRequest request, AmazonServiceCallback<DescribeTableRequest, DescribeTableResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void GetItemAsync(string tableName, Dictionary<string, AttributeValue> key, AmazonServiceCallback<GetItemRequest, GetItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void GetItemAsync(string tableName, Dictionary<string, AttributeValue> key, bool consistentRead, AmazonServiceCallback<GetItemRequest, GetItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void GetItemAsync(GetItemRequest request, AmazonServiceCallback<GetItemRequest, GetItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void ListTablesAsync(string exclusiveStartTableName, AmazonServiceCallback<ListTablesRequest, ListTablesResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void ListTablesAsync(string exclusiveStartTableName, int limit, AmazonServiceCallback<ListTablesRequest, ListTablesResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void ListTablesAsync(int limit, AmazonServiceCallback<ListTablesRequest, ListTablesResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void ListTablesAsync(ListTablesRequest request, AmazonServiceCallback<ListTablesRequest, ListTablesResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void PutItemAsync(string tableName, Dictionary<string, AttributeValue> item, AmazonServiceCallback<PutItemRequest, PutItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();



    }

    public void PutItemAsync(string tableName, Dictionary<string, AttributeValue> item, ReturnValue returnValues, AmazonServiceCallback<PutItemRequest, PutItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void PutItemAsync(PutItemRequest request, AmazonServiceCallback<PutItemRequest, PutItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void QueryAsync(QueryRequest request, AmazonServiceCallback<QueryRequest, QueryResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void ScanAsync(string tableName, List<string> attributesToGet, AmazonServiceCallback<ScanRequest, ScanResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void ScanAsync(string tableName, Dictionary<string, Condition> scanFilter, AmazonServiceCallback<ScanRequest, ScanResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void ScanAsync(string tableName, List<string> attributesToGet, Dictionary<string, Condition> scanFilter, AmazonServiceCallback<ScanRequest, ScanResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void ScanAsync(ScanRequest request, AmazonServiceCallback<ScanRequest, ScanResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void UpdateItemAsync(string tableName, Dictionary<string, AttributeValue> key, Dictionary<string, AttributeValueUpdate> attributeUpdates, AmazonServiceCallback<UpdateItemRequest, UpdateItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void UpdateItemAsync(string tableName, Dictionary<string, AttributeValue> key, Dictionary<string, AttributeValueUpdate> attributeUpdates, ReturnValue returnValues, AmazonServiceCallback<UpdateItemRequest, UpdateItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void UpdateItemAsync(UpdateItemRequest request, AmazonServiceCallback<UpdateItemRequest, UpdateItemResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void UpdateTableAsync(string tableName, ProvisionedThroughput provisionedThroughput, AmazonServiceCallback<UpdateTableRequest, UpdateTableResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void UpdateTableAsync(UpdateTableRequest request, AmazonServiceCallback<UpdateTableRequest, UpdateTableResponse> callback, AsyncOptions options = null)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }


    #endregion




    #region Login Functions


    public void RememberMe() {



        if (rememberMe.isOn) {


            PlayerPrefs.SetString("Username", userName_login.text);
            PlayerPrefs.SetString("Password", password.text);
            PlayerPrefs.Save();

        }



    }





    #endregion


    #region Load Data //First Time After Loggin



    public void LoadLoginData()
    {


        StartCoroutine(LoadLoginDataHelper_LoadOuts());
        StartCoroutine(LoadLoginDataHelper_Chests());
        StartCoroutine(LoadLoginDataHelper_Shards());
        StartCoroutine(LoadLoginDataHelper_Tokens());
        StartCoroutine(LoadLoginDataHelper_Armor());

    }



    IEnumerator LoadLoginDataHelper_Armor() {


        GetBaseHero();
        
        yield return 0;


    }

    IEnumerator LoadLoginDataHelper_Shards() {


        manager.shardText.text = manager.player.DB_currency.ToString();

        yield return 0;

    }

    IEnumerator LoadLoginDataHelper_Tokens()
    {

        manager.tokenText.text = manager.player.DB_token.ToString();

        yield return 0;

    }


    IEnumerator LoadLoginDataHelper_Chests() {




        manager.chestNumbers[0].text = manager.player.DB_chests["wooden"].ToString();
        manager.chestNumbers[1].text = manager.player.DB_chests["bronze"].ToString();
        manager.chestNumbers[2].text = manager.player.DB_chests["silver"].ToString();
        manager.chestNumbers[3].text = manager.player.DB_chests["gold"].ToString();
        manager.chestNumbers[4].text = manager.player.DB_chests["immortal"].ToString();






        yield return 0;

    }

    IEnumerator LoadLoginDataHelper_LoadOuts()
    {

       


        //yield return new WaitForSeconds(4);
        manager.LoadOut1 = new List<string>(manager.player.DB_loadout1);
        manager.LoadOut2 = new List<string>(manager.player.DB_loadout2);
        manager.LoadOut3 = new List<string>(manager.player.DB_loadout3);
        buttonText[0].text = manager.player.DB_loadOutNames["LoadOut1"];
        buttonText[1].text = manager.player.DB_loadOutNames["LoadOut2"];
        buttonText[2].text = manager.player.DB_loadOutNames["LoadOut3"];

        //if (GameManager.Instance.player.DB_loadout1.Contains("null") || GameManager.Instance.player.DB_loadout1.Count == null)
        //    {

        //    }
        //    else
        //    {


        //        for (int i = 0; i < GameManager.Instance.LoadOut1.Count; i++)
        //        {

        //            var pix = Resources.Load<Sprite>("cardImages/" + GameManager.Instance.LoadOut1[i]) as Sprite;
        //            GameObject image = Instantiate(Resources.Load("cardPrefab")) as GameObject;
        //            image.GetComponent<Image>().sprite = pix as Sprite;
        //            image.transform.gameObject.name = pix.name;
        //            image.transform.SetParent(LoadOut1.transform, false);


        //        }

        //    }



        //    if (GameManager.Instance.player.DB_loadout2.Contains("null") || GameManager.Instance.player.DB_loadout2 == null)
        //    {

        //    }
        //    else
        //    {



        //        for (int i = 0; i < GameManager.Instance.LoadOut2.Count; i++)
        //        {

        //            var pix = Resources.Load<Sprite>("cardImages/" + GameManager.Instance.LoadOut2[i]) as Sprite;
        //            GameObject image = Instantiate(Resources.Load("cardPrefab")) as GameObject;
        //            image.GetComponent<Image>().sprite = pix as Sprite;
        //            image.transform.gameObject.name = pix.name;
        //            image.transform.SetParent(LoadOut2.transform, false);


        //        }

        //    }

        //    if (GameManager.Instance.player.DB_loadout3.Contains("null") || GameManager.Instance.player.DB_loadout3 == null)
        //    {

        //    }
        //    else
        //    {


        //        for (int i = 0; i < GameManager.Instance.LoadOut3.Count; i++)
        //        {

        //            var pix = Resources.Load<Sprite>("cardImages/" + GameManager.Instance.LoadOut3[i]) as Sprite;
        //            GameObject image = Instantiate(Resources.Load("cardPrefab")) as GameObject;
        //            image.GetComponent<Image>().sprite = pix as Sprite;
        //            image.transform.gameObject.name = pix.name;
        //            image.transform.SetParent(LoadOut3.transform, false);


        //        }
        //    }






        LoadCards();

        
        yield return new WaitForSeconds(3);
        yield return 0;
        DataLoaded = true;
        


    }

    IEnumerator LoadLoginDataWait()
    {

        yield return StartCoroutine(LoadLoginDataHelper_LoadOuts());
        if (DataLoaded)
        {

            LoadCards();

        }

        yield return 0;

    }







    IEnumerator LoadLoginDataHelper_HybridArmy()
    {




        yield return 0;

    }




    //public void LoadLoadOutsAgain() {


    //    GameManager.Instance.LoadOut1 = new List<string>(GameManager.Instance.player.DB_loadout1);
    //    GameManager.Instance.LoadOut2 = new List<string>(GameManager.Instance.player.DB_loadout2);
    //    GameManager.Instance.LoadOut3 = new List<string>(GameManager.Instance.player.DB_loadout3);

    //    if (GameManager.Instance.player.DB_loadout1.Contains("null") || GameManager.Instance.player.DB_loadout1.Count == null)
    //    {

    //    }
    //    else
    //    {


    //        for (int i = 0; i < GameManager.Instance.LoadOut1.Count; i++)
    //        {

    //            var pix = Resources.Load<Sprite>("cardImages/" + GameManager.Instance.LoadOut1[i]) as Sprite;
    //            GameObject image = Instantiate(Resources.Load("cardPrefab")) as GameObject;
    //            image.GetComponent<Image>().sprite = pix as Sprite;
    //            image.transform.gameObject.name = pix.name;
    //            image.transform.SetParent(LoadOut1.transform, false);


    //        }

    //    }



    //    if (GameManager.Instance.player.DB_loadout2.Contains("null") || GameManager.Instance.player.DB_loadout2 == null)
    //    {

    //    }
    //    else
    //    {



    //        for (int i = 0; i < GameManager.Instance.LoadOut2.Count; i++)
    //        {

    //            var pix = Resources.Load<Sprite>("cardImages/" + GameManager.Instance.LoadOut2[i]) as Sprite;
    //            GameObject image = Instantiate(Resources.Load("cardPrefab")) as GameObject;
    //            image.GetComponent<Image>().sprite = pix as Sprite;
    //            image.transform.gameObject.name = pix.name;
    //            image.transform.SetParent(LoadOut2.transform, false);


    //        }

    //    }

    //    if (GameManager.Instance.player.DB_loadout3.Contains("null") || GameManager.Instance.player.DB_loadout3 == null)
    //    {

    //    }
    //    else
    //    {


    //        for (int i = 0; i < GameManager.Instance.LoadOut3.Count; i++)
    //        {

    //            var pix = Resources.Load<Sprite>("cardImages/" + GameManager.Instance.LoadOut3[i]) as Sprite;
    //            GameObject image = Instantiate(Resources.Load("cardPrefab")) as GameObject;
    //            image.GetComponent<Image>().sprite = pix as Sprite;
    //            image.transform.gameObject.name = pix.name;
    //            image.transform.SetParent(LoadOut3.transform, false);


    //        }
    //    }




    //}

    #endregion


    #region Option_Panel Variables
    [Space(10)]
    [Header("---------Options Variables---------")]
    public Dropdown resolution;
    public Dropdown display;
    public Toggle fourBy3, sixteenBy9, sixteenBy10;
    public List<Dropdown.OptionData> res16_9 = new List<Dropdown.OptionData>();
    public List<Dropdown.OptionData> res16_10 = new List<Dropdown.OptionData>();
    public List<Dropdown.OptionData> res4_3 = new List<Dropdown.OptionData>();
    // enum Dres16_9 {  1280720 = 0 }
    //1="1365,768" ,  1600,900 ,  1920,1080  };
    // public Dictionary<int, int> Dres16_10 = new Dictionary<int, int>() { { 1280, 800 }, { 1440,900 }, { 1680,1050 }, { 1920,1200 } };
    // public Dictionary<int, int> Dres4_3 = new Dictionary<int, int>() { { 800, 600 }, { 1024,768 }, { 1152,864 }, { 1280,960 }, { 1400,1050 }, { 1600,1200 } };
    public int width;
    public int height;
    public bool fullscreen = false;


    #endregion



    #region Options Functions


    public void RatioToggle() {

        if (fourBy3.isOn)
        {

            resolution.options = res4_3;

        }
        else if (sixteenBy9.isOn)
        {
            resolution.options = res16_9;

        }
        else if (sixteenBy10.isOn) {


            resolution.options = res16_10;

        }
        

    }



    public void ApplyResolution() {

        if (display.value == 0)
        {

            fullscreen = true;
            PlayerPrefs.SetString("fullscreen", "true");
         
        }
        else if (display.value == 1) {


            fullscreen = false;
            PlayerPrefs.SetString("fullscreen", "false");
        }



        if (fourBy3.isOn)
        {

            //  var value = resolution.options[resolution.value]
            

            int index = resolution.options[resolution.value].text.IndexOf("x");
            string data = resolution.options[resolution.value].text;
            string newValue = Regex.Replace(data, "x", "");
            width = int.Parse(newValue.Remove(4));
            height = int.Parse(newValue.Substring(4));
            PlayerPrefs.SetInt("width", width);
            PlayerPrefs.SetInt("height", height);
            Screen.SetResolution(width, height, true);
           

        }
        else if (sixteenBy9.isOn)
        {
            int index = resolution.options[resolution.value].text.IndexOf("x");
            string data = resolution.options[resolution.value].text;
            string newValue = Regex.Replace(data, "x", "");
            width = int.Parse(newValue.Remove(4));
            height = int.Parse(newValue.Substring(4));
            PlayerPrefs.SetInt("width", width);
            PlayerPrefs.SetInt("height", height);
            StartCoroutine(ChangeResolution(width, height));
        }
        else if (sixteenBy10.isOn)
        {


            int index = resolution.options[resolution.value].text.IndexOf("x");
            string data = resolution.options[resolution.value].text;
            string newValue = Regex.Replace(data, "x", "");
            width = int.Parse(newValue.Remove(4));
            height = int.Parse(newValue.Substring(4));
            PlayerPrefs.SetInt("width", width);
            PlayerPrefs.SetInt("height", height);
            
            Screen.SetResolution(width, height, true);

        }


    }



    IEnumerator ChangeResolution(int w, int h)
    {

      
        Screen.fullScreen = fullscreen;

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Screen.SetResolution(w, h, Screen.fullScreen);
        print("here");
    }

    #endregion




    #region LevelSystem_Panel variables

    [Header("--------LevelSystem Variables----------")]

    public Image hpSlider_animal;
    public Text t_level_animal;
    public Text t_xp_animal;

    public Image hpSlider_human;
    public Text t_level_human;
    public Text t_xp_human;

    public Image hpSlider_horror;
    public Text t_level_horror;
    public Text t_xp_horror;

    public Image hpSlider_mythical;
    public Text t_level_mythical;
    public Text t_xp_mythical;

    public Image hpSlider_hybrid;
    public Text t_level_hybrid;
    public Text t_xp_hybrid;

    public Text t_player_level;


    public int incrementOne = 150;
    public int incrementTwo = 250;
    public int incrementThree = 300;
    public int maxLevel = 50;

    //public int xpPerLevel_animal = 500;
    //public int currentLevelXp_animal = 0;
    //public int totalXp_animal = 0;    
    //public int currentLevel_animal = 1;


    #endregion


    #region LevelSystem Functions

    public void NewLevelInitializer_Animal(int extra)
    {

        if (GameManager.Instance.player.DB_stats["AnimalLevel"] < 50)
        {
            GameManager.Instance.player.DB_stats["AnimalLevel"] += 1;
        }

        //display
        t_level_animal.text = "" + GameManager.Instance.player.DB_stats["AnimalLevel"];

        if (GameManager.Instance.player.DB_stats["AnimalLevel"] < 21)
        {
            GameManager.Instance.player.DB_stats["AnimalXpPerLevel"] += incrementOne;
        }
        else if (GameManager.Instance.player.DB_stats["AnimalLevel"] >= 21 && GameManager.Instance.player.DB_stats["AnimalLevel"] <= 41)
        {
            GameManager.Instance.player.DB_stats["AnimalXpPerLevel"] += incrementTwo;
        }
        else
        {
            GameManager.Instance.player.DB_stats["AnimalXpPerLevel"] += incrementThree;
        }


        GameManager.Instance.player.DB_stats["AnimalLevelXp"] = extra;

        t_player_level.text = "" + (GameManager.Instance.player.DB_stats["AnimalLevel"] + GameManager.Instance.player.DB_stats["HumanLevel"] + GameManager.Instance.player.DB_stats["HorrorLevel"] + GameManager.Instance.player.DB_stats["MythicalLevel"] + GameManager.Instance.player.DB_stats["HybridLevel"]);

    }

    public void NewLevelInitializer_Human(int extra)
    {

        if (GameManager.Instance.player.DB_stats["HumanLevel"] < 50)
        {
            GameManager.Instance.player.DB_stats["HumanLevel"] += 1;
        }

        //display
        t_level_human.text = "" + GameManager.Instance.player.DB_stats["HumanLevel"];

        if (GameManager.Instance.player.DB_stats["HumanLevel"] < 21)
        {
            GameManager.Instance.player.DB_stats["HumanXpPerLevel"] += incrementOne;
        }
        else if (GameManager.Instance.player.DB_stats["HumanLevel"] >= 21 && GameManager.Instance.player.DB_stats["HumanLevel"] <= 41)
        {
            GameManager.Instance.player.DB_stats["HumanXpPerLevel"] += incrementTwo;
        }
        else
        {
            GameManager.Instance.player.DB_stats["HumanXpPerLevel"] += incrementThree;
        }


        GameManager.Instance.player.DB_stats["HumanLevelXp"] = extra;

        t_player_level.text = "" + (GameManager.Instance.player.DB_stats["AnimalLevel"] + GameManager.Instance.player.DB_stats["HumanLevel"] + GameManager.Instance.player.DB_stats["HorrorLevel"] + GameManager.Instance.player.DB_stats["MythicalLevel"] + GameManager.Instance.player.DB_stats["HybridLevel"]);

    }

    public void NewLevelInitializer_Horror(int extra)
    {

        if (GameManager.Instance.player.DB_stats["HorrorLevel"] < 50)
        {
            GameManager.Instance.player.DB_stats["HorrorLevel"] += 1;
        }

        //display
        t_level_horror.text = "" + GameManager.Instance.player.DB_stats["HorrorLevel"];

        if (GameManager.Instance.player.DB_stats["HorrorLevel"] < 21)
        {
            GameManager.Instance.player.DB_stats["HorrorXpPerLevel"] += incrementOne;
        }
        else if (GameManager.Instance.player.DB_stats["HorrorLevel"] >= 21 && GameManager.Instance.player.DB_stats["HorrorLevel"] <= 41)
        {
            GameManager.Instance.player.DB_stats["HorrorXpPerLevel"] += incrementTwo;
        }
        else
        {
            GameManager.Instance.player.DB_stats["HorrorXpPerLevel"] += incrementThree;
        }


        GameManager.Instance.player.DB_stats["HorrorLevelXp"] = extra;

        t_player_level.text = "" + (GameManager.Instance.player.DB_stats["AnimalLevel"] + GameManager.Instance.player.DB_stats["HumanLevel"] + GameManager.Instance.player.DB_stats["HorrorLevel"] + GameManager.Instance.player.DB_stats["MythicalLevel"] + GameManager.Instance.player.DB_stats["HybridLevel"]);

    }

    public void NewLevelInitializer_Mythical(int extra)
    {

        if (GameManager.Instance.player.DB_stats["MythicalLevel"] < 50)
        {
            GameManager.Instance.player.DB_stats["MythicalLevel"] += 1;
        }

        //display
        t_level_mythical.text = "" + GameManager.Instance.player.DB_stats["MythicalLevel"];

        if (GameManager.Instance.player.DB_stats["MythicalLevel"] < 21)
        {
            GameManager.Instance.player.DB_stats["MythicalXpPerLevel"] += incrementOne;
        }
        else if (GameManager.Instance.player.DB_stats["MythicalLevel"] >= 21 && GameManager.Instance.player.DB_stats["MythicalLevel"] <= 41)
        {
            GameManager.Instance.player.DB_stats["MythicalXpPerLevel"] += incrementTwo;
        }
        else
        {
            GameManager.Instance.player.DB_stats["MythicalXpPerLevel"] += incrementThree;
        }


        GameManager.Instance.player.DB_stats["MythicalLevelXp"] = extra;

        t_player_level.text = "" + (GameManager.Instance.player.DB_stats["AnimalLevel"] + GameManager.Instance.player.DB_stats["HumanLevel"] + GameManager.Instance.player.DB_stats["HorrorLevel"] + GameManager.Instance.player.DB_stats["MythicalLevel"] + GameManager.Instance.player.DB_stats["HybridLevel"]);

    }

    public void NewLevelInitializer_Hybrid(int extra)
    {

        if (GameManager.Instance.player.DB_stats["HybridLevel"] < 50)
        {
            GameManager.Instance.player.DB_stats["HybridLevel"] += 1;
        }

        //display
        t_level_hybrid.text = "" + GameManager.Instance.player.DB_stats["HybridLevel"];

        if (GameManager.Instance.player.DB_stats["HybridLevel"] < 21)
        {
            GameManager.Instance.player.DB_stats["HybridXpPerLevel"] += incrementOne;
        }
        else if (GameManager.Instance.player.DB_stats["HybridLevel"] >= 21 && GameManager.Instance.player.DB_stats["HybridLevel"] <= 41)
        {
            GameManager.Instance.player.DB_stats["HybridXpPerLevel"] += incrementTwo;
        }
        else
        {
            GameManager.Instance.player.DB_stats["HybridXpPerLevel"] += incrementThree;
        }

        GameManager.Instance.player.DB_stats["HybridLevelXp"] = extra;

        t_player_level.text = "" + (GameManager.Instance.player.DB_stats["AnimalLevel"] + GameManager.Instance.player.DB_stats["HumanLevel"] + GameManager.Instance.player.DB_stats["HorrorLevel"] + GameManager.Instance.player.DB_stats["MythicalLevel"] + GameManager.Instance.player.DB_stats["HybridLevel"]);

    }

    public void LevelSystemWork()
    {
        ///// Level System Animal
        LevelSystemWorkAnimal();

        ///// Level System Human
        LevelSystemWorkHuman();

        ///// Level System horror
        LevelSystemWorkHorror();

        ///// Level System Mythical
        LevelSystemWorkMythical();

        ///// Level System Hybrid
        LevelSystemWorkHybrid();

    }

    public void LevelSystemWorkAnimal()
    {

        if (GameManager.Instance.player.DB_stats["AnimalLevelXp"] >= GameManager.Instance.player.DB_stats["AnimalXpPerLevel"])
        {
            NewLevelInitializer_Animal(GameManager.Instance.player.DB_stats["AnimalLevelXp"] - GameManager.Instance.player.DB_stats["AnimalXpPerLevel"]);
        }

        if (GameManager.Instance.player.DB_stats["AnimalLevel"] < maxLevel)
        {
            t_xp_animal.text = GameManager.Instance.player.DB_stats["AnimalLevelXp"] + " / " + GameManager.Instance.player.DB_stats["AnimalXpPerLevel"];
            hpSlider_animal.fillAmount = Mathf.Lerp(hpSlider_animal.fillAmount, ((float)GameManager.Instance.player.DB_stats["AnimalLevelXp"] / (float)GameManager.Instance.player.DB_stats["AnimalXpPerLevel"]), Time.deltaTime * 20);
        }
        else
        {
            t_xp_animal.text = "Max Level 50";
            hpSlider_animal.fillAmount = 1f;
        }
    }

    public void LevelSystemWorkHuman()
    {
        if (GameManager.Instance.player.DB_stats["HumanLevelXp"] >= GameManager.Instance.player.DB_stats["HumanXpPerLevel"])
        {
            NewLevelInitializer_Human(GameManager.Instance.player.DB_stats["HumanLevelXp"] - GameManager.Instance.player.DB_stats["HumanXpPerLevel"]);
        }

        if (GameManager.Instance.player.DB_stats["HumanLevel"] < maxLevel)
        {
            t_xp_human.text = GameManager.Instance.player.DB_stats["HumanLevelXp"] + " / " + GameManager.Instance.player.DB_stats["HumanXpPerLevel"];
            hpSlider_human.fillAmount = Mathf.Lerp(hpSlider_human.fillAmount, ((float)GameManager.Instance.player.DB_stats["HumanLevelXp"] / (float)GameManager.Instance.player.DB_stats["HumanXpPerLevel"]), Time.deltaTime * 20);
        }
        else
        {
            t_xp_human.text = "Max Level 50";
            hpSlider_human.fillAmount = 1f;
        }

    }

    public void LevelSystemWorkHorror()
    {
        if (GameManager.Instance.player.DB_stats["HorrorLevelXp"] >= GameManager.Instance.player.DB_stats["HorrorXpPerLevel"])
        {
            NewLevelInitializer_Horror(GameManager.Instance.player.DB_stats["HorrorLevelXp"] - GameManager.Instance.player.DB_stats["HorrorXpPerLevel"]);
        }

        if (GameManager.Instance.player.DB_stats["HorrorLevel"] < maxLevel)
        {
            t_xp_horror.text = GameManager.Instance.player.DB_stats["HorrorLevelXp"] + " / " + GameManager.Instance.player.DB_stats["HorrorXpPerLevel"];
            hpSlider_horror.fillAmount = Mathf.Lerp(hpSlider_horror.fillAmount, ((float)GameManager.Instance.player.DB_stats["HorrorLevelXp"] / (float)GameManager.Instance.player.DB_stats["HorrorXpPerLevel"]), Time.deltaTime * 20);
        }
        else
        {
            t_xp_horror.text = "Max Level 50";
            hpSlider_horror.fillAmount = 1f;
        }

    }

    public void LevelSystemWorkMythical()
    {
        if (GameManager.Instance.player.DB_stats["MythicalLevelXp"] >= GameManager.Instance.player.DB_stats["MythicalXpPerLevel"])
        {
            NewLevelInitializer_Mythical(GameManager.Instance.player.DB_stats["MythicalLevelXp"] - GameManager.Instance.player.DB_stats["MythicalXpPerLevel"]);
        }

        if (GameManager.Instance.player.DB_stats["MythicalLevel"] < maxLevel)
        {
            t_xp_mythical.text = GameManager.Instance.player.DB_stats["MythicalLevelXp"] + " / " + GameManager.Instance.player.DB_stats["MythicalXpPerLevel"];
            hpSlider_mythical.fillAmount = Mathf.Lerp(hpSlider_mythical.fillAmount, ((float)GameManager.Instance.player.DB_stats["MythicalLevelXp"] / (float)GameManager.Instance.player.DB_stats["MythicalXpPerLevel"]), Time.deltaTime * 20);
        }
        else
        {
            t_xp_mythical.text = "Max Level 50";
            hpSlider_mythical.fillAmount = 1f;
        }

    }

    public void LevelSystemWorkHybrid()
    {

        if (GameManager.Instance.player.DB_stats["HybridLevelXp"] >= GameManager.Instance.player.DB_stats["HybridXpPerLevel"])
        {
            NewLevelInitializer_Hybrid(GameManager.Instance.player.DB_stats["HybridLevelXp"] - GameManager.Instance.player.DB_stats["HybridXpPerLevel"]);
        }

        if (GameManager.Instance.player.DB_stats["HybridLevel"] < maxLevel)
        {
            t_xp_hybrid.text = GameManager.Instance.player.DB_stats["HybridLevelXp"] + " / " + GameManager.Instance.player.DB_stats["HybridXpPerLevel"];
            hpSlider_hybrid.fillAmount = Mathf.Lerp(hpSlider_hybrid.fillAmount, ((float)GameManager.Instance.player.DB_stats["HybridLevelXp"] / (float)GameManager.Instance.player.DB_stats["HybridXpPerLevel"]), Time.deltaTime * 20);
        }
        else
        {
            t_xp_hybrid.text = "Max Level 50";
            hpSlider_hybrid.fillAmount = 1f;
        }

    }


    public void DisplayLevels()
    {

        //display
        t_level_animal.text = "" + GameManager.Instance.player.DB_stats["AnimalLevel"];
        t_level_human.text = "" + GameManager.Instance.player.DB_stats["HumanLevel"];
        t_level_mythical.text = "" + GameManager.Instance.player.DB_stats["MythicalLevel"];
        t_level_horror.text = "" + GameManager.Instance.player.DB_stats["HorrorLevel"];
        t_level_hybrid.text = "" + GameManager.Instance.player.DB_stats["HybridLevel"];

        t_player_level.text = "" + (GameManager.Instance.player.DB_stats["AnimalLevel"] + GameManager.Instance.player.DB_stats["HumanLevel"] + GameManager.Instance.player.DB_stats["HorrorLevel"] + GameManager.Instance.player.DB_stats["MythicalLevel"] + GameManager.Instance.player.DB_stats["HybridLevel"]);

    }

    #endregion



    #region HeroLoadOut_Panel variables

    [Header("--------HeroLoadOut Variables----------")]

    public GameObject Character;
    public Camera LoadOutCam;
    public bool keepRotating = false;


    public Transform itemButtons;
    public Transform weaponPanel;
    public Transform offhandPanel;
    public Transform helmPanel;
    public Transform chestPanel;
    public Transform legsPanel;
    public Transform itemPanel;
    public List<GameObject> allPanelsForItemsParent = new List<GameObject>();
    public Text itemNameText;
    public Image itemImageShow;
    public Text hp;
    public Text damage;
    public Text mspeed;
    public Text magicres;
    public Text physicalres;
    public Text aurasize;
    public Text attackspeed;
    public Text unchance;
    public JSONNode heroData;
    

    public float f_hp;
    public float f_damage;
    public float f_mspeed;
    public float f_magicres;
    public float f_physicalres;
    public float f_aurasize;
    public float f_attackspeed;
    public float f_unchance;


    #endregion

    #region HeroLoadOut Functions

    
    public void GetBaseHero() {


        TextAsset textAsset = Resources.Load("items") as TextAsset;
        var dataEnc = JSON.Parse(textAsset.text);
        heroData = dataEnc;


        print(heroData[0][(manager.player.DB_HeroLoadOut["weapon"])][7]);



        UpdateStats();
    }



    public void UpdateStats() {



        f_hp = heroData[0]["Base"][0].AsFloat;
        f_damage = heroData[0]["Base"][1].AsFloat;
        f_attackspeed = heroData[0]["Base"][2].AsFloat;
        f_mspeed = heroData[0]["Base"][3].AsFloat;
        f_magicres = heroData[0]["Base"][4].AsFloat;
        f_aurasize = heroData[0]["Base"][5].AsFloat;
        f_physicalres = heroData[0]["Base"][6].AsFloat;
        f_unchance = heroData[0]["Base"][7].AsFloat;


        float tempAS = 0;
        float tempMS = 0;
        float WeaponUC = heroData[0][(manager.player.DB_HeroLoadOut["weapon"])][7].AsFloat;
        f_unchance = WeaponUC;

        foreach (KeyValuePair<string,string> item in manager.player.DB_HeroLoadOut)
        {

            f_hp += heroData[0][item.Value][0].AsFloat;
            f_damage += heroData[0][item.Value][1].AsFloat;
            tempAS += heroData[0][item.Value][2].AsFloat;
            tempMS += heroData[0][item.Value][3].AsFloat;
            f_magicres += heroData[0][item.Value][4].AsFloat / 100;
            f_aurasize += heroData[0][item.Value][5].AsFloat;
            f_physicalres += heroData[0][item.Value][6].AsFloat /100;
            if (item.Value != manager.player.DB_HeroLoadOut["weapon"]) {

                f_unchance += WeaponUC * heroData[0][item.Value][7].AsFloat;
            }

        }

        f_attackspeed = f_attackspeed * (1 + (tempAS / 100));
        f_mspeed = f_mspeed * (1 + (tempMS / 100));


        hp.text =  f_hp.ToString();
        damage.text =  f_damage.ToString();
        mspeed.text =  f_mspeed.ToString();
        magicres.text = f_magicres.ToString();
        physicalres.text =  f_physicalres.ToString();
        aurasize.text =   f_aurasize.ToString();
        attackspeed.text =  f_attackspeed.ToString();
        unchance.text =  f_unchance.ToString();



    }

    public void ItemBackSaveButton() {

        manager.SavePlayerData();


    }
    public void ItemShowDesc(string name) {

        itemNameText.text = name;


    }

    public void ItemClicked(GameObject clicked,bool selected) {
        

        if (clicked.transform.parent.parent.name == "offhand")
        {

            if (!selected)
            {

               
                offhandPanel.transform.Find("Basic Shield Aura").FindChild("tick").GetComponent<Image>().enabled = true;
                offhandPanel.transform.Find("Basic Shield Aura").GetComponent<SelectItem>().selected = true;
                manager.player.DB_HeroLoadOut["offhand"] = "Basic Shield Aura";
                UpdateStats();
            }
            else
            {

                manager.player.DB_HeroLoadOut["offhand"] = clicked.gameObject.name;
                UpdateStats();
                foreach (Transform item in offhandPanel)
                {
                    if (item.name == clicked.name) { }
                    else
                    {
                        item.Find("tick").GetComponent<Image>().enabled = false;
                        item.GetComponent<SelectItem>().selected = false;
                    }
                }
            }
        }


        if (clicked.transform.parent.parent.name == "weapon")
        {

            if (!selected)
            {

               
                weaponPanel.transform.Find("Basic Sword").FindChild("tick").GetComponent<Image>().enabled = true;
                weaponPanel.transform.Find("Basic Sword").GetComponent<SelectItem>().selected = true;
                manager.player.DB_HeroLoadOut["weapon"] = "Basic Sword";
                UpdateStats();

            }
            else
            {

                manager.player.DB_HeroLoadOut["weapon"] = clicked.gameObject.name;
                UpdateStats();
                foreach (Transform item in weaponPanel)
                {
                    if (item.name == clicked.name) { }
                    else
                    {
                        item.Find("tick").GetComponent<Image>().enabled = false;
                        item.GetComponent<SelectItem>().selected = false;
                    }
                }
            }
        }


        if (clicked.transform.parent.parent.name == "helm")
        {

            if (!selected)
            {

              
                helmPanel.transform.Find("Basic Helm").FindChild("tick").GetComponent<Image>().enabled = true;
                helmPanel.transform.Find("Basic Helm").GetComponent<SelectItem>().selected = true;
                manager.player.DB_HeroLoadOut["helm"] = "Basic Helm";
                UpdateStats();
            }
            else
            {

                manager.player.DB_HeroLoadOut["helm"] = clicked.gameObject.name;
                UpdateStats();
                foreach (Transform item in helmPanel)
                {
                    if (item.name == clicked.name) { }
                    else
                    {
                        item.Find("tick").GetComponent<Image>().enabled = false;
                        item.GetComponent<SelectItem>().selected = false;
                    }
                }
            }
        }


        if (clicked.transform.parent.parent.name == "chest")
        {

            if (!selected)
            {

               
                chestPanel.transform.Find("Basic Chestplate").FindChild("tick").GetComponent<Image>().enabled = true;
                chestPanel.transform.Find("Basic Chestplate").GetComponent<SelectItem>().selected = true;
                manager.player.DB_HeroLoadOut["chest"] = "Basic Chestplate";
                UpdateStats();
            }
            else
            {

                manager.player.DB_HeroLoadOut["chest"] = clicked.gameObject.name;
                UpdateStats();
                foreach (Transform item in chestPanel)
                {
                    if (item.name == clicked.name) { }
                    else
                    {
                        item.Find("tick").GetComponent<Image>().enabled = false;
                        item.GetComponent<SelectItem>().selected = false;
                    }
                }
            }
        }

        if (clicked.transform.parent.parent.name == "legs")
        {

            if (!selected)
            {

                
                legsPanel.transform.Find("Basic Legs").FindChild("tick").GetComponent<Image>().enabled = true;
                legsPanel.transform.Find("Basic Legs").GetComponent<SelectItem>().selected = true;
                manager.player.DB_HeroLoadOut["legs"] = "Basic Legs";
                UpdateStats();
            }
            else
            {

                manager.player.DB_HeroLoadOut["legs"] = clicked.gameObject.name;
                UpdateStats();
                foreach (Transform item in legsPanel)
                {
                    if (item.name == clicked.name) { }
                    else
                    {
                        item.Find("tick").GetComponent<Image>().enabled = false;
                        item.GetComponent<SelectItem>().selected = false;
                    }
                }
            }
        }



    }



    public void LoadArmor() {

        for (int i = 0; i < manager.player.DB_armorItems.Count; i++)
        {

            if (manager.player.DB_armorItems[manager.allArmorNames[i]] == 0)
            {




            }
            else {


                GameObject image = Instantiate(Resources.Load("itemPrefab")) as GameObject;
                image.name = manager.allArmorNames[i];

                if (image.name.Contains("Helm")) {


                    image.transform.SetParent(helmPanel.transform, false);

                    if (manager.player.DB_HeroLoadOut["helm"] == image.name) {

                        image.transform.Find("tick").GetComponent<Image>().enabled = true;
                        image.GetComponent<SelectItem>().selected = true;
                    }

                }else if(image.name.Contains("Chestplate")){

                    image.transform.SetParent(chestPanel.transform, false);
                    if (manager.player.DB_HeroLoadOut["chest"] == image.name)
                    {
                        image.transform.Find("tick").GetComponent<Image>().enabled = true;
                        image.GetComponent<SelectItem>().selected = true;
                    }
                }
                else if(image.name.Contains("Legs")){


                    image.transform.SetParent(legsPanel.transform, false);
                    if (manager.player.DB_HeroLoadOut["legs"] == image.name)
                    {
                        image.transform.Find("tick").GetComponent<Image>().enabled = true;
                        image.GetComponent<SelectItem>().selected = true;
                    }
                }
                else if (image.name.Contains("Aura"))
                {

                    image.transform.SetParent(offhandPanel.transform, false);
                    if (manager.player.DB_HeroLoadOut["offhand"] == image.name)
                    {
                        image.transform.Find("tick").GetComponent<Image>().enabled = true;
                        image.GetComponent<SelectItem>().selected = true;
                    }
                }
                else if (image.name.Contains("Sword") || image.name.Contains("Club") || image.name.Contains("Katana") || image.name.Contains("Blade") || image.name.Contains("Edge"))
                {

                    image.transform.SetParent(weaponPanel.transform, false);

                    if (manager.player.DB_HeroLoadOut["weapon"] == image.name)
                    {
                        image.transform.Find("tick").GetComponent<Image>().enabled = true;
                        image.GetComponent<SelectItem>().selected = true;
                    }
                }

                //Image.transform.SetParent()

            }

        

        }




    }

    public void ShowSelectedItemPanel(string name) {


        StartCoroutine(ShowSelectedItemPane_Helper(name));


    }


    IEnumerator ShowSelectedItemPane_Helper(string name) {

        // Debug.Log(name);
       // Debug.Log(allPanelsForItemsParent.Count);
        for (int i = 0; i < allPanelsForItemsParent.Count; i++)
        {
            //Debug.Log(allPanelsForItemsParent.Count);

            if (name == allPanelsForItemsParent[i].name)
            {
                allPanelsForItemsParent[i].SetActive(true);
            }
            else
            {
                allPanelsForItemsParent[i].SetActive(false);

            }
            yield return null;
        }



    }

    #endregion






}
    