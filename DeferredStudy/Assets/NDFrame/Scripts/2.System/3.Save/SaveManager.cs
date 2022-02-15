using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;   // 好像是二进制文件引入

/// <summary>
/// 存档管理器
/// </summary>
public static class SaveManager
{
    private const string saveDirName = "saveData";      // 存档的保存
    private const string settingDirName = "setting";    // 设置的保存 ： 1.全局数据的保存(分辨率、按键) 2.存档的设置保存。这个是不受玩家影响的
    private static readonly string saveDirPath;         // 存档文件夹路径
    private static readonly string settingDirPath;      // 设置文件夹路径
    static SaveManager()
    {
        // 初始化路径
        saveDirPath = Application.persistentDataPath + "/" + saveDirName;
        settingDirPath = Application.persistentDataPath + "/" + settingDirName;
        // 确保路径的存在
        if (Directory.Exists(saveDirPath) == false)
        {
            Directory.CreateDirectory(saveDirPath);
        }
        if (Directory.Exists(settingDirPath) == false)
        {
            Directory.CreateDirectory(settingDirPath);
        }
    }

    #region 关于对象
    /// <summary>
    /// 保存对象至某个存档中
    /// </summary>
    /// <param name="saveObject">要保存的对象</param>
    /// <param name="saveFileName">保存的文件名称</param>
    /// <param name="saveID">存档的ID</param>
    public static void SaveObject(object saveObject, string saveFileName, int saveID = 0)   // 单存档不需要考虑saveID所以是0
    {
        // 存档所在的文件夹路径
        string dirPath = GetSavePath(saveID, true);
        // 具体的对象要保存的路径
        string savePath = dirPath + "/" + saveFileName;
        // 具体的保存
        SaveFile(saveObject, savePath);
        // TODO: 更新存档时间
    }

    /// <summary>
    /// 从某个具体的存档中加载某个对象
    /// </summary>
    /// <typeparam name="T">要返回的实际类型</typeparam>
    /// <param name="saveFileName">文件名称</param>
    /// <param name="id">存档ID</param>
    /// <returns></returns>
    public static T LoadObject<T>(string saveFileName, int saveID = 0) where T : class
    {
        // 存档所在的文件夹路径
        string dirPath = GetSavePath(saveID);
        if (dirPath == null) return null;
        // 具体的对象要保存的路径
        string savePath = dirPath + "/" + saveFileName;
        return LoadFile<T>(savePath);
    }
    #endregion

    #region 工具函数

    private static BinaryFormatter binaryFormatter = new BinaryFormatter();

    /// <summary>
    /// 获取某个存档的路径
    /// </summary>
    /// <param name="saveID">存档ID</param>
    /// <param name="createDir">如果不存在这个路径，是否需要创建</param>
    /// <returns></returns>
    private static string GetSavePath(int saveID, bool createDir = true)
    {
        // TODO : 严正是否有某个存档

        string saveDir = saveDirPath + "/" + saveID;
        // 确定文件夹是否存在
        if (Directory.Exists(saveDir) ==false)
        {
            if (createDir)
            {
                Directory.CreateDirectory(saveDir);
            }
            else
            {
                return null;
            }
        }
        return saveDir;
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="saveObject">保存的对象</param>
    /// <param name="path">保存的路径</param>
    private static void SaveFile(object saveObject, string path)
    {
        FileStream f = new FileStream(path, FileMode.OpenOrCreate);
        // 二进制的方式把对象写进文件
        binaryFormatter.Serialize(f, saveObject);
        f.Dispose();
    }

    /// <summary>
    /// 加载文件
    /// </summary>
    /// <typeparam name="T">加载后要转为的类型</typeparam>
    /// <param name="path">加载路径</param>
    private static T LoadFile<T>(string path) where T : class
    {
        if (!File.Exists(path))
        {
            return null;
        }
        FileStream file = new FileStream(path, FileMode.Open);
        // 将内容解码成对象
        T obj = (T)binaryFormatter.Deserialize(file);
        file.Dispose();
        return obj;
    }

    #endregion
}
