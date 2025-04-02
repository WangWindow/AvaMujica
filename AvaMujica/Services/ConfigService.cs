using System;
using System.Collections.Generic;
using System.Linq;
using AvaMujica.Models;

namespace AvaMujica.Services;

/// <summary>
/// 配置服务
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="database">数据库实例</param>
public class ConfigService(SqliteDatabase database)
{
    /// <summary>
    /// 数据库实例
    /// </summary>
    private readonly SqliteDatabase _database = database;

    /// <summary>
    /// 获取指定键的配置
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>配置对象，如果未找到则返回null</returns>
    public Config? GetConfig(string key)
    {
        string sql = "SELECT Key, Value FROM Configs WHERE Key = @Key";
        var parameters = new Dictionary<string, object> { { "@Key", key } };

        return _database
            .Query(
                sql,
                reader => new Config { Key = reader.GetString(0), Value = reader.GetString(1) },
                parameters
            )
            .FirstOrDefault();
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
    public List<Config> GetAllConfigs()
    {
        string sql = "SELECT Key, Value FROM Configs";
        return _database.Query<Config>(
            sql,
            reader => new Config { Key = reader.GetString(0), Value = reader.GetString(1) }
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

        _database.ExecuteNonQuery(sql, parameters);
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

        int affectedRows = _database.ExecuteNonQuery(sql, parameters);
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

        long count = Convert.ToInt64(_database.ExecuteScalar(sql, parameters) ?? 0);
        return count > 0;
    }

    /// <summary>
    /// 加载完整的Config对象
    /// </summary>
    /// <returns>包含所有配置项的Config对象</returns>
    public Config LoadFullConfig()
    {
        var config = new Config();
        var allConfigs = GetAllConfigs();

        foreach (var item in allConfigs)
        {
            switch (item.Key)
            {
                case "ApiKey":
                    config.ApiKey = item.Value;
                    break;
                case "ApiBase":
                    config.ApiBase = item.Value;
                    break;
                case "Model":
                    config.Model = item.Value;
                    break;
                case "SystemPrompt":
                    config.SystemPrompt = item.Value;
                    break;
                case "ShowReasoning":
                    if (bool.TryParse(item.Value, out bool showReasoning))
                        config.ShowReasoning = showReasoning;
                    break;
                case "Temperature":
                    if (float.TryParse(item.Value, out float temperature))
                        config.Temperature = temperature;
                    break;
                case "MaxTokens":
                    if (int.TryParse(item.Value, out int maxTokens))
                        config.MaxTokens = maxTokens;
                    break;
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
        SetConfig("ApiKey", config.ApiKey);
        SetConfig("ApiBase", config.ApiBase);
        SetConfig("Model", config.Model);
        SetConfig("SystemPrompt", config.SystemPrompt);
        SetConfig("ShowReasoning", config.ShowReasoning.ToString());
        SetConfig("Temperature", config.Temperature.ToString());
        SetConfig("MaxTokens", config.MaxTokens.ToString());
    }

    /// <summary>
    /// 获取API密钥
    /// </summary>
    /// <returns>API密钥</returns>
    public string GetApiKey()
    {
        var value = GetValue("ApiKey", string.Empty);
        return string.IsNullOrEmpty(value)
            ? Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY") ?? string.Empty
            : value;
    }

    /// <summary>
    /// 设置API密钥
    /// </summary>
    /// <param name="apiKey">API密钥</param>
    public void SetApiKey(string apiKey)
    {
        SetConfig("ApiKey", apiKey);
    }

    /// <summary>
    /// 获取API基础地址
    /// </summary>
    /// <returns>API基础地址</returns>
    public string GetApiBase()
    {
        return GetValue("ApiBase", "https://api.deepseek.com");
    }

    /// <summary>
    /// 设置API基础地址
    /// </summary>
    /// <param name="apiBase">API基础地址</param>
    public void SetApiBase(string apiBase)
    {
        SetConfig("ApiBase", apiBase);
    }

    /// <summary>
    /// 获取模型名称
    /// </summary>
    /// <returns>模型名称</returns>
    public string GetModel()
    {
        return GetValue("Model", "deepseek-reasoner");
    }

    /// <summary>
    /// 设置模型名称
    /// </summary>
    /// <param name="model">模型名称</param>
    public void SetModel(string model)
    {
        SetConfig("Model", model);
    }

    /// <summary>
    /// 获取系统提示
    /// </summary>
    /// <returns>系统提示</returns>
    public string GetSystemPrompt()
    {
        return GetValue(
            "SystemPrompt",
            "你是一名优秀的心理咨询师，具有丰富的咨询经验。你的工作是为用户提供情感支持，解决用户的疑问。"
        );
    }

    /// <summary>
    /// 设置系统提示
    /// </summary>
    /// <param name="systemPrompt">系统提示</param>
    public void SetSystemPrompt(string systemPrompt)
    {
        SetConfig("SystemPrompt", systemPrompt);
    }

    /// <summary>
    /// 获取是否显示推理过程
    /// </summary>
    /// <returns>是否显示推理过程</returns>
    public bool GetShowReasoning()
    {
        var value = GetValue("ShowReasoning", "true");
        return bool.TryParse(value, out bool result) && result;
    }

    /// <summary>
    /// 设置是否显示推理过程
    /// </summary>
    /// <param name="showReasoning">是否显示推理过程</param>
    public void SetShowReasoning(bool showReasoning)
    {
        SetConfig("ShowReasoning", showReasoning.ToString());
    }

    /// <summary>
    /// 获取温度参数
    /// </summary>
    /// <returns>温度参数</returns>
    public float GetTemperature()
    {
        var value = GetValue("Temperature", "1.3");
        return float.TryParse(value, out float result) ? result : 1.3f;
    }

    /// <summary>
    /// 设置温度参数
    /// </summary>
    /// <param name="temperature">温度参数</param>
    public void SetTemperature(float temperature)
    {
        SetConfig("Temperature", temperature.ToString());
    }

    /// <summary>
    /// 获取最大令牌数
    /// </summary>
    /// <returns>最大令牌数</returns>
    public int GetMaxTokens()
    {
        var value = GetValue("MaxTokens", "2000");
        return int.TryParse(value, out int result) ? result : 2000;
    }

    /// <summary>
    /// 设置最大令牌数
    /// </summary>
    /// <param name="maxTokens">最大令牌数</param>
    public void SetMaxTokens(int maxTokens)
    {
        SetConfig("MaxTokens", maxTokens.ToString());
    }

    /// <summary>
    /// 初始化默认配置（如果不存在）
    /// </summary>
    public void InitializeDefaultConfig()
    {
        if (!ConfigExists("ApiBase"))
            SetConfig("ApiBase", "https://api.deepseek.com");

        if (!ConfigExists("Model"))
            SetConfig("Model", "deepseek-reasoner");

        if (!ConfigExists("SystemPrompt"))
            SetConfig(
                "SystemPrompt",
                "你是一名优秀的心理咨询师，具有丰富的咨询经验。你的工作是为用户提供情感支持，解决用户的疑问。"
            );

        if (!ConfigExists("ShowReasoning"))
            SetConfig("ShowReasoning", "true");

        if (!ConfigExists("Temperature"))
            SetConfig("Temperature", "1.3");

        if (!ConfigExists("MaxTokens"))
            SetConfig("MaxTokens", "2000");

        // API密钥不设置默认值，从环境变量获取
        if (string.IsNullOrEmpty(GetValue("ApiKey", "")))
        {
            var envApiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY");
            if (!string.IsNullOrEmpty(envApiKey))
                SetConfig("ApiKey", envApiKey);
        }
    }
}
