using UnityEngine;
using System.Collections;
using System.IO;
using SimpleJSON;

public class DataLoad : MonoBehaviour {

    
	public JSONNode unitData;
    public JSONNode cardData;
    public JSONNode heroData;
    public TextAsset unit_json;
    public TextAsset card_json;
    public TextAsset hero_json;



    void Awake () {

		unit_json = Resources.Load("unit") as TextAsset;
		var unitString = JSON.Parse(CryptographyProvider.DecryptText(unit_json.ToString(),"90abc"));
		unitData = unitString;


        hero_json = Resources.Load("hero") as TextAsset;
        var heroString = JSON.Parse(CryptographyProvider.DecryptText(hero_json.ToString(), "90abc"));
        heroData = heroString;


        card_json = Resources.Load("cards") as TextAsset;
        var cardString = JSON.Parse(CryptographyProvider.DecryptText(card_json.ToString(), "90abc"));
        cardData = cardString;

    }


	void Update () {

	}
}

