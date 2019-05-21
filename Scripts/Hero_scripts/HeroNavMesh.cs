using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;

public class HeroNavMesh : NetworkBehaviour
{

    NavMeshAgent pathFinder;

    bool canChangeDest = true;
    [SyncVar]
    public bool playerCommand = false;

    public float[] EnemyDistanceList;
    public GameObject[] EnemyList;

    float refreshRate = 0.5f;
    public string targetTag;

    float lineOfSight;

    bool updatePathRunnig = false;

    //public Transform targetToAttack;
    public Transform targetToAttackTransform;
    public Vector3 targetToAttackPos;


    [SyncVar]
    public Vector3 playerPos;

    // Use this for initialization
    void Start()
    {
        pathFinder = this.GetComponent<NavMeshAgent>();
        lineOfSight = this.GetComponent<HeroStats>().LineOfSight;
        //AssignAuthToClient();

        if (this.gameObject.tag.Contains("Player"))
        {
            targetTag = "Enemy";
        }
        else if (this.gameObject.tag.Contains("Enemy"))
        {
            targetTag = "Player";
        }

        EnemyList = GameObject.FindGameObjectsWithTag(targetTag);

    }


    //void Awake()
    //{


    //}

    private IEnumerator UpdatePath()
    {
        updatePathRunnig = true;
        EnemyList = GameObject.FindGameObjectsWithTag(targetTag);

        GameObject tBase = GameObject.FindGameObjectWithTag(targetTag + "Base");
        GameObject tHero = GameObject.FindGameObjectWithTag(targetTag + "Hero");
        Array.Resize<GameObject>(ref EnemyList, EnemyList.Length + 2);
        EnemyList[EnemyList.Length - 2] = tBase;
        EnemyList[EnemyList.Length - 1] = tHero;

        EnemyDistanceList = new float[EnemyList.Length];

        if (EnemyList.Length > 0)
        {

            for (int i = 0; i < EnemyList.Length; i++)
            {
                if (EnemyList[i] != null)
                {
                    EnemyDistanceList[i] = Vector3.Distance(EnemyList[i].transform.position, transform.position);
                    //  print("Distance to " + objName + " : " + EnemyDistanceList[i]);

                }
            }

            int tempIndex = 0;
            for (int i = 0; i < EnemyDistanceList.Length; i++)
            {
                if (EnemyList[i] != null)
                {
                    if (EnemyDistanceList[i] < EnemyDistanceList[tempIndex])
                    {
                        tempIndex = i;
                    }
                }
            }

            if (EnemyList[tempIndex] != null && EnemyDistanceList[tempIndex] < lineOfSight)
            {
                if (EnemyList[tempIndex].transform.name.Contains("wall"))
                {
                    transform.LookAt(new Vector3(EnemyList[tempIndex].transform.position.x, EnemyList[tempIndex].transform.position.y, this.transform.position.z));
                    pathFinder.SetDestination(new Vector3(EnemyList[tempIndex].transform.position.x, EnemyList[tempIndex].transform.position.y, this.transform.position.z));
                    targetToAttackTransform = EnemyList[tempIndex].transform;
                    targetToAttackPos = new Vector3(EnemyList[tempIndex].transform.position.x, EnemyList[tempIndex].transform.position.y, this.transform.position.z);
                }
                transform.LookAt(EnemyList[tempIndex].transform.position);
                pathFinder.SetDestination(EnemyList[tempIndex].transform.position);
                targetToAttackTransform = EnemyList[tempIndex].transform;
                targetToAttackPos = EnemyList[tempIndex].transform.position;
            }
            else {
                targetToAttackTransform = null;
                pathFinder.SetDestination(this.transform.position);
            }

        }

        yield return refreshRate;

        updatePathRunnig = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (canChangeDest && Input.GetMouseButtonUp(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (this.GetComponent<NetworkIdentity>().isServer && this.tag.Contains("Enemy"))
                {
                    playerPos = new Vector3(hit.point.x, this.transform.position.y, hit.point.z);
                    playerCommand = true;
                    sendRPCpos(playerPos, playerCommand);
                    pathFinder.SetDestination(playerPos);
                }
                else if (!this.GetComponent<NetworkIdentity>().isServer && this.tag.Contains("Player"))
                {
                    playerPos = new Vector3(hit.point.x, this.transform.position.y, hit.point.z);
                    playerCommand = true;
                    sendCmdPos(playerPos, playerCommand);
                    pathFinder.SetDestination(playerPos);
                }
            }
        }
        else if (playerCommand)
        {
            float distance = Vector3.Distance(this.transform.position, playerPos);
            if (distance <= 2f)
            {
                playerCommand = false;
            }
        }
        else {
            if (!updatePathRunnig)
            {
                StartCoroutine(UpdatePath());
            }
        }

    }


    void OnEnable()
    {
        CardMove.EventHeroDestChange += ChangeDestBool;
        AddButtons.EventHeroDestChange += ChangeDestBool;

    }


    void OnDisable()
    {
        CardMove.EventHeroDestChange -= ChangeDestBool;
        AddButtons.EventHeroDestChange -= ChangeDestBool;


    }


    [Server]
    void sendRPCpos(Vector3 playerPos, bool playerCommand)
    {
        RpcSendPos(playerPos, playerCommand);
    }

    [Client]
    void sendCmdPos(Vector3 playerPos, bool playerCommand)
    {
        //this.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToServer);
        //if (this.GetComponent<NetworkIdentity>().hasAuthority) {
        CmdSendPos(playerPos, playerCommand);
        //}
    }

    [Command]
    void CmdSendPos(Vector3 playerPos, bool playerCommand)
    {
        this.playerPos = playerPos;
        this.playerCommand = playerCommand;
        this.pathFinder.SetDestination(playerPos);
        //sendRPCpos(playerPos, playerCommand);
    }


    [ClientRpc]
    void RpcSendPos(Vector3 playerPos, bool playerCommand)
    {
        this.playerPos = playerPos;
        this.playerCommand = playerCommand;
        this.pathFinder.SetDestination(playerPos);
    }

    void ChangeDestBool(bool canChange)
    {
        canChangeDest = canChange;

    }

}
