using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardExplane : MonoBehaviour
{
    [SerializeField]
    private Card card;

    [SerializeField]
    private Text cardNameUI;

    [SerializeField]
    private Text cardExplaneUI;

    public void SetCard(CardInfo cardinfo)
    {
        card.SetCard(cardinfo);
        cardNameUI.text = cardinfo.cardName;
        cardExplaneUI.text = cardinfo.info;
    }
}
