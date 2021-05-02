using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Deck : MonoBehaviour
{
    [SerializeField]
    private Transform content;

    [SerializeField]
    private Transform cardPrefab;

    [SerializeField]
    private GameObject selectObject;

    [SerializeField]
    private GameObject cardExplane;

    [SerializeField]
    private GraphicRaycaster m_gr;

    [SerializeField]
    private Transform[] emptyCards;

    PointerEventData m_ped;

    // 클릭에 감지된 오브젝트. 카드 오브젝트가 들어간다.
    GameObject rayObject;

    private Transform[] cardList;

    private int[] deck = new int[8];

    private void Awake()
    {
        CardInfo[] cardInfos = Resources.LoadAll<CardInfo>("Cards");

        Array.Sort(cardInfos, delegate (CardInfo x, CardInfo y)
        {
            return x.id.CompareTo(y.id);
        });

        m_ped = new PointerEventData(null);

        cardList = new Transform[cardInfos.Length];

        for (int i=0;i<cardInfos.Length;i++)
        {
            cardList[i] = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
            cardList[i].SetParent(content);
            cardList[i].localScale = Vector3.one;
            Card card = cardList[i].GetComponent<Card>();
            card.SetCard(cardInfos[i]);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_ped.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            m_gr.Raycast(m_ped, results);
            

            if (results.Count > 0)
            {
                // selectObject 의 버튼과 겹치지 않게 하는 if 문
                /*
                if (results[0].gameObject.transform.parent != selectObject.transform)
                {
                    rayObject = results[0].gameObject;

                    selectObject.transform.position = rayObject.transform.position;
                    selectObject.SetActive(true);
                }*/
                if (results[0].gameObject.tag == "Card")
                {
                    rayObject = results[0].gameObject;

                    selectObject.transform.position = rayObject.transform.position;
                    selectObject.transform.GetChild(1).gameObject.SetActive(true);
                    selectObject.transform.GetChild(2).gameObject.SetActive(false);
                    selectObject.SetActive(true);
                } else if (results[0].gameObject.tag == "UseCard")
                {
                    rayObject = results[0].gameObject;

                    selectObject.transform.position = rayObject.transform.position;
                    selectObject.transform.GetChild(1).gameObject.SetActive(false);
                    selectObject.transform.GetChild(2).gameObject.SetActive(true);
                    selectObject.SetActive(true);
                }
            } else
            {
                selectObject.SetActive(false);
            }
        }
    }

    public void InfoCard()
    {
        cardExplane.GetComponent<CardExplane>().SetCard(rayObject.GetComponent<Card>().cardInfo);
        cardExplane.SetActive(true);
    }

    public void AddCard()
    {
        for (int i = 0; i < 8; i++)
        {
            if (emptyCards[i].childCount == 0) {
                rayObject.transform.SetParent(emptyCards[i]);
                rayObject.transform.position = emptyCards[i].position;
                rayObject.tag = "UseCard";

                deck[i] = rayObject.GetComponent<Card>().GetID();
                SendDeck();
                break;
            }
        }

        selectObject.SetActive(false);
    }

    public void RemoveCard()
    {
        rayObject.transform.SetParent(content);
        rayObject.tag = "Card";

        for (int i = 0; i < 8; i++)
        {
            if (deck[i] == rayObject.GetComponent<Card>().GetID()) {
                deck[i] = 0;
            }
        }

        selectObject.SetActive(false);
    }

    public void SendDeck()
    {
        byte[] data = new byte[1+8 * 4];
        data[0] = 4;
        for (int i = 0; i < 8; i++)
        {
            byte[] bytes = BitConverter.GetBytes(deck[i]);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Array.Copy(bytes, 0, data, 4 * i + 1, 4);
        }
        Client.sendData(data);
    }
}
