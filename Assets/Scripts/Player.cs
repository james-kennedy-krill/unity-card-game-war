using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public GameObject handArea;
    public GameObject playedArea;
    public GameObject discardPileArea;
    public Text cardsLeftText;
    public Text handTotalText;
    public Text discardPileTotalText;

    [SerializeField]
    private List<string> hand = new List<string>();
    [SerializeField]
    private string played = null;
    [SerializeField]
    private List<string> discardPile = new List<string>();

    private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        gameController = FindObjectOfType<GameController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddCardToHand(string cardName, Transform cardTransform)
    {
        hand.Add(cardName);
        cardTransform.SetParent(handArea.transform, false);
        CheckTotalCardCount();
    }

    public void ReceiveCards(List<string> cards)
    {
        discardPile.AddRange(cards);
        CheckTotalCardCount();
    }

    public void Play()
    {
        AudioSource.PlayClipAtPoint(gameController.clickCardSound, Camera.main.transform.position);
        gameController.PlayCards();
    }

    public string PlayCardFromHand(bool flipCard = true)
    {
        // move the card to the played
        played = hand[0];
        // remove the card from the hand
        hand.RemoveAt(0);

        // play the card to the played area and compare to oponent
        // TODO flip the card somehow?
        handArea.transform.GetChild(0).SetParent(playedArea.transform, false);
        AudioSource.PlayClipAtPoint(gameController.playCardSound, Camera.main.transform.position);
        if (flipCard) {
            playedArea.transform.GetChild(playedArea.transform.childCount-1).transform.GetComponent<Selectable>().faceUp = true;
        }

        UpdateHandAndDiscardPileTotals();

        return played;
    }

    public void ClearPlayed()
    {
        played = null;
        CheckTotalCardCount();
    }

    public void CheckHandCount()
    {
        if (hand.Count == 0)
        {
            if (discardPile.Count == 0) {
                gameController.EndGame(this);
            } else {
                ShuffleDiscardToHand();
            }
        }
    }

    private void CheckTotalCardCount()
    {
        int total = hand.Count + discardPile.Count;
        cardsLeftText.text = total.ToString();

        UpdateHandAndDiscardPileTotals();
    }

    private void UpdateHandAndDiscardPileTotals()
    {
        handTotalText.text = hand.Count.ToString();
        discardPileTotalText.text = discardPile.Count.ToString();
    }

    public void ShuffleDiscardToHand()
    {
        // shuffle the discard pile
        GameController.Shuffle(discardPile);
        AudioSource.PlayClipAtPoint(gameController.shuffleSound, Camera.main.transform.position);

        // Reset Rotation and flip over
        foreach (string card in discardPile)
        {
            Transform child = discardPileArea.transform.Find(card);
            Debug.Log(child);
            Debug.Log(child.rotation);
            child.GetComponent<LayoutElement>().preferredWidth = 140;
            child.Rotate(0, 0, 0, Space.Self);
            Debug.Log(child.rotation);
            child.GetComponent<Selectable>().faceUp = false;
            child.SetParent(handArea.transform, false);
        }

        // move them all to the hand
        var tempList = discardPile.Where(card => true).ToList();
        hand.AddRange(tempList);
        discardPile.Clear();

        CheckTotalCardCount();
    }
}
