// <copyright file="SQLiteConnection.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

namespace SQLiteLibrary;

/// <summary>
/// SQLite database connection.
/// </summary>
public sealed class SQLiteConnection : IDisposable
{
    private readonly SQLiteConnectionHandle _handle;
    private readonly List<SQLiteStatement> _statements = new();

    private SQLiteConnection(SQLiteConnectionHandle connectionHandle) => _handle = connectionHandle;

    private SQLiteConnection() => throw new NotSupportedException();

    /// <summary>
    /// Creates a SQLite connection to a temporary in-memory database.
    /// </summary>
    /// <returns>The opened SQLite connection.</returns>
    /// <exception cref="SQLiteException">Thrown when the native SQLite library returns an error.</exception>
    public static SQLiteConnection CreateTemporaryInMemoryDb()
        => Create(":memory:", NativeMethods.SQLITE_OPEN_READWRITE | NativeMethods.SQLITE_OPEN_CREATE);

    /// <summary>
    /// Creates a read-only SQLite connection to a new or existing database.
    /// </summary>
    /// <param name="fileName">Filename of the database.</param>
    /// <returns>The opened SQLite connection.</returns>
    /// <exception cref="SQLiteException">Thrown when the native SQLite library returns an error.</exception>
    public static SQLiteConnection OpenExistingDbReadOnly(string fileName)
        => Create(fileName, NativeMethods.SQLITE_OPEN_READONLY);

    /// <summary>
    /// Creates a SQLite connection to a existing database.
    /// </summary>
    /// <param name="fileName">Filename of the database.</param>
    /// <returns>The opened SQLite connection.</returns>
    /// <exception cref="SQLiteException">Thrown when the native SQLite library returns an error.</exception>
    public static SQLiteConnection OpenExistingDb(string fileName)
        => Create(fileName, NativeMethods.SQLITE_OPEN_READWRITE);

    /// <summary>
    /// Creates a SQLite connection to a new or existing database.
    /// </summary>
    /// <param name="fileName">Filename of the database.</param>
    /// <returns>The opened SQLite connection.</returns>
    /// <exception cref="SQLiteException">Thrown when the native SQLite library returns an error.</exception>
    public static SQLiteConnection CreateNewOrOpenExistingDb(string fileName)
        => Create(fileName, NativeMethods.SQLITE_OPEN_READWRITE | NativeMethods.SQLITE_OPEN_CREATE);

    /// <summary>
    /// Set the busy time out of the connection.
    /// </summary>
    /// <param name="timeOutInMilliSeconds">the busy time out.</param>
    public void SetBusyTimeout(int timeOutInMilliSeconds)
    {
        int res = NativeMethods.sqlite3_busy_timeout(_handle, timeOutInMilliSeconds);
        NativeMethods.CheckResult(res, "sqlite3_busy_timeout", _handle);
    }

    /// <summary>
    /// Prepare a SQLite statement.
    /// </summary>
    /// <param name="sqlStatement">The SQL statement.</param>
    /// <returns>The prepared SQL statement and part of the statement after the first SQL command.</returns>
    /// <exception cref="SQLiteException">Thrown when the native SQLite library returns an error.</exception>
    [Obsolete("Use UTF8 string method instead.", DiagnosticId = "DNSQLL001")]
    public SQLiteStatement PrepareStatement(string sqlStatement)
        => sqlStatement is null ? throw new ArgumentNullException(nameof(sqlStatement)) : DoPrepareStatement(sqlStatement);

    /// <summary>
    /// Prepare a SQLite statement.
    /// </summary>
    /// <param name="sqlStatement">The SQL statement.</param>
    /// <returns>The prepared SQL statement and part of the statement after the first SQL command.</returns>
    /// <exception cref="SQLiteException">Thrown when the native SQLite library returns an error.</exception>
    public SQLiteStatement PrepareStatement(ReadOnlySpan<byte> sqlStatement)
        => DoPrepareStatement(sqlStatement);

    /// <summary>
    /// Prepare and execute a non query SQL statement.
    /// </summary>
    /// <param name="sqlStatement">The SQL statement.</param>
    [Obsolete("Use UTF8 string method instead.", DiagnosticId = "DNSQLL001")]
    public void ExecuteNonQuery(string sqlStatement)
    {
        if (sqlStatement is null)
        {
            throw new ArgumentNullException(nameof(sqlStatement));
        }

        using SQLiteStatement stmt = DoPrepareStatement(sqlStatement);
        stmt.DoneStep();
    }

