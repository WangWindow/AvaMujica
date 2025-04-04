using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using AvaMujica.Models;

namespace AvaMujica.Services;

#pragma warning disable CS8604 // 引用类型参数可能为 null

/// <summary>
/// 配置服务
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
public class ConfigService
{
    private readonly DatabaseService _databaseService = DatabaseService.Instance;

    /// <summary>
    /// 单例实例
    /// </summary>
    private static ConfigService? _instance;
    private static readonly Lock _lock = new();
    public static ConfigService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ConfigService();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// 获取指定键的配置
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>配置对象，如果未找到则返回null</returns>
    public ConfigAdapter? GetConfig(string key)
    {
        string sql = "SELECT Key, Value FROM Configs WHERE Key = @Key";
        var parameters = new Dictionary<string, object> { { "@Key", key } };

        var configs = _databaseService.Query(
            sql,
            reader => new ConfigAdapter { Key = reader.GetString(0), Value = reader.GetString(1) },
            parameters
        );

        return configs.Count > 0 ? configs[0] : null;
    }

    /// <summary>
    /// 获取指定键的配置值
    /// </summary>
    /// <param name="key">配置键</param>
    /// <param name="defaultValue">默认值（如果配置不存在）</param>
    /// <returns>配置值，不存在则返回默认值</returns>
    public string GetValue(string key, string defaultValue = "")
    {
        var config = GetConfig(key);
        return config?.Value ?? defaultValue;
    }

    /// <summary>
    /// 获取所有配置
    /// </summary>
    /// <returns>配置列表</returns>
    public List<ConfigAdapter> GetAllConfigs()
    {
        string sql = "SELECT Key, Value FROM Configs";
        return _databaseService.Query<ConfigAdapter>(
            sql,
            reader => new ConfigAdapter { Key = reader.GetString(0), Value = reader.GetString(1) }
        );
    }

    /// <summary>
    /// 设置配置
    /// </summary>
    /// <param name="key">配置键</param>
    /// <param name="value">配置值</param>
    public void SetConfig(string key, string value)
    {
        string sql =
            @"
            INSERT INTO Configs (Key, Value)
            VALUES (@Key, @Value)
            ON CONFLICT(Key) DO UPDATE SET Value = @Value";

        var parameters = new Dictionary<string, object> { { "@Key", key }, { "@Value", value } };
        _databaseService.ExecuteNonQuery(sql, parameters);
    }

    /// <summary>
    /// 删除配置
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>是否成功删除</returns>
    public bool DeleteConfig(string key)
    {
        string sql = "DELETE FROM Configs WHERE Key = @Key";
        var parameters = new Dictionary<string, object> { { "@Key", key } };

        int affectedRows = _databaseService.ExecuteNonQuery(sql, parameters);
        return affectedRows > 0;
    }

    /// <summary>
    /// 检查配置是否存在
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>是否存在</returns>
    public bool ConfigExists(string key)
    {
        string sql = "SELECT COUNT(*) FROM Configs WHERE Key = @Key";
        var parameters = new Dictionary<string, object> { { "@Key", key } };

        long count = Convert.ToInt64(_databaseService.ExecuteScalar(sql, parameters) ?? 0);
        return count > 0;
    }

    /// <summary>
    /// 尝试将配置值转换为指定类型
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="dbConfig">配置对象</param>
    /// <param name="property">属性信息</param>
    /// <param name="config">配置对象实例</param>
    private bool TryConvertAndSetValue<T>(
        ConfigAdapter dbConfig,
        PropertyInfo property,
        Config config
    )
    {
        if (typeof(T) == typeof(string))
        {
            property.SetValue(config, dbConfig.Value);
            return true;
        }
        else
        {
            try
            {
                if (typeof(T) == typeof(bool) && bool.TryParse(dbConfig.Value, out bool boolValue))
                {
                    property.SetValue(config, boolValue);
                    return true;
                }
                else if (
                    typeof(T) == typeof(float)
                    && float.TryParse(dbConfig.Value, out float floatValue)
                )
                {
                    property.SetValue(config, floatValue);
                    return true;
                }
                else if (typeof(T) == typeof(int) && int.TryParse(dbConfig.Value, out int intValue))
                {
                    property.SetValue(config, intValue);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"转换失败: {property.Name} - {ex.Message}");
            }
        }
        return false;
    }

    /// <summary>
    /// 加载完整的Config对象
    /// </summary>
    /// <returns>包含所有配置项的Config对象</returns>
    public Config LoadFullConfig()
    {
        // 创建一个默认配置对象
        var config = new Config();

        // 从数据库获取所有配置项
        var allConfigs = GetAllConfigs();
        var properties = typeof(Config).GetProperties();
        var configDict = allConfigs.ToDictionary(c => c.Key);

        foreach (var property in properties)
        {
            // 检查属性是否存在于数据库配置中
            if (configDict.TryGetValue(property.Name, out var dbConfig))
            {
                // 根据属性类型转换并设置值
                if (property.PropertyType == typeof(string))
                {
                    TryConvertAndSetValue<string>(dbConfig, property, config);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    TryConvertAndSetValue<bool>(dbConfig, property, config);
                }
                else if (property.PropertyType == typeof(float))
                {
                    TryConvertAndSetValue<float>(dbConfig, property, config);
                }
                else if (property.PropertyType == typeof(int))
                {
                    TryConvertAndSetValue<int>(dbConfig, property, config);
                }
            }
            else
            {
                // 如果数据库中不存在该配置，则使用默认值
                var defaultValue = property.GetValue(config);
                if (defaultValue != null)
                {
                    // 获取当前配置值
                    var currentConfig = GetConfig(property.Name);

                    // 仅当默认值与当前值不同时才写入数据库
                    if (currentConfig == null || currentConfig.Value != defaultValue.ToString())
                    {
                        SetConfig(property.Name, defaultValue.ToString());
                        Console.WriteLine($"写入默认配置: {property.Name}={defaultValue}");
                    }
                }
            }
        }

        return config;
    }

    /// <summary>
    /// 保存完整的Config对象到数据库
    /// </summary>
    /// <param name="config">要保存的配置对象</param>
    public void SaveFullConfig(Config config)
    {
        var properties = typeof(Config).GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(config);
            if (value != null)
            {
                SetConfig(property.Name, value.ToString());
            }
        }
    }

    /// <summary>
    /// 初始化默认配置
    /// </summary>
    public void InitializeDefaultConfig()
    {
        var defaultConfig = new Config();
        var properties = typeof(Config).GetProperties();

        foreach (var property in properties)
        {
            var propertyName = property.Name;
            if (!ConfigExists(propertyName))
            {
                var value = property.GetValue(defaultConfig);
                if (value != null)
                {
                    SetConfig(propertyName, value.ToString());
                    Console.WriteLine($"初始化默认配置: {propertyName}={value}");
                }
            }
        }
    }

    /// <summary>
    /// 重置所有配置为默认值
    /// </summary>
    public void ResetAllToDefaults()
    {
        var defaultConfig = new Config();
        SaveFullConfig(defaultConfig);
        Console.WriteLine("所有配置已重置为默认值");
    }
}
