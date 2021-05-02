using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    struct Player
    {
        public byte team;
        public string name;
    }

    [SerializeField]
    private Sprite[] teamSprites = new Sprite[3];

    [SerializeField]
    private Text[] texts = new Text[3];
    
    [SerializeField]
    private Image[] images = new Image[3];

    private static Mutex mut = new Mutex();

    private Player[] players = new Player[3];
    private int playerCount = 0;

    void Awake()
    {
        Client.SetPlayerManager(this);
    }

    private void Update()
    {
        mut.WaitOne();
        for (int i=0; i< playerCount; i++)
        {
            texts[i].text = players[i].name;
            images[i].sprite = teamSprites[players[i].team];
        }
        mut.ReleaseMutex();
    }

    public void SetPlayers(int count, byte[] team, string[] name)
    {
        mut.WaitOne();
        playerCount = count;
        for (int i=0; i<count; i++)
        {
            players[i].team = team[i];
            players[i].name = name[i];
        }
        mut.ReleaseMutex();
    }
}
