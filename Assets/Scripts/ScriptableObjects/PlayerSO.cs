using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSO", menuName = "Player")]
public class PlayerSO : ScriptableObject
{
    public string playerName;
    public Sprite playerFlag;
    public Color32 playerColor;
    public Sprite playerSprite;
    public Sprite playerGoldImage;
    public Sprite playerCoatImage;
    public Sprite playerDeckImage;
    public Nationality playerNationality;

    [Range (0,5)]
    public int playerAIAgrresivness;

    [Range(-10, 5)]
    public int AICardDeployMinimalFactor;

    [Range(-9, 0)]
    public int AIUnitAttackMinimalFactor;

    public int startMorale;
    public int startGold;
    public List<int> incomeList;
    public int startMaxGold;

    public List<CardSO> startDeck;
    private List<CardSO> battleDeck;
    private List<CardSO> storageDeck;

    
}
