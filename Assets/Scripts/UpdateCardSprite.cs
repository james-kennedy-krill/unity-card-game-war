using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateCardSprite : MonoBehaviour
{
    public Sprite cardFace;
    public Sprite cardBack;
    private Image cardImage;
    private Selectable selectable;
    private GameController gc;



    // Start is called before the first frame update
    void Start()
    {
        List<string> deck = GameController.GenerateDeck();
        gc = FindObjectOfType<GameController>();

        int i = 0;
        foreach (string card in deck)
        {
            if (this.name == card)
            {
                cardFace = gc.cardFaces[i];
                break;
            }
            i++;
        }
        cardImage = GetComponent<Image>();
        selectable = GetComponent<Selectable>();
    }

    // Update is called once per frame
    void Update()
    {
        if (selectable.faceUp == true)
        {
            cardImage.sprite = cardFace;
        }
        else {
            cardImage.sprite = cardBack;
        }
    }
}
