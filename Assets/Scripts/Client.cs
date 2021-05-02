using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

public class Client : MonoBehaviour
{
    private static Chatting chatting = null;
    private static PlayerManager playerManager = null;
    private static GameManager gameManager = null;

    private static Socket m_ClientSocket = null;
    //private static TcpClient clientSocket = new TcpClient();
    //private static NetworkStream serverStream;

    private static Mutex mut = new Mutex();

    private const int m_port = 30004;

    private static bool gameScene = false;

    static byte[] receiveBytes = new byte[4096];

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    private void OnDestroy()
    {
        if (m_ClientSocket != null)
        {
            m_ClientSocket.Close();
        }
    }

    private void Update()
    {
        mut.WaitOne();
        if (gameScene)
        {
            SceneManager.LoadScene("Game");
            gameScene = false;
        }
        mut.ReleaseMutex();
    }

    public static void SetChatting(Chatting obj)
    {
        chatting = obj;
    }

    public static void SetPlayerManager(PlayerManager obj)
    {
        playerManager = obj;
    }

    public static void SetGameManager(GameManager obj)
    {
        gameManager = obj;
    }

    public static bool Connect()
    {
        /*
        try
        {
            clientSocket.Connect("numberer.iptime.org", m_port);

            serverStream = clientSocket.GetStream();

            Debug.Log("Successful to connect");

            return true;
        } catch
        {
            Debug.Log("Failed to connect");
        }
        */
        m_ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            m_ClientSocket.Connect("numberer.iptime.org", m_port);

            m_ClientSocket.BeginReceive(receiveBytes, 0, 4096, SocketFlags.None, new AsyncCallback(receiveDataCallback), m_ClientSocket);

            Debug.Log("Successful to connect");

            return true;

        } catch
        {
            Debug.Log("Failed to connect");
        }
        return false;
    }
    
    public static void sendData(byte[] data)
    {
        m_ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(sendDataCallback), m_ClientSocket);
    }
    
    static void receiveDataCallback(IAsyncResult ar)
    {
        int len = 0;
        try
        {
            len = m_ClientSocket.EndReceive(ar);
        } catch
        {
            Debug.Log("Server Error");
            return;
        }
        if (len == 0)
        {
            Debug.Log("Server Close");
            return;
        }


        int byteLen = 0;
        while (byteLen < len)
        {
            byte[] lenByte = new byte[2];
            Array.Copy(receiveBytes, byteLen, lenByte, 0, 2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenByte);
            int lenData = BitConverter.ToUInt16(lenByte, 0);
            byteLen += 2;
            byte type = receiveBytes[byteLen++];

            byte[] data = new byte[lenData - 1];

            Array.Copy(receiveBytes, byteLen, data, 0, lenData - 1);

            byteLen += lenData - 1;

            switch (type)
            {
                case 0: // 플레이어 정보 신호
                    int[] id = new int[3];
                    byte[] team = new byte[3];
                    string[] name = new string[3];
                    int i;

                    for (i = 0; i < data.Length / 23; i++)
                    {
                        byte[] subData = new byte[2];

                        Array.Copy(data, 0, subData, 0, 2);
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(subData);
                        id[i] = BitConverter.ToInt16(subData, 0);
                        Array.Copy(data, 2, data, 0, data.Length - 2);

                        team[i] = data[0];
                        Array.Copy(data, 1, data, 0, data.Length - 1);

                        subData = new byte[20];

                        Array.Copy(data, 0, subData, 0, 20);
                        name[i] = Encoding.UTF8.GetString(subData);

                        Array.Copy(data, 20, data, 0, data.Length - 20);
                    }

                    if (playerManager)
                        playerManager.SetPlayers(i, team, name);
                    break;
                case 1: // 게임 시작 신호
                    mut.WaitOne();
                    gameScene = true;
                    mut.ReleaseMutex();
                    break;
                case 2: // 채팅 받은 신호
                    chatting.AddText(Encoding.UTF8.GetString(data));
                    break;
                case 3: // 플레이어 정보 & 덱 신호 & 남은 시간
                    if (gameManager)
                    {
                        short ownId = new short();
                        short ownEnergy = new short();
                        short ownMaxEnergy = new short();
                        short Time = new short();

                        byte[] subData = new byte[2];

                        Array.Copy(data, 0, subData, 0, 2);
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(subData);
                        ownId = BitConverter.ToInt16(subData, 0);

                        Array.Copy(data, 2, subData, 0, 2);
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(subData);
                        ownEnergy = BitConverter.ToInt16(subData, 0);

                        Array.Copy(data, 4, subData, 0, 2);
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(subData);
                        ownMaxEnergy = BitConverter.ToInt16(subData, 0);

                        Array.Copy(data, 6, subData, 0, 2);
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(subData);
                        Time = BitConverter.ToInt16(subData, 0);

                        gameManager.PlayerSet(ownId, ownEnergy, ownMaxEnergy, Time);

                        byte[] orders = new byte[8];

                        Array.Copy(data, 8, orders, 0, 8);

                        gameManager.OrderSet(orders);

                        int[] cardIDs = new int[8];
                        short[] cardCosts = new short[8];

                        Array.Copy(data, 16, data, 0, data.Length - 16);

                        for (i = 0; i < 8; i++)
                        {
                            byte[] cardId = new byte[4];

                            Array.Copy(data, 0, cardId, 0, 4);
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(cardId);
                            cardIDs[i] = BitConverter.ToInt32(cardId, 0);

                            byte[] cardCost = new byte[2];

                            Array.Copy(data, 4, cardCost, 0, 2);
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(cardCost);
                            cardCosts[i] = BitConverter.ToInt16(cardCost, 0);

                            Array.Copy(data, 6, data, 0, data.Length - 6);
                        }

                        gameManager.DeckSet(cardIDs, cardCosts);
                    }
                    break;
                case 4: // 유닛들 신호
                    if (gameManager)
                    {
                        for (i=0;i<data.Length;)
                        {
                            ushort ownid;
                            ushort owntype;
                            byte ownteam;
                            float ownx;
                            float owny;
                            float ownangle;
                            uint ownhealth;
                            uint ownmh;
                            uint ownpoison;
                            byte ownstatus;

                            byte[] subData = new byte[8];


                            Array.Copy(data, 0, subData, 0, 2);
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(subData, 0, 2);

                            ownid = BitConverter.ToUInt16(subData, 0);

                            
                            Array.Copy(data, 2, subData, 0, 2);
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(subData, 0, 2);

                            owntype = BitConverter.ToUInt16(subData, 0);

                            ownteam = data[4];

                            if (ownteam < 3)
                            {

                                Array.Copy(data, 5, subData, 0, 8);
                                if (BitConverter.IsLittleEndian)
                                    Array.Reverse(subData);

                                ownx = (float)BitConverter.ToDouble(subData, 0);

                                Array.Copy(data, 13, subData, 0, 8);
                                if (BitConverter.IsLittleEndian)
                                    Array.Reverse(subData);

                                owny = (float)BitConverter.ToDouble(subData, 0);

                                Array.Copy(data, 21, subData, 0, 4);
                                if (BitConverter.IsLittleEndian)
                                    Array.Reverse(subData, 0, 4);

                                ownhealth = BitConverter.ToUInt16(subData, 0);

                                Array.Copy(data, 25, subData, 0, 4);
                                if (BitConverter.IsLittleEndian)
                                    Array.Reverse(subData, 0, 4);

                                ownmh = BitConverter.ToUInt16(subData, 0);

                                Array.Copy(data, 29, subData, 0, 4);
                                if (BitConverter.IsLittleEndian)
                                    Array.Reverse(subData, 0, 4);

                                ownpoison = BitConverter.ToUInt16(subData, 0);

                                ownstatus = data[33];

                                gameManager.SetObject(ownid, owntype, ownteam, ownx, owny, 0, ownhealth, ownmh, ownpoison, ownstatus);

                                Array.Copy(data, 34, data, 0, data.Length - 34);
                                i += 34;
                            } else
                            {
                                Array.Copy(data, 5, subData, 0, 8);
                                if (BitConverter.IsLittleEndian)
                                    Array.Reverse(subData);

                                ownx = (float)BitConverter.ToDouble(subData, 0);

                                Array.Copy(data, 13, subData, 0, 8);
                                if (BitConverter.IsLittleEndian)
                                    Array.Reverse(subData);

                                owny = (float)BitConverter.ToDouble(subData, 0);

                                Array.Copy(data, 21, subData, 0, 8);
                                if (BitConverter.IsLittleEndian)
                                    Array.Reverse(subData);

                                ownangle = (float)BitConverter.ToDouble(subData, 0);

                                Debug.Log(ownangle);

                                gameManager.SetObject(ownid, owntype, ownteam, ownx, owny, ownangle, 0, 0, 0, 0);

                                Array.Copy(data, 29, data, 0, data.Length - 29);
                                i += 29;
                            }
                        }
                        // todo
                        //gameManager.SetObjects(data);
                    }
                    break;
                case 5: // ping
                    Client.sendData(new byte[1]{ 5 });
                    break;
                case 6: // Delete Object
                    if (gameManager)
                    {
                        byte[] subData = new byte[2];

                        Array.Copy(data, 0, subData, 0, 2);
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(subData);

                        gameManager.DisObject(BitConverter.ToUInt16(subData, 0));
                    }
                    break;
                case 7: // Game End
                    if (gameManager)
                    {
                        gameManager.GameSet(data[0]);
                    }
                    break;
                case 8: // Create Object (Not Magic)
                    if (gameManager)
                    {
                        byte[] subData = new byte[8];

                        Array.Copy(data, 0, subData, 0, 8);
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(subData);

                        float ownx = (float)BitConverter.ToDouble(subData, 0);

                        Array.Copy(data, 8, subData, 0, 8);
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(subData);

                        float owny = (float)BitConverter.ToDouble(subData, 0);

                        Debug.Log(ownx + ", " + owny);
                    }
                    break;
                default:
                    break;
            }
        }

        m_ClientSocket.BeginReceive(receiveBytes, 0, 4096, SocketFlags.None, new AsyncCallback(receiveDataCallback), m_ClientSocket);
    }

    public static void sendDataCallback(IAsyncResult ar)
    {
        Socket sock = (Socket)ar.AsyncState;
        int len = sock.EndSend(ar);
    }
    
}
