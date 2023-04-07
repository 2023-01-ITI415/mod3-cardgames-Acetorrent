using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement; // We’ll need this line later in the chapter

[RequireComponent(typeof(Deck))]
// a
[RequireComponent(typeof(JsonParseLayout))]
public class Prospector : MonoBehaviour
{
    private static Prospector S; // A private Singleton for Prospector

    [Header("Inscribed")]
    public float roundDelay = 2f;
    [Header("Dynamic")]
    public List<CardProspector> drawPile;
    public List<CardProspector> discardPile;
    public List<CardProspector> mine;
    public List<CardProspector> potentialSpecialCards;
    public List<float> specialCardChance = new List<float> {1f, 1f, 1f, 1f, 1f };
    public CardProspector target;

    public CardProspector [,] gameBoard = new CardProspector[5, 5];


    private Transform layoutAnchor;
    private Deck deck;
    private JsonLayout jsonLayout;
    // A Dictionary to pair mine layout IDs and actual Cards
    private Dictionary<int, CardProspector> mineIdToCardDict;



    void Start()
    {
        
        string sceneName = SceneManager.GetActiveScene().name;

        // Set the private Singleton. We’ll use this later.
        if (S != null) Debug.LogError("Attempted to set S more than once!"); // b
        S = this;

        jsonLayout = GetComponent<JsonParseLayout>().layout;

        deck = GetComponent<Deck>();
        // These two lines replace the Start() call we commented out in Deck
        deck.InitDeck();
        Deck.Shuffle(ref deck.cards);
        drawPile = ConvertCardsToCardProspectors(deck.cards);
        LayoutMine();

        convertToSilver();

        convertToGold();
        //DO Check and convert Cards here.

        MoveToTarget(Draw());

        // Set up the draw pile
        UpdateDrawPile();
    }

    /// <summary>
    /// Converts each Card in a List(Card) into a List(CardProspector) so that it
    /// can be used in the Prospector game.
    /// </summary>
    /// <param name="listCard">A List(Card) to be converted</param>
    /// <returns>A List(CardProspector) of the converted cards</returns>
    List<CardProspector>
    ConvertCardsToCardProspectors(List<Card> listCard)
    {
        List<CardProspector> listCP = new List<CardProspector>();
        CardProspector cp;
        foreach (Card card in listCard)
        {
            cp = card as CardProspector;
            // c
            listCP.Add(cp);
        }
        return (listCP);
    }

    /// <summary> 
    /// Pulls a single card from the beginning of the drawPile and returns it > 
    /// Note: There is no protection against trying to draw from an empty pile!
    /// </summary>
    /// <returns>The top card of drawPile</returns>
    CardProspector Draw()
    {
        CardProspector cp = drawPile[0]; // Pull the 0th CardProspector
        drawPile.RemoveAt(0); // Then remove it from drawPile
        return (cp); // And return it
    }

    /// <summary>
    /// Positions the initial tableau of cards, a.k.a. the "mine"
    /// </summary>
    void LayoutMine()
    {
        // Create an empty GameObject to serve as an anchor for the tableau // a
        if (layoutAnchor == null)
        {
            // Create an empty GameObject named _LayoutAnchor in the Hierarchy
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform; // Grab its Transform
        }

        CardProspector cp;
        // b
        mineIdToCardDict = new Dictionary<int, CardProspector>();


        // Iterate through the JsonLayoutSlots pulled from the JSON_Layout
        foreach (JsonLayoutSlot slot in jsonLayout.slots)
        {
            cp = Draw(); // Pull a card from the top (beginning) of the draw Pile
            cp.faceUp = slot.faceUp; // Set its faceUp to the value in SlotDef
                                     // Make the CardProspector a child of layoutAnchor
            cp.transform.SetParent(layoutAnchor);

            // Convert the last char of the layer string to an int (e.g. "Row 0")
            int z = int.Parse(slot.layer[slot.layer.Length - 1].ToString());
            // Set the localPosition of the card based on theslot information
            cp.SetLocalPos(new Vector3(
            jsonLayout.multiplier.x * slot.x,
            jsonLayout.multiplier.y * slot.y,
            -z));
            // d

            cp.layoutID = slot.id;
            cp.layoutSlot = slot;
            // CardProspectors in the mine have the state CardState.mine
            cp.state = eCardState.mine;
            // Set the sorting layer of all SpriteRenderers on the Card
            cp.SetSpriteSortingLayer(slot.layer);
            mine.Add(cp); // Add this CardProspector to the List<> mine
            potentialSpecialCards.Add(cp); //Add the cards inside the array of possible special cards.

            mineIdToCardDict.Add(slot.id, cp);
        }


    }

