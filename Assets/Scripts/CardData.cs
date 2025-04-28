using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

[CreateAssetMenu(fileName = "NewCard" , menuName = "Cards/Card Data")]

public class CardData : ScriptableObject
{
    public enum CardType
    {
        Attack,
            Heal,
            Buff,
            Utility
    }
    public string cardName;
    public string description;
    public Sprite artwork;
    public int manaCost; public int effectAmount;
    public CardType cardType;

    public Color GetCardColor()
    {
        switch (cardType)
        {
            case CardType.Attack:
                return new Color(0.9f, 0.3f, 0.3f); //red
            case CardType.Heal:
                return new Color(0.3f, 0.9f, 0.3f); //G
            case CardType.Buff:
                return new Color(0.3f, 0.3f, 0.9f); //B
            case CardType.Utility:
                return new Color(0.9f, 0.9f, 0.3f); //Y
            default:
                return Color.white;
        }
    }
}