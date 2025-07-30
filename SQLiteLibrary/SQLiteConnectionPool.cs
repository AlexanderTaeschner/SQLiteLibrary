// <copyright file="SQLiteConnectionPool.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

namespace SQLiteLibrary;

/// <summary>
/// Provides a resource pool that enables reusing instances of prepared SQL statements.
/// </summary>
/// <typeparam name="TEnum">Enum type used as key to the SQL texts.</typeparam>
public sealed class SQLiteConnectionPool<TEnum> : IDisposable
    where TEnum : Enum
{
    private readonly Dictionary<TEnum, byte[]> _sqlTextDictionary;
    private readonly Func<SQLiteConnection> _createSQLiteConnection;

    private readonly Lock _lock = new();

    private readonly Dictionary<TEnum, List<SQLitePooledStatement<TEnum>>> _pool = [];

    private readonly List<SQLitePooledStatement<TEnum>> _rentedStatements = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SQLiteConnectionPool{TEnum}"/> class.
    /// </summary>
    /// <param name="sqlTextDictionary">Dictionary with SQL texts.</param>
    /// <param name="createSQLiteConnection">Function used to create a new SQLiteConnection instance.</param>
    public SQLiteConnectionPool(
        IReadOnlyDictionary<TEnum, string> sqlTextDictionary,
        Func<SQLiteConnection> createSQLiteConnection)
    {
        _sqlTextDictionary = new Dictionary<TEnum, byte[]>(sqlTextDictionary.Select(kvp => new KeyValuePair<TEnum, byte[]>(kvp.Key, NativeMethods.ToUtf8Bytes(kvp.Value))));
        _createSQLiteConnection = createSQLiteConnection;
    }

    private SQLiteConnectionPool() => throw new NotSupportedException();

    /// <summary>
    /// Retrieve a prepared SQL statement.
    /// </summary>
    /// <param name="sqlTextKey">Key to the SQL text.</param>
    /// <returns>A prepared SQL statement.</returns>
    public SQLitePooledStatement<TEnum> Rent(TEnum sqlTextKey)
    {
        lock (_lock)
        {
            if (!_pool.TryGetValue(sqlTextKey, out List<SQLitePooledStatement<TEnum>>? objList))
            {
                objList = [];
                _pool.Add(sqlTextKey, objList);
            }

            SQLitePooledStatement<TEnum> poolStmt;
            if (objList.Count > 0)
            {
                int i = objList.Count - 1;
                poolStmt = objList[i];
                objList.RemoveAt(i);
                return poolStmt;
            }

            SQLiteConnection connection = _createSQLiteConnection();
            SQLiteStatement stmt = connection.PrepareStatement(_sqlTextDictionary[sqlTextKey]);
            poolStmt = new SQLitePooledStatement<TEnum>(sqlTextKey, connection, stmt);
            _rentedStatements.Add(poolStmt);
            return poolStmt;
        }
    }

    /// <summary>
    /// Returns an statement to the pool that was previously rented. Do not use the statement after returning it.
    /// </summary>
    /// <param name="rentedStatement">The rented statement.</param>
    public void Return(SQLitePooledStatement<TEnum> rentedStatement)
    {
        ArgumentNullException.ThrowIfNull(rentedStatement);

        lock (_lock)
        {
            rentedStatement.Statement.Reset();
            _ = _rentedStatements.Remove(rentedStatement);

            if (!_pool.TryGetValue(rentedStatement.SQLTextKey, out List<SQLitePooledStatement<TEnum>>? objList))
            {
                objList = [];
                _pool.Add(rentedStatement.SQLTextKey, objList);
            }

            objList.Add(rentedStatement);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (List<SQLitePooledStatement<TEnum>> objList in _pool.Values)
        {
            foreach (SQLitePooledStatement<TEnum> stmt in objList)
            {
                stmt.Dispose();
            }

            objList.Clear();
        }

        _pool.Clear();

        foreach (SQLitePooledStatement<TEnum>? stmt in _rentedStatements)
        {
            stmt.Dispose();
        }

        _rentedStatements.Clear();
    }
}
