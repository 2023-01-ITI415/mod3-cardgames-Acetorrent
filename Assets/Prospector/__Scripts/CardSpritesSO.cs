using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardSprites", menuName = "ScriptableObjects/CardSpritesSO")]

public class CardSpritesSO : ScriptableObject
{
    [Header("Card Stock")]
    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;
    public Sprite cardFrontSilver;
    public Sprite cardBackSilver;

    [Header("Suits")]
    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpade;

    [Header("Pip Sprites")]
    public Sprite[] faceSprites;
    public Sprite[] rankSprites;

    private static CardSpritesSO S;
    public static Dictionary<char, Sprite> SUITS { get; private set; }

    public void Init()
    {
        INIT_STATICS(this);

    }

    static void INIT_STATICS(CardSpritesSO cSSO)
    {
        if (S != null)
        {
            Debug.LogError("CardSpriteSO.S can't be set a 2nd time!");

            return;
        }
        S = cSSO;

        SUITS = new Dictionary<char, Sprite>()
        {
            { 'C', S.suitClub },
            { 'D', S.suitDiamond },
            { 'H', S.suitHeart },
            { 'S', S.suitSpade }
        };
    }

    public static Sprite[] RANKS
    {
        get { return S.rankSprites; }
    }

    public static Sprite GET_FACE(string name)
    {
        foreach(Sprite spr in S.faceSprites)
        {
            if (spr.name == name) return spr;

        }

        return null;
    }

    public static Sprite BACK
    {
        get { return S.cardBack; }
    }

    public static Sprite GET_SILVER_BACK
    {
        get { return S.cardBackSilver; }
    }

    public static Sprite GET_SILVER_FRONT
    {
        get { return S.cardFrontSilver; }
    }

    public static Sprite GET_GOLD_BACK
    {
        get { return S.cardBackGold; }
    }

    public static Sprite GET_GOLD_FRONT
    {
        get { return S.cardFrontGold; }
    }
    public static void RESET()
    {
        S = null;
    }
}

