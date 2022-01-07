using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Test : MonoBehaviour
{
    CubeController cube;
    void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && cube==null)
        {
            ResManager.Instance.LoadGameObjectAsync<CubeController>("Cube", call);
        }
        if (Input.GetKeyDown(KeyCode.B) && cube != null)
        {
            PoolManager.Instance.PushGameObject(cube.gameObject);
            cube = null;
        }

    }
    void call(CubeController cubeController)
    {
        cube = cubeController;
    }
}
