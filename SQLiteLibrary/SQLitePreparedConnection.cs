// <copyright file="SQLitePreparedConnection.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

namespace SQLiteLibrary;

/// <summary>
/// Provides a sqlite connection with prepared SQL statements.
/// </summary>
/// <typeparam name="TEnum">Enum type used as key to the SQL texts.</typeparam>
public sealed class SQLitePreparedConnection<TEnum> : IDisposable
    where TEnum : Enum
{
    private readonly Dictionary<TEnum, SQLiteStatement> _preparedStatements;
    private readonly SQLiteConnection _connection;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SQLitePreparedConnection{TEnum}"/> class.
    /// </summary>
    /// <param name="sqlTextDictionary">Dictionary with SQL texts.</param>
    /// <param name="createSQLiteConnection">Function used to create a new SQLiteConnection instance.</param>
    public SQLitePreparedConnection(IReadOnlyDictionary<TEnum, string> sqlTextDictionary, Func<SQLiteConnection> createSQLiteConnection)
    {
        ArgumentNullException.ThrowIfNull(sqlTextDictionary);
        ArgumentNullException.ThrowIfNull(createSQLiteConnection);

        _connection = createSQLiteConnection();
        var preparedStatements = new Dictionary<TEnum, SQLiteStatement>();
        foreach (KeyValuePair<TEnum, string> kvp in sqlTextDictionary)
        {
#pragma warning disable DNSQLL001 // Type or member is obsolete
            SQLiteStatement stmt = _connection.PrepareStatement(kvp.Value);
#pragma warning restore DNSQLL001 // Type or member is obsolete
            preparedStatements.Add(kvp.Key, stmt);
        }

        _preparedStatements = preparedStatements;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SQLitePreparedConnection{TEnum}"/> class.
    /// </summary>
    /// <param name="sqlTextDictionary">Dictionary with SQL texts.</param>
    /// <param name="createSQLiteConnection">Function used to create a new SQLiteConnection instance.</param>
    public SQLitePreparedConnection(IReadOnlyDictionary<TEnum, byte[]> sqlTextDictionary, Func<SQLiteConnection> createSQLiteConnection)
    {
        ArgumentNullException.ThrowIfNull(sqlTextDictionary);
        ArgumentNullException.ThrowIfNull(createSQLiteConnection);

        _connection = createSQLiteConnection();
        var preparedStatements = new Dictionary<TEnum, SQLiteStatement>();
        foreach (KeyValuePair<TEnum, byte[]> kvp in sqlTextDictionary)
        {
            SQLiteStatement stmt = _connection.PrepareStatement(kvp.Value);
            preparedStatements.Add(kvp.Key, stmt);
        }

        _preparedStatements = preparedStatements;
    }

    /// <summary>
    /// Get the prepared statement.
    /// </summary>
    /// <param name="sqlTextKey">Key to the SQL text.</param>
    /// <returns>The prepared staement.</returns>
    public SQLiteStatement GetStatement(TEnum sqlTextKey) => _preparedStatements[sqlTextKey];

    /// <summary>
    /// Execute a prepared non query SQL statement.
    /// </summary>
    /// <param name="sqlTextKey">Key to the SQL text.</param>
    public void ExecuteNonQuery(TEnum sqlTextKey) => _preparedStatements[sqlTextKey].DoneStep();

    /// <summary>
    /// Return the prepared statement.
    /// </summary>
    /// <param name="preparedStatement">A prepared statement.</param>
    public void Return(SQLiteStatement preparedStatement) => preparedStatement?.Reset();

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (SQLiteStatement stmt in _preparedStatements.Values)
        {
            stmt.Dispose();
        }

        _preparedStatements.Clear();
        _connection.Dispose();
        _disposed = true;
    }
}
