using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Transform content;

    [SerializeField]
    private GameObject cardPrefab;

    [SerializeField]
    private GameObject unitPrefab;

    [SerializeField]
    private Text timeText;

    [SerializeField]
    private Text energyText;

    [SerializeField]
    private Camera cam;

    private CardInfo[] cardInfos;

    private Mutex mut = new Mutex();

    short ownId = 0;
    short ownEnergy = 0;
    short ownMaxEnergy = 0;

    short gameTime = 0;

    int[] cardIds = new int[8];
    short[] cardCosts = new short[8];

    byte[] cardOrder;

    bool isGameSet;
    byte winTeam;

    struct UnitData
    {
        public bool enable;
        public ushort type;
        public byte team;
        public float x, y, angle;
        public uint h, mh, p;
        public byte status;
    }

    private UnitData[] unitDatas = new UnitData[65536];

    private GameObject[] gameObjects = new GameObject[65536];
    private GameObject[] cardObjects;

    private List<GameObject> preview = new List<GameObject>();

    private void Awake()
    {
        isGameSet = false;

        cardInfos = Resources.LoadAll<CardInfo>("Cards");

        Array.Sort(cardInfos, delegate (CardInfo x, CardInfo y)
        {
            return x.id.CompareTo(y.id);
        });

        cardObjects = new GameObject[8];

        Client.SetGameManager(this);
    }

    public void SetObject(ushort id, ushort type, byte team, float x, float y, float angle, uint h, uint mh, uint p, byte status)
    {
        mut.WaitOne();
        unitDatas[id].type = type;
        unitDatas[id].team = team;
        unitDatas[id].x = x;
        unitDatas[id].y = y;
        unitDatas[id].angle = angle;
        unitDatas[id].h = h;
        unitDatas[id].mh = mh;
        unitDatas[id].p = p;
        unitDatas[id].status = status;
        unitDatas[id].enable = true;
        mut.ReleaseMutex();
    }

    public void DisObject(ushort id)
    {
        mut.WaitOne();
        unitDatas[id].enable = false;
        mut.ReleaseMutex();
    }

    public void PlayerSet(short id, short energy, short maxEnergy, short time)
    {
        mut.WaitOne();
        ownId = id;
        ownEnergy = energy;
        ownMaxEnergy = maxEnergy;
        gameTime = time;
        mut.ReleaseMutex();
    }

    public void DeckSet(int[] ids, short[] costs)
    {
        mut.WaitOne();
        cardIds = ids;
        cardCosts = costs;
        mut.ReleaseMutex();
    }

    public void OrderSet(byte[] orders)
    {
        mut.WaitOne();
        cardOrder = orders;
        mut.ReleaseMutex();
    }

    public void GameSet(byte team)
    {
        mut.WaitOne();
        isGameSet = true;
        winTeam = team;
        mut.ReleaseMutex();
    }

    public void SetPreview(CardInfo c)
    {
        ClearPreview();

        List<UnitInfo> unitList = new List<UnitInfo>();

        unitList = c.units;

        foreach (UnitInfo unit in unitList)
        {
            GameObject g = Instantiate(unitPrefab);

            preview.Add(g);

            Animator ani = g.GetComponent<Animator>();
            AnimationClip clip = ani.GetCurrentAnimatorClipInfo(0)[0].clip;

            RuntimeAnimatorController controller = ani.runtimeAnimatorController;
            AnimatorOverrideController overrideController = new AnimatorOverrideController();
            overrideController.runtimeAnimatorController = controller;
            overrideController[clip] = unit.anime;

            ani.runtimeAnimatorController = overrideController;
        }
    }

    public void ClearPreview()
    {
        foreach (GameObject obj in preview)
        {
            Destroy(obj);
        }

        preview = new List<GameObject>();
    }

    public void PosPreview(int x)
    {
        foreach (GameObject obj in preview)
        {
            obj.transform.position = new Vector2(x, 0);
        }
    }

    public void ShowPreview(bool isShow)
    {
        foreach (GameObject obj in preview)
        {
            if (isShow)
            {
                obj.GetComponent<SpriteRenderer>().color = new Color(255, 1, 1, 0.5f);
                obj.SetActive(true);
            }
            else
            {
                obj.SetActive(false);
            }
        }
    }

    void Update()
    {
        mut.WaitOne();

        if (isGameSet)
        {

            Debug.Log(winTeam);

            // todo : game end effect

            mut.ReleaseMutex();

            return;
        }

        timeText.text = gameTime / 60 + ":" + ((gameTime % 60 < 10)?"0":"") + gameTime % 60;

        energyText.text = ownEnergy + "";

        for (int i = 0; i < cardIds.Length; i++)
        {
            if (cardObjects[i] == null)
            {
                cardObjects[i] = Instantiate(cardPrefab, content);
                Card card = cardObjects[i].GetComponent<Card>();
                card.SetCard(cardInfos[cardIds[i]]);
            } else
            {
                Card card = cardObjects[i].GetComponent<Card>();
                card.SetCost(cardCosts[i]);
            }
        }

        if (cardOrder != null)
        {
            for (int i = 0; i < 8; i++)
            {
                if (cardObjects[cardOrder[i]] != null)
                {
                    if (cardObjects[cardOrder[i]].tag == "Card")
                    {
                        cardObjects[cardOrder[i]].SetActive((i / 4) == 0);
                        cardObjects[cardOrder[i]].GetComponent<Card>().order = i;

                        // 자식들의 맨 끝쪽으로 우선순위 교체
                        cardObjects[cardOrder[i]].GetComponent<Transform>().SetAsLastSibling();
                    }
                }
            }
        }

        for (int i = 0; i < 65536; i++)
        {
            if (unitDatas[i].enable)
            {
                if (!gameObjects[i])
                {
                    gameObjects[i] = Instantiate(unitPrefab);
                    gameObjects[i].GetComponent<SpriteRenderer>().sortingOrder = i-32768;
                }
                gameObjects[i].GetComponent<Unit>().SetUnit(unitDatas[i].type, unitDatas[i].team, unitDatas[i].x, unitDatas[i].y, unitDatas[i].angle, unitDatas[i].h, unitDatas[i].mh, unitDatas[i].p, unitDatas[i].status);
                gameObjects[i].SetActive(true);
            } else if (gameObjects[i])
            {
                // todo : Death Event
                gameObjects[i].SetActive(false);
            }
        }
        mut.ReleaseMutex();
    }
}
