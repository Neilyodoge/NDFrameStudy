using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;   // 好像是二进制文件引入
using System;
using System.Linq;

/// <summary>
/// 一个存档的数据
/// </summary>
[Serializable] // 不加标签是没法进行二进制的读写或保存的
public class SaveItem
{
    public int saveID { get; private set; }
    public DateTime lastSaveTime { get; private set; }
    public SaveItem(int saveID, DateTime lastSaveTime)  // 构造函数
    {
        this.saveID = saveID;
        this.lastSaveTime = lastSaveTime;
    }

    public void UpdateTime(DateTime lastSaveTime)
    {
        this.lastSaveTime = lastSaveTime;
    }

}

/// <summary>
/// 存档管理器
/// </summary>
public static class SaveManager
{
    /// <summary>
    /// 存档管理器的设置数据
    /// </summary>
    [Serializable] // 不加标签是没法进行二进制的读写或保存的
    private class SaveManagerData
    {
        public int currID = 0;                                      // 当前存档ID
        public List<SaveItem> saveItemList = new List<SaveItem>();  // 所有存档列表
    }

    private static SaveManagerData saveManagerData;

    private const string saveDirName = "saveData";      // 存档的保存
    private const string settingDirName = "setting";    // 设置的保存 ： 1.全局数据的保存(分辨率、按键) 2.存档的设置保存。这个是不受玩家影响的
    private static readonly string saveDirPath;         // 存档文件夹路径
    private static readonly string settingDirPath;      // 设置文件夹路径

    // 存档中对象的缓存字典
    //                          <存档ID，<文件名称，实际的对象>>
    private static Dictionary<int, Dictionary<string, object>> cacheDic = new Dictionary<int, Dictionary<string, object>>();

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

