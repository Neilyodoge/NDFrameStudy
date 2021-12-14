using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigManager : ManagerBase<ConfigManager>
{
    [SerializeField]
    private ConfigSetting configSetting;

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <typeparam name="T">具体的配置类型</typeparam>
    /// <param name="configTypeName">配置类型名称</param>
    /// <param name="id">id</param>
    /// <returns></returns>
    public T GetConfig<T>(string configTypeName, int id) where T : ConfigBase
    {
        return configSetting.GetConfig<T>(configTypeName, id);
    }
}
