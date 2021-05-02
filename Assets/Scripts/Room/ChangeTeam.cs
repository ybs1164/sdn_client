using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTeam : MonoBehaviour
{
    public void changeTeam(int team)
    {
        byte[] a = new byte[2] { 1, (byte)team };

        Client.sendData(a);
    }
}
