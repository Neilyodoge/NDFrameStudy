using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Test : MonoBehaviour
{
    
    void Start()
    {
        DemoConfig config = ConfigManager.Instance.GetConfig<DemoConfig>("武器", 1);
        Debug.Log(config.Name);
    }

    private void Update()
    {

    }
}