    void convertToSilver()
    {
        foreach (float chance in specialCardChance)
        {
            if (Random.value <= chance)
            {
                int locale = Random.Range(0, potentialSpecialCards.Count);
               
                CardProspector tcp;
                tcp = potentialSpecialCards[locale];
                potentialSpecialCards.RemoveAt(locale);
                tcp.cardType = eCardType.silver;

                SpriteRenderer tsr = tcp.GetComponent<SpriteRenderer>();
                tsr.sprite = CardSpritesSO.GET_SILVER_FRONT;


                GameObject tgo = tcp.transform.Find("back").gameObject;
                tsr = tgo.GetComponent<SpriteRenderer>();
                tsr.sprite = CardSpritesSO.GET_SILVER_BACK;
            }

            
        }
        
    }

    void convertToGold()
    {
        foreach (float chance in specialCardChance)
        {
            if (Random.value <= chance)
            {
                int locale = Random.Range(0, potentialSpecialCards.Count);

                CardProspector tcp;
                tcp = potentialSpecialCards[locale];
                potentialSpecialCards.RemoveAt(locale);
                tcp.cardType = eCardType.gold;

                SpriteRenderer tsr = tcp.GetComponent<SpriteRenderer>();
                tsr.sprite = CardSpritesSO.GET_GOLD_FRONT;


                GameObject tgo = tcp.transform.Find("back").gameObject;
                tsr = tgo.GetComponent<SpriteRenderer>();
                tsr.sprite = CardSpritesSO.GET_GOLD_BACK;
            }


        }
    }

    /// <summary>
    /// Moves the current target card to the discardPile
    /// </summary>
    /// <param name="cp">The CardProspector to be moved</param>
    void MoveToDiscard(CardProspector cp)
    {
        // Set the state of the card to discard > 
        cp.state = eCardState.discard;
        discardPile.Add(cp); // Add it to the discardPile List<>
        cp.transform.SetParent(layoutAnchor); // Update its transform parent

        // Position it on the discardPile
        cp.SetLocalPos(new Vector3(
        jsonLayout.multiplier.x * jsonLayout.discardPile.x,
        jsonLayout.multiplier.y * jsonLayout.discardPile.y,
        0));

        cp.faceUp = true;

        // Place it on top of the pile for depth sorting

        cp.SetSpriteSortingLayer(jsonLayout.discardPile.layer);
        // a
        cp.SetSortingOrder(-200 + (discardPile.Count * 3));
        // b
    }

    /// <summary>
    /// Make cp the new target card
    /// </summary>
    /// <param name="cp">The CardProspector to be moved</param>
    void MoveToTarget(CardProspector cp)
    {
        // If there is currently a target card, move it to discardPile
        if (target != null) MoveToDiscard(target);

        // Use MoveToDiscard to move the target card to the correct location
        MoveToDiscard(cp);
        // c

        // Then set a few additional things to make cp the new target
        target = cp; // cp is the new target
        cp.state = eCardState.target;

        // Set the depth sorting so that cp is on top of the discardPile
        cp.SetSpriteSortingLayer("Target");
        // c
        cp.SetSortingOrder(0);
    }

    void MoveToSlot(CardProspector cp)
    {
        if (target == null) return;

        target.SetLocalPos(new Vector3(
                jsonLayout.multiplier.x * jsonLayout.slots[CardProspector.GET_SLOT(cp)].x,
                jsonLayout.multiplier.y * jsonLayout.slots[CardProspector.GET_SLOT(cp)].y,
                0));
        target.faceUp = true;


        gameBoard[(int)Mathf.Floor(CardProspector.GET_SLOT(cp) / 5), CardProspector.GET_SLOT(cp) - (5 *((int)Mathf.Floor(CardProspector.GET_SLOT(cp) / 5)))] = target;

        Debug.Log(gameBoard[(int)Mathf.Floor(CardProspector.GET_SLOT(cp) / 5), CardProspector.GET_SLOT(cp) - (5 * ((int)Mathf.Floor(CardProspector.GET_SLOT(cp) / 5)))]);
        target = null;
        // Place it on top of the pile for depth sorting

        cp.SetSpriteSortingLayer(jsonLayout.drawPile.layer + 1);

    }

    /// <summary>
    /// Arranges all the cards of the drawPile to show how many are left
    /// </summary>
    void UpdateDrawPile()
    {
        CardProspector cp;
        // Go through all the cards of the drawPile
        for (int i = 0; i < drawPile.Count; i++)
        {
            cp = drawPile[i];
            cp.transform.SetParent(layoutAnchor);

            // Position it correctly with the layout.drawPile.stagger
            Vector3 cpPos = new Vector3();
            cpPos.x = jsonLayout.multiplier.x * jsonLayout.drawPile.x;
            // Add the staggering for the drawPile
            cpPos.x += jsonLayout.drawPile.xStagger * i;
            cpPos.y = jsonLayout.multiplier.y * jsonLayout.drawPile.y;
            cpPos.z = 0.1f * i;
            cp.SetLocalPos(cpPos);

            cp.faceUp = false; // DrawPile Cards are all face-down
            cp.state = eCardState.drawpile;
            // Set depth sorting

            cp.SetSpriteSortingLayer(jsonLayout.drawPile.layer);
            cp.SetSortingOrder(-10 * i);
        }
    }

