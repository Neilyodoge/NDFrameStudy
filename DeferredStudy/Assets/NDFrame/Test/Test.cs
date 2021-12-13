using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Test : MonoBehaviour
{
    public GameObject go;
    void Start()
    {
        PoolManager.Instance.GetGameObject(go);
        PoolManager.Instance.GetGameObject(go);
        PoolManager.Instance.GetGameObject(go);
        GameObject go2 = PoolManager.Instance.GetGameObject(go);
        PoolManager.Instance.PushGameObject(go2);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            PoolManager.Instance.ClearGameObject(go);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            GameObject go1 = PoolManager.Instance.GetGameObject(go);
            GameObject go2 = PoolManager.Instance.GetGameObject(go);
            GameObject go3 = PoolManager.Instance.GetGameObject(go);
            PoolManager.Instance.PushGameObject(go1);
            PoolManager.Instance.PushGameObject(go2);
            PoolManager.Instance.PushGameObject(go3);

        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            PoolManager.Instance.ClearAllGameObject();
        }
    }
}
