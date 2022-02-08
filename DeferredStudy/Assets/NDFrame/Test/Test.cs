using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Test : MonoBehaviour
{
    void Start()
    {

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            AudioManager.Instance.PlayBGAudio("Menu");
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            AudioManager.Instance.IsPause = true;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            AudioManager.Instance.IsPause = false;
        }
    }


}
