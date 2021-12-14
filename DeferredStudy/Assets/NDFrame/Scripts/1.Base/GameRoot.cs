using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameRoot 是伴随整个游戏存在的，所以不会被销毁
/// </summary>
public class GameRoot : SingletonMono<GameRoot>
{
    /// <summary>
    /// 框架设置
    /// </summary>
    [SerializeField]
    private GameSetting gameSetting;
    public GameSetting GameSetting { get { return gameSetting; } }  // 为了让上面的gameSetting 只读
    protected override void Awake()
    {
        if (Instance != null)   // 先判断单例是否存在
        {
            // 如果已经存在就销毁gameRoot
            Destroy(gameObject);
            return;
        }
        base.Awake();   // 只是赋值
        DontDestroyOnLoad(gameObject);  // 切换场景时候不要销毁gameRoot
        InitMangers();                  // 初始化所有管理器
    }

    private void InitMangers()  // 初始化所有管理器
    {
        ManagerBase[] managers = GetComponents<ManagerBase>();  // 找到所有管理器
        for (int i = 0; i < managers.Length; i++)
        {
            managers[i].Init();     // 对所有管理器进行初始化
        }
    }
}
