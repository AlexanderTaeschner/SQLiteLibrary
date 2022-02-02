// <copyright file="SQLitePooledStatement.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

namespace SQLiteLibrary;

/// <summary>
/// Prepared SQL statement which can be reused.
/// </summary>
/// <typeparam name="TEnum">Enum type used as key to the SQL texts.</typeparam>
public sealed class SQLitePooledStatement<TEnum> : IDisposable
    where TEnum : Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SQLitePooledStatement{TEnum}"/> class.
    /// </summary>
    /// <param name="sqlTextKey">Used key to the SQL text.</param>
    /// <param name="connection">SQLite connection.</param>
    /// <param name="statement">Prepared SQL statement.</param>
    public SQLitePooledStatement(
        TEnum sqlTextKey,
        SQLiteConnection connection,
        SQLiteStatement statement)
    {
        SQLTextKey = sqlTextKey;
        Connection = connection;
        Statement = statement;
    }

    private SQLitePooledStatement() => throw new NotSupportedException();

    /// <summary>
    /// Gets the prepared SQL statement.
    /// </summary>
    public SQLiteStatement Statement { get; }

    /// <summary>
    /// Gets the connection.
    /// </summary>
    public SQLiteConnection Connection { get; }

    internal TEnum SQLTextKey { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        Statement.Dispose();
        Connection.Dispose();
    }
}
