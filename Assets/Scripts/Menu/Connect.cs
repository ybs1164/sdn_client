using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Connect : MonoBehaviour
{
    [SerializeField] private Text text;

    public void Connecting()
    {
        if (!Client.Connect()) return;

        string tt = text.text;

        byte[] a = new byte[1]{0};
        byte[] b = Encoding.UTF8.GetBytes(tt);

        byte[] c = new byte[a.Length + b.Length + 1];

        Array.Copy(a, 0, c, 0, a.Length);
        Array.Copy(b, 0, c, a.Length, b.Length);

        Client.sendData(c);

        SceneManager.LoadScene("Room");
    }
}
