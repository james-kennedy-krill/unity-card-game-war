using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Settings")]
    public float timeToWait = 1.0f;
    public float timeToCompare = 2.0f;
    [Range(1,3)]
    public int numberOfFaceDownWarCards = 1;

    [Space]
    [Header("Prefabs and Game Objects")]
    public Sprite[] cardFaces;
    public GameObject cardPrefab;
    public GameObject deckStack;
    public Player[] players;

    [Space]
    [Header("Components")]
    public Animator warAnimator;
    public Animator winLoseAnimator;

    [Header("Sounds")]
    public AudioClip clickCardSound;
    public AudioClip playCardSound;
    public AudioClip warPreSound;
    public AudioClip warSound;
    public AudioClip warWinSound;
    public AudioClip warLoseSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip shuffleSound;

    public static string[] suits = new string[] { "C", "D", "H", "S" };
    public static string[] ranks = new string[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
    public List<string>[] playerCards;

    public List<string> deck;
    public List<string> player1Hand = new List<string>();
    public List<string> player2Hand = new List<string>();

    private bool playingCard = false;
    private bool autoPlay = false;

    // Start is called before the first frame update
    void Start()
    {
        warAnimator.gameObject.SetActive(false);
        winLoseAnimator.gameObject.SetActive(false);
        playerCards = new List<string>[] { player1Hand, player2Hand };
        GenerateCards();
    }

    public void GenerateCards()
    {
        deck = GenerateDeck();
        Shuffle(deck);
        SpawnCards();
    }

    public static List<string> GenerateDeck()
    {
        List<string> newDeck = new List<string>();
        foreach (string s in suits)
        {
            foreach (string r in ranks)
            {
                newDeck.Add(s + r);
            }
        }

        return newDeck;
    }

    public static void Shuffle<T>(List<T> list)
    {
        System.Random random = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            int k = random.Next(n);
            n--;
            T temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }

    void SpawnCards()
    {
        foreach (string card in deck)
        {
            GameObject newCard = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
            newCard.name = card;
            newCard.transform.SetParent(deckStack.transform, false);
        }
        Debug.Log("Cards in deck" + deckStack.transform.childCount);
    }

    public void Deal()
    {
        int totalCardsInDeck = deckStack.transform.childCount;
        AudioSource.PlayClipAtPoint(shuffleSound, Camera.main.transform.position);
        foreach (Player player in players)
        {
            for (int i = 0; i < totalCardsInDeck / 2; i++)
            {
                Transform card = deckStack.transform.GetChild(0);
                int deckCardIndex = deck.IndexOf(card.name);
                player.AddCardToHand(deck[deckCardIndex], card);
                deck.RemoveAt(deckCardIndex);

            }
        }
        GameObject.Find("Start Game").SetActive(false);
    }

    public void PlayCards() {
        // Prevent clicking before animations are done
        if (playingCard) return;
        playingCard = true;

        StartCoroutine(PlayCardsAndCompare());
    }

    IEnumerator PlayCardsAndCompare(bool war = false, List<string> player1stack = null, List<string> player2stack = null)
    {
        // play a card from each players hand to the played area
        string player1Card = players[0].PlayCardFromHand();
        if (!war) {
            yield return new WaitForSeconds(0.25f);
        }
        string player2Card = players[1].PlayCardFromHand();

        // compare each players played card
        Debug.Log("Player 1 played: " + player1Card);
        Debug.Log("Player 2 played: " + player2Card);
        int winningPlayerIndex = CompareCards(player1Card, player2Card);

        if (winningPlayerIndex == -1)
        {
            // TIE
            Debug.Log("It was a tie! ... but I don't know what to do yet with that.");
            StartCoroutine(War(player1Card, player2Card, player1stack, player2stack));
        }
        else
        {
            // SOMEONE WON
            // whoever wins receives both cards
            List<string> winningCards = new List<string>();
            if (war && player1stack != null && player2stack != null) {
                Debug.Log("war was won");
                List<string> winningStack = new List<string>();
                winningStack.AddRange(player1stack);
                winningStack.AddRange(player2stack);
                winningStack.Add(player1Card);
                winningStack.Add(player2Card);
                winningCards.AddRange(winningStack);
            } else {
                Debug.Log("regular was won");
                winningCards.Add(player1Card);
                winningCards.Add(player2Card);
            }

            for (int i = 0; i < winningCards.ToArray().Length; i++) {
                Debug.Log(winningCards[i]);
            }

            // Add cards to winners discard pile and reset played
            StartCoroutine(DeclareWinner(winningPlayerIndex, winningCards.ToArray(), war));
        }
    }

    IEnumerator War(string player1card, string player2card, List<string> player1stack = null, List<string> player2stack = null) {
        Debug.Log("WAR!!!");

        // Play pre-war sound to get you pumped...
        AudioSource.PlayClipAtPoint(warPreSound, Camera.main.transform.position);
        yield return new WaitForSeconds(2);

        // Animate in WAR!! Popup...
        warAnimator.gameObject.SetActive(true);
        warAnimator.SetTrigger("War");
        AudioSource.PlayClipAtPoint(warSound, Camera.main.transform.position);
        yield return new WaitForSeconds(1.2f);

        // Remove WAR!! Popup to ensure interaction is possible
        warAnimator.gameObject.SetActive(false);

        // Add first card played to stack, save for later...
        if (player1stack != null) {
            player1stack.Add(player1card);
        } else {
            player1stack = new List<string>() { player1card };
        }

        if (player2stack != null) {
            player2stack.Add(player2card);
        } else {
            player2stack = new List<string>() { player2card };
        }

        // deal face down cards, add to stack
        for(var i=0; i<numberOfFaceDownWarCards; i++) {
            // Player 1: Check
            if (!players[0].CheckHandCount()) break;
            string player1stackedcard = players[0].PlayCardFromHand(false);
            if (!players[1].CheckHandCount()) break;
            string player2stackedcard = players[1].PlayCardFromHand(false);
            player1stack.Add(player1stackedcard);
            player2stack.Add(player2stackedcard);
            yield return new WaitForSeconds(0.5f);
        }

        // Reshuffle if no cards left
        if(!players[0].CheckHandCount()) yield break;
        if (!players[1].CheckHandCount()) yield break;

        // play another card, and compare that card, pass the stack of cards
        StartCoroutine(PlayCardsAndCompare(true, player1stack, player2stack));
    }

    IEnumerator DeclareWinner(int winningPlayerIndex, string[] cards, bool war = false)
    {
        yield return new WaitForSeconds(timeToWait);

        if (war) {
            if (winningPlayerIndex == 0)
            {
                AudioSource.PlayClipAtPoint(warWinSound, Camera.main.transform.position);
            }
            else
            {
                AudioSource.PlayClipAtPoint(warLoseSound, Camera.main.transform.position);
            }
        } else {
            if (winningPlayerIndex == 0)
            {
                AudioSource.PlayClipAtPoint(winSound, Camera.main.transform.position);
            }
            else
            {
                AudioSource.PlayClipAtPoint(loseSound, Camera.main.transform.position);
            }
        }


        Transform winningCard = players[winningPlayerIndex].playedArea.transform.GetChild(players[winningPlayerIndex].playedArea.transform.childCount-1).transform;
        winningCard.GetComponent<Animator>().SetTrigger("Grow");

        if (war) {
            yield return new WaitForSeconds(2);
        } else {
            yield return new WaitForSeconds(timeToCompare);
        }


        winningCard.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        players[winningPlayerIndex].ReceiveCards(new List<string>(cards));
        int lastRotation = 0;
        foreach (Player player in players)
        {
            Debug.Log("Moving cards for player :");
            Debug.Log(player.ToString());
            Debug.Log(player.playedArea.transform.childCount);
            foreach (string card in cards) {
                Debug.Log(card);
                Transform cardToMove = player.playedArea.transform.Find(card);
                if (cardToMove == null) continue;
                Debug.Log(cardToMove.ToString());

                // Rotate the card slightly between
                cardToMove.GetComponent<LayoutElement>().preferredWidth = 160;
                int randomRotation = Random.Range(-10, 10);
                while (randomRotation == lastRotation)
                {
                    randomRotation = Random.Range(-10, 10);
                }
                lastRotation = randomRotation;
                cardToMove.Rotate(0, 0, randomRotation, Space.Self);

                // Move the cards from players Played Area to winner's discard pile
                cardToMove.transform.GetComponent<Selectable>().faceUp = true;
                cardToMove.SetParent(players[winningPlayerIndex].discardPileArea.transform);
            }

            player.ClearPlayed();
        }

        if (!players[0].CheckHandCount()) yield break;
        if (!players[1].CheckHandCount()) yield break;
        playingCard = false;

        if (autoPlay) {
            PlayCards();
        }
    }

    public void EndGame(Player losingPlayer)
    {

        Debug.Log("GAME OVER.");
        StopAllCoroutines();
        winLoseAnimator.gameObject.SetActive(true);
        // TODO create game over / lose/win screen
        if (losingPlayer == players[1]) {
            winLoseAnimator.SetTrigger("Win");
        } else {
            winLoseAnimator.SetTrigger("Lose");
        }
    }

    private int CompareCards(string player1Card, string player2Card) {
        // remove the first letter of the card string (the suit, doesn't matter)
        player1Card = player1Card.Substring(1);
        Debug.Log("Player 1 card value is: " + player1Card);
        player2Card = player2Card.Substring(1);
        Debug.Log("Player 2 card value is: " + player2Card);

        int p1 = 0;
        int p2 = 0;

        // if the remainder is a letter, transform it into a number
        if (!int.TryParse(player1Card, out p1)) {
            if (player1Card == "A") {
                p1 = 1;
            }
            if (player1Card == "J") {
                p1 = 11;
            }
            if (player1Card == "Q") {
                p1 = 12;
            }
            if (player1Card == "K") {
                p1 = 13;
            }
        }

        if (!int.TryParse(player2Card, out p2))
        {
            if (player2Card == "A")
            {
                p2 = 1;
            }
            if (player2Card == "J")
            {
                p2 = 11;
            }
            if (player2Card == "Q")
            {
                p2 = 12;
            }
            if (player2Card == "K")
            {
                p2 = 13;
            }
        }

        Debug.Log("P1 : " + p1 + ", P2 : " + p2);

        // compare the two remaining numbers
        if (p1 > p2) {
            return 0;
        } else if (p2 > p1) {
            return 1;
        }

        // It's a tie
        return -1;
    }

    public void OnAutoPlayToggle()
    {
        autoPlay = !autoPlay;
        if (autoPlay) {
            PlayCards();
        }
    }

}
