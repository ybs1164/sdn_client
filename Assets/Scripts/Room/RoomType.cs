using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomType : MonoBehaviour
{
    [SerializeField]
    private GameObject waiting;
    [SerializeField]
    private GameObject settingDeck;

    public void ChangeTheme()
    {
        if (waiting.activeSelf == true)
        {
            waiting.SetActive(false);
            settingDeck.SetActive(true);
        } else
        {
            waiting.SetActive(true);
            settingDeck.SetActive(false);
        }
    }

}
