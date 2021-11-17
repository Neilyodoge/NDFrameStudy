using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// fileName = 创建出来文件的名字
[CreateAssetMenu(fileName = "GameConfig", menuName = "gameConfig/Setting")]
public class GameConfig : ScriptableObject
{
    public string a;
    public int b;
    public GameData gameData;
}

[Serializable]  // 这里需要序列化
public class GameData
{
    public float A;
    public List<GameData> gameDataList;
    public Dictionary<int, GameData> gameDataDic;  // 字典无效
}
