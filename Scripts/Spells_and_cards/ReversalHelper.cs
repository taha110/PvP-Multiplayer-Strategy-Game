using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ReversalHelper : NetworkBehaviour
{
    [SyncVar]
    public string cName;


    public delegate void LoadReverseCard(string cardName);
    public static event LoadReverseCard EventLoadReverseCard;

    // Use this for initialization
    void Start()
    {
        //cName = "nothing";
        StartCoroutine(work());
    }

    IEnumerator work()
    {
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForSeconds(0.25f);

            if (i == 0)
            {
                if (isServer != this.GetComponent<NetPlayerCheck>().IsPlayer1)
                {
                    if (isServer)
                    {
                        RpcSyncReverse(cName);
                    }
                    else {
                        CmdSyncReverse(cName);
                    }
                }
                else
                {

                }
            }
            if (i == 1)
            {
                if (isServer != this.GetComponent<NetPlayerCheck>().IsPlayer1)
                {
                    LoadCard();
                }
                else
                {
                    Destroy(this.gameObject, 1f);
                }
            }
        }
    }


    void LoadCard()
    {
        //GameObject.Find("Reversal").GetComponentInChildren<BreakerCards>().CardName = cName;
        EventLoadReverseCard(cName);
        Destroy(this.gameObject, 1f);
    }

    [ClientRpc]
    void RpcSyncReverse(string _name)
    {
        //reverseCardName = name;
        //print("sending RpcSyncReverse -  Cname :  " + _name);
        this.cName = _name;
    }
    [Command]
    void CmdSyncReverse(string _name)
    {
        //reverseCardName = name;
        //print("sending CmdSyncReverse-  Cname :  " + _name);
        this.cName = _name;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
