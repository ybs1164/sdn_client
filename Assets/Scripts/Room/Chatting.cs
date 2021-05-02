using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Chatting : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private Text text;
    [SerializeField] private InputField inputText;

    string t = "";

    void Awake()
    {
        Client.SetChatting(this);
    }

    private void Update()
    {
        text.text = t;
    }

    public void chat()
    {
        string s = inputText.text;

        byte[] a = new byte[1]{2};
        byte[] b = Encoding.UTF8.GetBytes(s);

        byte[] c = new byte[a.Length + b.Length + 1];

        Array.Copy(a, 0, c, 0, a.Length);
        Array.Copy(b, 0, c, a.Length, b.Length);

        Client.sendData(c);

        // todo : player's name
        //AddText(name + ": " + s);
    }

    public void AddText(string data)
    {
        t += data + "\n";
    }
}
