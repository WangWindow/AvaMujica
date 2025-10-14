using System;
using System.Collections.Generic;
using System.Data.Common;

namespace AvaMujica.Services;

public interface IDatabaseService
{
    int ExecuteNonQuery(string sql, Dictionary<string, object>? parameters = null);
    object? ExecuteScalar(string sql, Dictionary<string, object>? parameters = null);
    void ExecuteReader(
        string sql,
        Action<DbDataReader> handleReader,
        Dictionary<string, object>? parameters = null
    );
    List<T> Query<T>(
        string sql,
        Func<DbDataReader, T> mapper,
        Dictionary<string, object>? parameters = null
    );
}
