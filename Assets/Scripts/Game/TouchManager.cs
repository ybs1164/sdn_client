using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TouchManager : MonoBehaviour
{
    [SerializeField]
    private GameManager gm;

    [SerializeField]
    private GraphicRaycaster m_gr;

    PointerEventData m_ped;

    private GameObject[] selectCardObj = new GameObject[2];

    private Vector2 sv;

    private void Awake()
    {
        m_ped = new PointerEventData(null);
    }

    void Update()
    {
        if (Input.touchCount >= 1)
        {
            m_ped.position = Input.GetTouch(0).position;
            List<RaycastResult> results = new List<RaycastResult>();
            m_gr.Raycast(m_ped, results);

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(m_ped.position);

            switch (Input.GetTouch(0).phase)
            {
                case TouchPhase.Began:
                    if (results.Count > 0)
                    {
                        if (selectCardObj[0])
                        {
                            selectCardObj[0].GetComponent<Transform>().localScale = Vector3.one;
                        }

                        selectCardObj[0] = results[0].gameObject;

                        selectCardObj[0].GetComponent<Transform>().localScale = new Vector3(1.1f, 1.1f, 1.1f);

                        sv = m_ped.position;

                        gm.SetPreview(selectCardObj[0].GetComponent<Card>().cardInfo);
                    }
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (selectCardObj[0])
                    {
                        Vector2 v = m_ped.position - sv;

                        if (m_ped.position.y > 180)
                        {
                            Vector2 unitPos = Camera.main.ScreenToWorldPoint(m_ped.position);
                            unitPos.x = Mathf.RoundToInt(unitPos.x);
                            unitPos.y = 0;

                            selectCardObj[0].SetActive(false);

                            gm.PosPreview(Mathf.RoundToInt(mousePos.x));
                            gm.ShowPreview(true);

                        } else
                        {
                            selectCardObj[0].SetActive(true);

                            gm.ShowPreview(false);

                            selectCardObj[0].GetComponent<Card>().SetDv(v);

                            float size = Mathf.Clamp((180 - m_ped.position.y) / (180 - sv.y) * 1.1f, 0, 1.1f);

                            selectCardObj[0].GetComponent<Transform>().localScale = new Vector3(size, size);
                        }
                    }
                    break;
                case TouchPhase.Ended:

                    if (selectCardObj[0])
                    {
                        selectCardObj[0].GetComponent<Transform>().localScale = Vector3.one;

                        selectCardObj[0].GetComponent<Card>().SetDv(Vector2.zero);
                        selectCardObj[0].SetActive(true);
                    }

                    if (m_ped.position.y > 180)
                    {
                        if (selectCardObj[0])
                        {
                            int order = selectCardObj[0].GetComponent<Card>().order;

                            byte[] data = new byte[4] { 3, (byte)order, 0, 0 };

                            byte[] bytes = BitConverter.GetBytes((short)Mathf.RoundToInt(mousePos.x));

                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(bytes);

                            Array.Copy(bytes, 0, data, 2, 2);

                            Client.sendData(data);

                            selectCardObj[0] = null;
                        }
                    }
                    
                    // todo : waiting signal
                    gm.ShowPreview(false);
                    
                    break;
                case TouchPhase.Canceled:
                    Debug.Log("What?");
                    break;
                default:
                    break;
            }
        }
        else if (Input.touchCount > 1)
        {
            // todo
        }
    }
}
