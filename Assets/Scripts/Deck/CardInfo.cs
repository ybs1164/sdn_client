using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Card", order = 2)]
public class CardInfo : ScriptableObject
{
    public string cardName;

    public int id;

    public Sprite icon;

    public int cost;

    [TextArea]
    public string info;

    public List<UnitInfo> units;
}