    /// <summary>
    /// This turns cards in the Mine face-up and face-down
    /// </summary>
    public void SetMineFaceUps()
    {
        // d
        CardProspector coverCP;
        foreach (CardProspector cp in mine)
        {
            bool faceUp = true; // Assume the card will be face- up

            // Iterate through the covering cards by mine layout ID
            foreach (int coverID in cp.layoutSlot.hiddenBy)
            {
                coverCP = mineIdToCardDict[coverID];
                // If the covering card is null or still in the mine...
                if (coverCP == null || coverCP.state == eCardState.mine)
                {
                    faceUp = false; // then this card is face- down
                }
            }
            cp.faceUp = faceUp; // Set the value on the card
        }
    }


    static public void CARD_CLICKED(CardProspector cp)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        // The reaction is determined by the state of the clicked card
        switch (cp.state)
        {
            case eCardState.target:
                // Clicking the target card does nothing
                break;
            case eCardState.drawpile:

                if (sceneName == "GameScene") return;
                // Clicking *any* card in the drawPile will draw the next card
                // Call two methods on the Prospector Singleton S
                S.MoveToTarget(S.Draw()); // Draw a new target card
                S.UpdateDrawPile(); // Restack the drawPile
                ScoreManager.TALLY(eScoreEvent.draw);
                break;
            case eCardState.mine:
                // More to come here
                // a
                // Clicking a card in the mine will check if it’s a valid play
                bool validMatch = true; // Initially assume that it’s valid

                // If the card is face-down, it’s not valid
                if (!cp.faceUp) validMatch = false;

                if (sceneName == "GameScene")
                {
                    if (cp.faceUp) return;


                    S.MoveToSlot(cp);
                    S.MoveToTarget(S.Draw()); // Draw a new target card
                    S.UpdateDrawPile(); // Restack the drawPile




                    Debug.Log("LayoutID: " + CardProspector.GET_SLOT(cp));
                    //
                }

                if (sceneName != "GameScene")
                {
                    // If it’s not an adjacent rank, it’s not valid
                    if (!cp.AdjacentTo(S.target)) validMatch = false;
                    // b

                    if (validMatch)
                    { // If it’s a valid card
                        S.mine.Remove(cp); // Remove it from the tableau List 
                        S.MoveToTarget(cp); // Make it the target card
                        S.SetMineFaceUps();
                        ScoreManager.TALLY(eScoreEvent.mine);

                        if (cp.cardType == eCardType.silver)
                        {
                            ScoreManager.TALLY(eScoreEvent.mine);
                        }
                        else if (cp.cardType == eCardType.gold)
                        {
                            //none
                        }
                        // Add a if silver card click check here
                    }
                }
                break;
        }
        S.CheckForGameOver();

    }
    /// <summary>
    /// Test whether the game is over
    /// </summary>
    void CheckForGameOver()
    {
        // a
        // If the mine is empty, the game is over
        if (mine.Count == 0)
        {
            GameOver(true); // Call GameOver() with a win
            return;
        }

        // If there are still cards in the mine & draw pile, the game’s not over > 
        if (drawPile.Count > 0)
            return;

        // Check for remaining valid plays
        foreach (CardProspector cp in mine)
        {

            // If there is a valid play, the game’s not over
            if (target.AdjacentTo(cp))
                return;
        }

        // Since there are no valid plays, the game is over
        GameOver(false); // Call GameOver with a loss
    }


    /// <summary>
    /// Called when the game is over. Simple for now, but expandable
    /// </summary>
    /// <param name="won">true if the player won</param>
    void GameOver(bool won)
    {
        if (won)
        {
            ScoreManager.TALLY(eScoreEvent.gameWin);
            // b
        }
        else
        {
            //Debug.Log("Game Over. You Lost. :("); 
            // a
            ScoreManager.TALLY(eScoreEvent.gameLoss);
        }
        // Reset the CardSpritesSO singleton to null
        CardSpritesSO.RESET();
        // b
        // Reload the scene, resetting the game
        // Note that there are TWO underscores at the  beginning of "__Prospector…
        // But wait a moment first, giving the final score a moment to travel
        Invoke("ReloadLevel", roundDelay);
        UITextManager.GAME_OVER_UI(won);


    }

    void ReloadLevel()
    {

        // Reload the scene, resetting the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);


    }



}
