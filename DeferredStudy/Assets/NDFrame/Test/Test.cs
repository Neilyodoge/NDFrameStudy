using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Test : MonoBehaviour
{
    void Start()
    {
        ResManager.Instance.Load<CubeController>("Cube");
    }

    private void Update()
    {

    }
}
