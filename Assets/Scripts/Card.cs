using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Suit {
    Clubs=1,
    Diamonds=2,
    Hearts=3,
    Spades=4
}

public enum Rank {
    Ace=1,
    Two=2,
    Three=3,
    Four=4,
    Five=5,
    Six=6,
    Seven=7,
    Eight=8,
    Ninte=9,
    Ten=10,
    Jack=11,
    Queen=12,
    King=13
}

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class Card : ScriptableObject
{
    public Sprite sprite;
    public Suit suit;
    public Rank rank;

}
