using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : ManagerBase<PoolManager>
{
    public override void Init()
    {
        base.Init();
        Debug.Log("PoolManager 初始化成功");
    }
}
