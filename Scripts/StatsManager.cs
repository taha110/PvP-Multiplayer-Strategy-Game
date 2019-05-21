using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Threading;
using System.IO;
using System;
using System.Linq;


public class StatsManager : MonoBehaviour
{


    private static StatsManager instance;
    public static StatsManager Instance
    {
        get
        {
            return instance;
        }
    }






    int winXP = 500;
    int lossXP = 200;


    

    void Awake()
    {
        if (instance != null && instance != this)
        {

            Destroy(gameObject);
        }



        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    
   


    private void FactionGameCounter()
    {
        switch (GameManager.Instance.factionName)
        {
            case "Animal":
                GameManager.Instance.player.DB_stats["AnimalGames"] += 1;
                break;
            case "Mythical":
                GameManager.Instance.player.DB_stats["MythicalGames"] += 1;
                break;
            case "Horror":
                GameManager.Instance.player.DB_stats["HorrorGames"] += 1;
                break;
            case "Human":
                GameManager.Instance.player.DB_stats["HumanGames"] += 1;
                break;
            case "Hybrid":
                GameManager.Instance.player.DB_stats["HybridGames"] += 1;
                break;
            default:
                break;
        }
    }

    public void GameWon()
    {
        GameManager.Instance.player.DB_stats["GamesWon"] += 1;
        GameManager.Instance.player.DB_stats["WinStreak"] += 1;


        if (GameManager.Instance.player.DB_stats["WinStreak"] > GameManager.Instance.player.DB_stats["BestWinStreak"])
        {
            GameManager.Instance.player.DB_stats["BestWinStreak"] = GameManager.Instance.player.DB_stats["WinStreak"];
        }
        UpdateXP(winXP);
        UpdateFactionWins();
       
    }

    public void GameLost()
    {
        GameManager.Instance.player.DB_stats["GamesLost"] += 1;
        GameManager.Instance.player.DB_stats["WinStreak"] = 0;
        UpdateXP(lossXP);
        UpdateFactionLoss();
       

    }

    private void UpdateFactionWins()
    {
        switch (GameManager.Instance.factionName)
        {
            case "Animal":
                GameManager.Instance.player.DB_stats["AnimalWin"] += 1;
                break;
            case "Mythical":
                GameManager.Instance.player.DB_stats["MythicalWin"] += 1;
                break;
            case "Horror":
                GameManager.Instance.player.DB_stats["HorrorWin"] += 1;
                break;
            case "Human":
                GameManager.Instance.player.DB_stats["HumanWin"] += 1;
                break;
            case "Hybrid":
                GameManager.Instance.player.DB_stats["HybridWin"] += 1;
                break;
            default:
                break;
        }

        

    }

    private void UpdateFactionLoss()
    {
        switch (GameManager.Instance.factionName)
        {
            case "Animal":
                GameManager.Instance.player.DB_stats["AnimalLoss"] += 1;
                break;
            case "Mythical":
                GameManager.Instance.player.DB_stats["MythicalLoss"] += 1;
                break;
            case "Horror":
                GameManager.Instance.player.DB_stats["HorrorLoss"] += 1;
                break;
            case "Human":
                GameManager.Instance.player.DB_stats["HumanLoss"] += 1;
                break;
            case "Hybrid":
                GameManager.Instance.player.DB_stats["HybridLoss"] += 1;
                break;
            default:
                break;
        }
    }

    private void UpdateXP(int xp)
    {
        GameManager.Instance.player.DB_stats["PlayerXP"] += xp;
        switch (GameManager.Instance.factionName)
        {
            case "Animal":
                GameManager.Instance.player.DB_stats["AnimalXP"] += xp;
                GameManager.Instance.player.DB_stats["AnimalLevelXp"] += xp;

                break;
            case "Mythical":
                GameManager.Instance.player.DB_stats["MythicalXP"] += xp;
                GameManager.Instance.player.DB_stats["MythicalLevelXp"] += xp;

                break;
            case "Horror":
                GameManager.Instance.player.DB_stats["HorrorXP"] += xp;
                GameManager.Instance.player.DB_stats["HorrorLevelXp"] += xp;

                break;
            case "Human":
                GameManager.Instance.player.DB_stats["HumanXP"] += xp;
                GameManager.Instance.player.DB_stats["HumanLevelXp"] += xp;

                break;
            case "Hybrid":
                GameManager.Instance.player.DB_stats["HybridXP"] += xp;
                GameManager.Instance.player.DB_stats["HybridLevelXp"] += xp;

                break;
            default:
                break;
        }
    }

    public void AddGameTime(float _time)
    {
        GameManager.Instance.player.DB_stats["TimePlayed"] += (int)_time;

    }

    public void AddCard(string _name)
    {
        GameManager.Instance.player.DB_stats["CardsUsed"] += 1;
        GameManager.Instance.player.DB_stats[_name + "Used"] +=1;
        CardsBase mcard = new CardsBase(_name);
        GameManager.Instance.player.DB_stats[mcard.Type + "Used"] +=1;
        

       

    }

    public void UnitKilled()
    {
        GameManager.Instance.player.DB_stats["UnitsKilled"] += 1;
    }

    public void UnitDied()
    {
        GameManager.Instance.player.DB_stats["UnitsLost"] += 1;
    }

    public void HeroDied()
    {
        GameManager.Instance.player.DB_stats["HeroDeaths"] += 1;

    }

    public void HeroKilled()
    {
        GameManager.Instance.player.DB_stats["HeroesKilled"] += 1;

    }

    public void UpdateTotalKillGold(int killGold)
    {
        GameManager.Instance.player.DB_stats["TotalKillGold"]  += killGold;
    }

    public void UnitAttacked(float damage)
    {
        GameManager.Instance.player.DB_stats["DamageDealt"] += (int)damage;
    }

    public void HeroAttacked(float damage)
    {
        GameManager.Instance.player.DB_stats["HeroDamageDealt"] += (int)damage;
    }

    public void AddUnit(string _name)
    {
        GameManager.Instance.player.DB_stats["UnitsTrained"] += 1;
        GameManager.Instance.player.DB_stats[_name+"Used"] += 1;

        GameManager.Instance.player.DB_stats["GoldSpent"] +=  GameObject.Find("Data").GetComponent<DataLoad>().unitData[_name][UnitAttributes.KillGold.GetHashCode()].AsInt;
    }

    public void AUnitRevived()
    {
        GameManager.Instance.player.DB_stats["UnitsRevived"] += 1;
    }

    






}