    /// <summary>
    /// Prepare and execute a non query SQL statement.
    /// </summary>
    /// <param name="sqlStatement">The SQL statement.</param>
    public void ExecuteNonQuery(ReadOnlySpan<byte> sqlStatement)
    {
        using SQLiteStatement stmt = DoPrepareStatement(sqlStatement);
        stmt.DoneStep();
    }

    /// <summary>
    /// Prepare and execute a scalar query SQL statement and return the contents of the first column.
    /// </summary>
    /// <param name="sqlStatement">The SQL statement.</param>
    /// <returns>The contents of the first result column.</returns>
    [Obsolete("Use UTF8 string method instead.", DiagnosticId = "DNSQLL001")]
    public string ExecuteScalarStringQuery(string sqlStatement)
    {
        if (sqlStatement is null)
        {
            throw new ArgumentNullException(nameof(sqlStatement));
        }

        string value;
        using (SQLiteStatement stmt = DoPrepareStatement(sqlStatement))
        {
            stmt.NewRowStep();
            value = stmt.GetColumnStringValue(0);
            stmt.DoneStep();
        }

        return value;
    }

    /// <summary>
    /// Prepare and execute a scalar query SQL statement and return the contents of the first column.
    /// </summary>
    /// <param name="sqlStatement">The SQL statement.</param>
    /// <returns>The contents of the first result column.</returns>
    public string ExecuteScalarStringQuery(ReadOnlySpan<byte> sqlStatement)
    {
        string value;
        using (SQLiteStatement stmt = DoPrepareStatement(sqlStatement))
        {
            stmt.NewRowStep();
            value = stmt.GetColumnStringValue(0);
            stmt.DoneStep();
        }

        return value;
    }

    /// <summary>
    /// Prepare a SQLite statement and execute step expecting new row.
    /// </summary>
    /// <param name="sqlStatement">The SQL statement.</param>
    /// <returns>The prepared SQL statement and part of the statement after the first SQL command.</returns>
    /// <exception cref="SQLiteException">Thrown when the native SQLite library returns an error.</exception>
    [Obsolete("Use UTF8 string method instead.", DiagnosticId = "DNSQLL001")]
    public SQLiteStatement PrepareStatementAndNewRowStep(string sqlStatement)
    {
        if (sqlStatement is null)
        {
            throw new ArgumentNullException(nameof(sqlStatement));
        }

        SQLiteStatement stmt = DoPrepareStatement(sqlStatement);
        stmt.NewRowStep();
        return stmt;
    }

    /// <summary>
    /// Prepare a SQLite statement and execute step expecting new row.
    /// </summary>
    /// <param name="sqlStatement">The SQL statement.</param>
    /// <returns>The prepared SQL statement and part of the statement after the first SQL command.</returns>
    /// <exception cref="SQLiteException">Thrown when the native SQLite library returns an error.</exception>
    public SQLiteStatement PrepareStatementAndNewRowStep(ReadOnlySpan<byte> sqlStatement)
    {
        SQLiteStatement stmt = DoPrepareStatement(sqlStatement);
        stmt.NewRowStep();
        return stmt;
    }

    /// <summary>
    /// Gets the ROWID of the last row insert from the database connection which invoked the function.
    /// </summary>
    /// <returns>The ROWID of the last row insert.</returns>
    public long GetLastInsertRowid() => NativeMethods.sqlite3_last_insert_rowid(_handle);

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (SQLiteStatement stmt in _statements)
        {
            stmt.Dispose();
        }

        _statements.Clear();
        _handle.Dispose();
    }

    private static unsafe SQLiteConnection Create(string fileName, int flags)
    {
        if (fileName is null)
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        byte* utf8Filename = NativeMethods.ToUtf8BytePtr(fileName);
        int result = NativeMethods.sqlite3_open_v2(utf8Filename, out SQLiteConnectionHandle connectionHandle, flags, IntPtr.Zero);
        NativeMethods.FreeUtf8BytePtr(utf8Filename);
        NativeMethods.CheckResult(result, "sqlite3_open", null);

        result = NativeMethods.sqlite3_extended_result_codes(connectionHandle, 1);
        NativeMethods.CheckResult(result, "sqlite3_extended_result_codes", connectionHandle);

        return new SQLiteConnection(connectionHandle);
    }

    [Obsolete("Use UTF8 string method instead.", DiagnosticId = "DNSQLL001")]
    private SQLiteStatement DoPrepareStatement(string sqlStatement)
    {
        var stmt = SQLiteStatement.Create(_handle, sqlStatement);
        _statements.Add(stmt);
        return stmt;
    }

    private SQLiteStatement DoPrepareStatement(ReadOnlySpan<byte> sqlStatement)
    {
        var stmt = SQLiteStatement.Create(_handle, sqlStatement);
        _statements.Add(stmt);
        return stmt;
    }
}
