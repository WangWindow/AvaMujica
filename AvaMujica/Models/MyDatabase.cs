using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Data.Sqlite;

namespace AvaMujica.Models;

/// <summary>
/// SQLite数据库操作类
/// </summary>
public class SqliteDatabase : IDisposable
{
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
    public SqliteDatabase()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbFolder = Path.Combine(appDataPath, "AvaMujica");

        if (!Directory.Exists(dbFolder))
        {
            Directory.CreateDirectory(dbFolder);
        }

        _dbPath = Path.Combine(dbFolder, "data.db");
        _connectionString = $"Data Source={_dbPath}";
        _connection = new SqliteConnection(_connectionString);

        // 确保数据库和表已创建
        EnsureDatabaseCreated();
    }

    /// <summary>
    /// 确保数据库和表已创建
    /// </summary>
    private void EnsureDatabaseCreated()
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
                        CreatedAt TEXT NOT NULL,
                        UpdatedAt TEXT NOT NULL
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
                        Time TEXT NOT NULL,
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
    }

    /// <summary>
    /// 执行不返回结果的SQL命令
    /// </summary>
    /// <param name="sql">SQL命令</param>
    /// <param name="parameters">命令参数</param>
    /// <returns>影响的行数</returns>
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
    /// <param name="sql">SQL命令</param>
    /// <param name="parameters">命令参数</param>
    /// <returns>查询结果第一行第一列的值</returns>
    public object? ExecuteScalar(string sql, Dictionary<string, object>? parameters = null)
    {
        _connection.Open();
        try
        {
            using var command = _connection.CreateCommand();
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
        finally
        {
            _connection.Close();
        }
    }

    /// <summary>
    /// 使用委托处理查询结果
    /// </summary>
    /// <param name="sql">SQL命令</param>
    /// <param name="handleReader">处理SqliteDataReader的委托</param>
    /// <param name="parameters">命令参数</param>
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
    /// 获取多行查询结果
    /// </summary>
    /// <typeparam name="T">返回对象类型</typeparam>
    /// <param name="sql">SQL命令</param>
    /// <param name="mapper">从SqliteDataReader映射到对象的函数</param>
    /// <param name="parameters">命令参数</param>
    /// <returns>映射后的对象列表</returns>
    public List<T> Query<T>(
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

    public void Dispose() => _connection.Dispose();
}