        // 获取SaveManagerData
        InitSaveManagerData();
    }

    #region 存档设置
    /// <summary>
    /// 初始化存档管理器数据
    /// </summary>
    /// <returns></returns>
    private static void InitSaveManagerData()
    {
        saveManagerData = LoadFile<SaveManagerData>(saveDirPath + "/SaveManagerData");
        if (saveManagerData == null)
        {
            saveManagerData = new SaveManagerData();
            UpdataSaveManagerData();
        }
    }

    /// <summary>
    /// 更新存档管理器数据
    /// </summary>
    public static void UpdataSaveManagerData()
    {
        SaveFile(saveManagerData, saveDirPath + "/SaveManagerData");
    }

    /// <summary>
    /// 获取所有存档
    /// 最新的在最后面
    /// </summary>
    /// <returns></returns>
    public static List<SaveItem> GetSaveItems()
    {
        return saveManagerData.saveItemList;
    }

    /// <summary>
    /// 获取所有存档
    /// 最新的在最前面
    /// </summary>
    /// <returns></returns>
    public static List<SaveItem> GetAllSaveItemByCreatTime()
    {
        List<SaveItem> saveItems = new List<SaveItem>(saveManagerData.saveItemList.Count);
        for (int i = 0; i < saveManagerData.saveItemList.Count; i++)    // 对列表反转
        {
            saveItems[i] = saveManagerData.saveItemList[saveManagerData.saveItemList.Count - (i + 1)];
        }
        return saveItems;
    }

    /// <summary>
    /// 获取所有存档
    /// 最新更新的在最上面
    /// </summary>
    /// <returns></returns>
    public static List<SaveItem> GetAllSaveItemByUpdataTime()
    {
        List<SaveItem> saveItems = new List<SaveItem>(saveManagerData.saveItemList.Count);
        for (int i = 0; i < saveManagerData.saveItemList.Count; i++)
        {
            saveItems[i] = saveManagerData.saveItemList[i];
        }
        OrderByUnpdataTimeComparer orderBy = new OrderByUnpdataTimeComparer();  // 排序
        saveItems.Sort(orderBy);
        return saveItems;
    }

    private class OrderByUnpdataTimeComparer : IComparer<SaveItem>
    {
        public int Compare(SaveItem x, SaveItem y)  // 实现接口出来的
        {
            if (x.lastSaveTime > y.lastSaveTime)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }

    /// <summary>
    /// 获取所有存档
    /// 万能解决方案
    /// </summary>
    public static List<SaveItem> GetAllSaveItem<T>(Func<SaveItem, T> orderFunc, bool isDescending = false)
    {
        if (isDescending)
        {
            return saveManagerData.saveItemList.OrderByDescending(orderFunc).ToList();       // 这里用到了命名空间 Linq
        }
        else
        {
            return saveManagerData.saveItemList.OrderBy(orderFunc).ToList();       // 这里用到了命名空间 Linq
        }
        
        
    }
    #endregion

    #region 关于存档
    /// <summary>
    /// 获取SaveItem
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static SaveItem GetSaveItem(int id)
    {
        for (int i = 0; i < saveManagerData.saveItemList.Count; i++)
        {
            if (saveManagerData.saveItemList[i].saveID == id)
            {
                return saveManagerData.saveItemList[i];
            }
        }
        return null;
    }
    /// <summary>
    /// 添加一个存档
    /// </summary>
    /// <returns></returns>
    public static SaveItem CreateSaveItem()
    {
        SaveItem saveItem = new SaveItem(saveManagerData.currID, DateTime.Now);
        saveManagerData.saveItemList.Add(saveItem);
        saveManagerData.currID += 1;
        // 更新 saveManagerData 写入磁盘
        UpdataSaveManagerData();
        return saveItem;
    }
    /// <summary>
    /// 删除存档
    /// </summary>
    /// <param name="saveID">存档的ID</param>
    public static void DeleteSaveItem(int saveID)
    {
        string itemDir = GetSavePath(saveID, false);
        // 如果路径存在且有效
        if (itemDir != null)    
        {
            // 把这个存档下的文件递归删除
            Directory.Delete(itemDir, true);
        }
        saveManagerData.saveItemList.Remove(GetSaveItem(saveID));
        // 移除缓存
        RemoveCache(saveID);
        // 更新 saveManagerData 写入磁盘
        UpdataSaveManagerData();
    }
    public static void DeleteSaveItem(SaveItem saveItem)
    {
        string itemDir = GetSavePath(saveItem.saveID, false);
        // 如果路径存在且有效
        if (itemDir != null)
        {
            // 把这个存档下的文件递归删除
            Directory.Delete(itemDir, true);
        }
        saveManagerData.saveItemList.Remove(saveItem);
        // 移除缓存
        RemoveCache(saveItem.saveID);
        // 更新 saveManagerData 写入磁盘
        UpdataSaveManagerData();
    }
    #endregion

    #region 关于缓存
    /// <summary>
    /// 设置缓存
    /// </summary>
    /// <param name="saveID">存档ID</param>
    /// <param name="fileName">文件名称</param>
    /// <param name="saveObject">要缓存的对象</param>
    private static void SetCache(int saveID, string fileName, object saveObject)
    {
        // 缓存字典中是否有这个saveID
        if (cacheDic.ContainsKey(saveID))
        {
            // 这个存档中有没有这个文件
            if (cacheDic[saveID].ContainsKey(fileName))
            {
                cacheDic[saveID][fileName] = saveObject;
            }
            else
            {
                cacheDic[saveID].Add(fileName, saveObject);
            }
        }
        else
        {
            cacheDic.Add(saveID, new Dictionary<string, object>() { { fileName, saveObject } });
        }
    }

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <param name="saveID">存档ID</param>
    /// <param name="fileName">要获取的对象</param>
    /// <returns></returns>
    private static T GetCache<T>(int saveID, string fileName) where T : class
    {
        // 缓存字典中是否有这个saveID
        if (cacheDic.ContainsKey(saveID))
        {
            // 这个存档中有没有这个文件
            if (cacheDic[saveID].ContainsKey(fileName))
            {
                return cacheDic[saveID][fileName] as T;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 移除缓存
    /// </summary>
    /// <param name="saveID"></param>
    private static void RemoveCache(int saveID)
    {
        cacheDic.Remove(saveID);
    }
    #endregion

    #region 关于对象
    /// <summary>
    /// 保存对象至某个存档中
    /// </summary>
    /// <param name="saveObject">要保存的对象</param>
    /// <param name="saveFileName">保存的文件名称</param>
    /// <param name="saveID">存档的ID</param>
    public static void SaveObject(object saveObject, string saveFileName, int saveID = 0)   // 单存档不需要考虑saveID所以是0
    {
        string dirPath = GetSavePath(saveID, true);         // 存档所在的文件夹路径
        string savePath = dirPath + "/" + saveFileName;     // 具体的对象要保存的路径
        SaveFile(saveObject, savePath);                     // 具体的保存

        
        GetSaveItem(saveID).UpdateTime(DateTime.Now);       // 更新存档时间
        // 更新 saveManagerData 写入磁盘
        UpdataSaveManagerData();
        SetCache(saveID, saveFileName, saveObject);         // 更新缓存

    }

    public static void SaveObject(object saveObject, string saveFileName, SaveItem saveItem)
    {
        string dirPath = GetSavePath(saveItem.saveID, true);         // 存档所在的文件夹路径
        string savePath = dirPath + "/" + saveFileName;     // 具体的对象要保存的路径
        SaveFile(saveObject, savePath);                     // 具体的保存


        saveItem.UpdateTime(DateTime.Now);       // 更新存档时间
        // 更新 saveManagerData 写入磁盘
        UpdataSaveManagerData();
        SetCache(saveItem.saveID, saveFileName, saveObject);         // 更新缓存
    }

    /// <summary>
    /// 保存对象至某个存档中
    /// </summary>
    /// <param name="saveObject">要保存的对象</param>
    /// <param name="saveID">存档的ID</param>
    public static void SaveObject(object saveObject, int saveID = 0)
    {
        SaveObject(saveObject,saveObject.GetType().Name, saveID);
    }

    /// <summary>
    /// 保存对象至某个存档中
    /// </summary>
    /// <param name="saveObject">要保存的对象</param>
    public static void SaveObject(object saveObject, SaveItem saveItem)
    {
        SaveObject(saveObject, saveObject.GetType().Name, saveItem);
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
        T obj = GetCache<T>(saveID, saveFileName);
        if (obj == null)
        {
            // 存档所在的文件夹路径
            string dirPath = GetSavePath(saveID);
            if (dirPath == null) return null;
            // 具体的对象要保存的路径
            string savePath = dirPath + "/" + saveFileName;
            obj = LoadFile<T>(savePath);
            SetCache(saveID, saveFileName, obj);
        }
        return obj;
    }

    /// <summary>
    /// 从某个具体的存档中加载某个对象
    /// </summary>
    /// <typeparam name="T">要返回的实际类型</typeparam>
    /// <param name="saveFileName">文件名称</param>
    /// <returns></returns>
    public static T LoadObject<T>(string saveFileName, SaveItem saveItem) where T : class
    {
        return LoadObject<T>(saveFileName, saveItem.saveID);
    }

    /// <summary>
    /// 从某个具体的存档中加载某个对象
    /// </summary>
    /// <typeparam name="T">要返回的实际类型</typeparam>
    /// <param name="id">存档ID</param>
    /// <returns></returns>
    public static T LoadObject<T>(int saveID = 0) where T : class
    {
        return LoadObject<T>(typeof(T).Name, saveID);
    }

    /// <summary>
    /// 从某个具体的存档中加载某个对象
    /// </summary>
    /// <typeparam name="T">要返回的实际类型</typeparam>
    /// <returns></returns>
    public static T LoadObject<T>(SaveItem saveItem) where T : class
    {
        return LoadObject<T>(typeof(T).Name, saveItem.saveID);
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
        // 严正是否有某个存档
        if (GetSaveItem(saveID)==null) throw new Exception("ND:saveID 存档不存在！");

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
