using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Data.Sqlite;

namespace AvaMujica.Services;

/// <summary>
/// 数据库操作服务
/// </summary>
public class DatabaseService : IDisposable
{
    /// <summary>
    /// 单例实例
    /// </summary>
    private static DatabaseService? _instance;
    private static readonly Lock _lock = new();
    public static DatabaseService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new DatabaseService();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// SQLite连接对象
    /// </summary>
    private readonly SqliteConnection _connection;

    /// <summary>
    /// 数据库文件路径
    /// </summary>
    private readonly string _dbPath;

    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    private readonly string _connectionString;

    /// <summary>
    /// 互斥锁，确保并发操作安全访问数据库
    /// </summary>
    private readonly Lock _dbLock = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    private DatabaseService()
    {
        string dbFolder;

        try
        {
            dbFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AvaMujica"
            );

            Debug.WriteLine($"数据库路径: {dbFolder}");

            if (!Directory.Exists(dbFolder))
            {
                try
                {
                    Directory.CreateDirectory(dbFolder);
                    Debug.WriteLine($"成功创建数据库目录: {dbFolder}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"创建数据库目录失败: {ex.Message}");
                    throw new Exception($"无法创建数据库目录: {dbFolder}", ex);
                }
            }

            _dbPath = Path.Combine(dbFolder, "data.db");
            _connectionString = $"Data Source={_dbPath}";
            _connection = new SqliteConnection(_connectionString);

            Debug.WriteLine($"数据库文件路径: {_dbPath}");

            // 确保数据库和表已创建
            EnsureDatabaseCreated();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"数据库初始化失败: {ex.Message}");
            // 创建一个内存数据库作为备用方案
            _dbPath = ":memory:";
            _connectionString = "Data Source=:memory:";
            _connection = new SqliteConnection(_connectionString);
            EnsureDatabaseCreated();
            Debug.WriteLine("已切换到内存数据库作为备用");
        }
    }

    /// <summary>
    /// 确保数据库和表已创建
    /// </summary>
    private void EnsureDatabaseCreated()
    {
        try
        {
            _connection.Open();

            // 创建会话表
            using (var command = _connection.CreateCommand())
            {
                command.CommandText =
                    @"
                    CREATE TABLE IF NOT EXISTS ChatSessions (
                        Id TEXT PRIMARY KEY,
                        Title TEXT NOT NULL,
                        Type TEXT NOT NULL,
                        CreatedTime TEXT NOT NULL,
                        UpdatedTime TEXT NOT NULL
                    );";
                command.ExecuteNonQuery();
            }

            // 创建消息表
            using (var command = _connection.CreateCommand())
            {
                command.CommandText =
                    @"
                    CREATE TABLE IF NOT EXISTS ChatMessages (
                        Id TEXT PRIMARY KEY,
                        SessionId TEXT NOT NULL,
                        Role TEXT NOT NULL,
                        Content TEXT NOT NULL,
                        ReasoningContent TEXT,
                        SendTime TEXT NOT NULL,
                        FOREIGN KEY(SessionId) REFERENCES ChatSessions(Id) ON DELETE CASCADE
                    );";
                command.ExecuteNonQuery();
            }

            // 创建配置表
            using (var command = _connection.CreateCommand())
            {
                command.CommandText =
                    @"
                    CREATE TABLE IF NOT EXISTS Configs (
                        Key TEXT PRIMARY KEY,
                        Value TEXT NOT NULL
                    );";
                command.ExecuteNonQuery();
            }

            // 启用外键约束
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();
            }

            _connection.Close();
            Debug.WriteLine("数据库表创建成功");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"创建数据库表失败: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 执行不返回结果的SQL命令
    /// </summary>
    public int ExecuteNonQuery(string sql, Dictionary<string, object>? parameters = null)
    {
        lock (_dbLock)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
            }

            return command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// 执行返回单个值的SQL命令
    /// </summary>
    public object? ExecuteScalar(string sql, Dictionary<string, object>? parameters = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value);
            }
        }

        return command.ExecuteScalar();
    }

    /// <summary>
    /// 使用委托处理查询结果
    /// </summary>
    public void ExecuteReader(
        string sql,
        Action<SqliteDataReader> handleReader,
        Dictionary<string, object>? parameters = null
    )
    {
        lock (_dbLock)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
            }

            using var reader = command.ExecuteReader();
            handleReader(reader);
        }
    }

    /// <summary>
    /// 通用的查询方法
    /// </summary>
    private List<T> QueryInternal<T>(
        string sql,
        Func<SqliteDataReader, T> mapper,
        Dictionary<string, object>? parameters = null
    )
    {
        var result = new List<T>();

        lock (_dbLock)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(mapper(reader));
            }
        }

        return result;
    }

    /// <summary>
    /// 获取多行查询结果
    /// </summary>
    public List<T> Query<T>(
        string sql,
        Func<SqliteDataReader, T> mapper,
        Dictionary<string, object>? parameters = null
    )
    {
        return QueryInternal(sql, mapper, parameters);
    }

    public void Dispose() => _connection.Dispose();
}
