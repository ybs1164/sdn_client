using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField]
    private Text costText;

    [SerializeField]
    private Image cardIcon;

    public CardInfo cardInfo;

    int id = 0;
    public int order = -1; // 게임 실행 시에만 적용됨. 주어진 덱의 순서를 뜻함.

    private Vector2 dv = Vector2.zero;

    private void Update()
    {
        if (order > -1)
        {
            float x = (order - 2) * 600 + 300;

            transform.localPosition = new Vector2(x, 431);

            transform.position = transform.position + (Vector3)dv;
        }
    }

    public void SetDv(Vector2 dpos)
    {
        dv = dpos;
    }

    public void SetCard(CardInfo cardinfo)
    {
        if (cardinfo.id == 0)
        {
            gameObject.tag = "UseCard";
            gameObject.SetActive(false);
            return;
        }
        cardInfo = cardinfo;
        id = cardinfo.id;
        costText.text = cardinfo.cost.ToString();
        cardIcon.sprite = cardinfo.icon;
    }

    public void SetCost(int cost)
    {
        costText.text = cost.ToString();
    }

    public int GetID()
    {
        return id;
    }
}
