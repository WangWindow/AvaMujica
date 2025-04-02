using System;
using AvaMujica.Models;

namespace AvaMujica.Services;

/// <summary>
/// 服务集合，管理所有应用服务
/// </summary>
public class ServiceCollection
{
    /// <summary>
    /// 单例实例
    /// </summary>
    private static ServiceCollection? _instance;

    /// <summary>
    /// 单例锁对象
    /// </summary>
    private static readonly object _lock = new();

    /// <summary>
    /// 获取单例实例
    /// </summary>
    public static ServiceCollection Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ServiceCollection();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// 数据库实例
    /// </summary>
    public SqliteDatabase Database { get; }

    /// <summary>
    /// 配置服务
    /// </summary>
    public ConfigService ConfigService { get; }

    /// <summary>
    /// 数据库服务
    /// </summary>
    public DatabaseService DatabaseService { get; }

    /// <summary>
    /// API服务
    /// </summary>
    public ApiService ApiService { get; }

    /// <summary>
    /// 历史服务
    /// </summary>
    public HistoryService HistoryService { get; }

    /// <summary>
    /// 聊天服务
    /// </summary>
    public ChatService ChatService { get; }

    /// <summary>
    /// 私有构造函数，确保只能通过单例访问
    /// </summary>
    private ServiceCollection()
    {
        try
        {
            // 初始化服务依赖链 - 按依赖顺序初始化
            Database = new SqliteDatabase();
            ConfigService = new ConfigService(Database);
            DatabaseService = new DatabaseService(Database);
            ApiService = new ApiService(ConfigService);
            HistoryService = new HistoryService(DatabaseService);
            ChatService = new ChatService(
                ConfigService,
                ApiService,
                DatabaseService,
                HistoryService
            );

            // 初始化配置
            ConfigService.InitializeDefaultConfig();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"服务初始化失败: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 重置服务实例（主要用于测试）
    /// </summary>
    public static void Reset()
    {
        lock (_lock)
        {
            _instance = null;
        }
    }
}
