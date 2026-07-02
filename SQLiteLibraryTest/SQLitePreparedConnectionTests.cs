// <copyright file="SQLitePreparedConnectionTests.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

using SQLiteLibrary;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace SQLiteLibraryTest;

[ExcludeFromCodeCoverage]
public class SQLitePreparedConnectionTests
{
    private const string FileName = "SQLitePreparedConnectionTests_TestDb.sqlite";
    private int _openDatabaseCalls;

    private enum SQLitePreparedConnectionTestStatements
    {
        InsertValue,
        InsertValue43,
        SelectValue,
    }

    [Fact]
    public void SQLitePreparedConnection_Works()
    {
        if (File.Exists(FileName))
        {
            File.Delete(FileName);
        }

        using (var conn = SQLiteConnection.CreateNewOrOpenExistingDb(FileName))
        {
            conn.ExecuteNonQuery("CREATE TABLE t(x INTEGER);\0"u8);
        }

        Assert.True(File.Exists(FileName));

        Dictionary<SQLitePreparedConnectionTestStatements, string> sqlTextDictionary = new()
        {
            { SQLitePreparedConnectionTestStatements.InsertValue, "INSERT INTO t VALUES (@value);" },
            { SQLitePreparedConnectionTestStatements.InsertValue43, "INSERT INTO t VALUES (43);" },
            { SQLitePreparedConnectionTestStatements.SelectValue, "SELECT x FROM t;" },
        };

        _openDatabaseCalls = 0;
        using var preparedConnection = new SQLitePreparedConnection<SQLitePreparedConnectionTestStatements>(sqlTextDictionary, CreateSQLiteConnection);
        TestPreparedConnection(preparedConnection);
    }

    [Fact]
    public void SQLitePreparedConnection_Works2()
    {
        if (File.Exists(FileName))
        {
            File.Delete(FileName);
        }

        using (var conn = SQLiteConnection.CreateNewOrOpenExistingDb(FileName))
        {
            conn.ExecuteNonQuery("CREATE TABLE t(x INTEGER);\0"u8);
        }

        Assert.True(File.Exists(FileName));

        Dictionary<SQLitePreparedConnectionTestStatements, byte[]> sqlTextDictionary = new()
        {
            { SQLitePreparedConnectionTestStatements.InsertValue, "INSERT INTO t VALUES (@value);\0"u8.ToArray() },
            { SQLitePreparedConnectionTestStatements.InsertValue43, "INSERT INTO t VALUES (43);\0"u8.ToArray() },
            { SQLitePreparedConnectionTestStatements.SelectValue, "SELECT x FROM t;\0"u8.ToArray() },
        };

        _openDatabaseCalls = 0;
        using var preparedConnection = new SQLitePreparedConnection<SQLitePreparedConnectionTestStatements>(sqlTextDictionary, CreateSQLiteConnection);
        TestPreparedConnection(preparedConnection);
    }

    [Fact]
    public void SQLitePreparedConnection_Ctor_Throws_On_Null_Arguments()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new SQLitePreparedConnection<SQLitePreparedConnectionTestStatements>((IReadOnlyDictionary<SQLitePreparedConnectionTestStatements, string>)null!, CreateSQLiteConnection));
        _ = Assert.Throws<ArgumentNullException>(() => new SQLitePreparedConnection<SQLitePreparedConnectionTestStatements>((IReadOnlyDictionary<SQLitePreparedConnectionTestStatements, byte[]>)null!, CreateSQLiteConnection));

        Dictionary<SQLitePreparedConnectionTestStatements, string> stringDictionary = new()
        {
            { SQLitePreparedConnectionTestStatements.SelectValue, "SELECT 1;" },
        };

        Dictionary<SQLitePreparedConnectionTestStatements, byte[]> byteDictionary = new()
        {
            { SQLitePreparedConnectionTestStatements.SelectValue, "SELECT 1;\0"u8.ToArray() },
        };

        _ = Assert.Throws<ArgumentNullException>(() => new SQLitePreparedConnection<SQLitePreparedConnectionTestStatements>(stringDictionary, null!));
        _ = Assert.Throws<ArgumentNullException>(() => new SQLitePreparedConnection<SQLitePreparedConnectionTestStatements>(byteDictionary, null!));
    }

    [Fact]
    public void SQLitePreparedConnection_Return_Null_Works()
    {
        Dictionary<SQLitePreparedConnectionTestStatements, byte[]> sqlTextDictionary = new()
        {
            { SQLitePreparedConnectionTestStatements.SelectValue, "SELECT 1;\0"u8.ToArray() },
        };

        using var preparedConnection = new SQLitePreparedConnection<SQLitePreparedConnectionTestStatements>(sqlTextDictionary, SQLiteConnection.CreateTemporaryInMemoryDb);

        preparedConnection.Return(null!);
    }

    [Fact]
    public void SQLitePreparedConnection_Dispose_Twice_Works()
    {
        Dictionary<SQLitePreparedConnectionTestStatements, byte[]> sqlTextDictionary = new()
        {
            { SQLitePreparedConnectionTestStatements.SelectValue, "SELECT 1;\0"u8.ToArray() },
        };

        var preparedConnection = new SQLitePreparedConnection<SQLitePreparedConnectionTestStatements>(sqlTextDictionary, SQLiteConnection.CreateTemporaryInMemoryDb);
        preparedConnection.Dispose();
        preparedConnection.Dispose();
    }

    private void TestPreparedConnection(SQLitePreparedConnection<SQLitePreparedConnectionTestStatements> preparedConnection)
    {
        int i;
        for (i = 1; i <= 42; i++)
        {
            SQLiteStatement insertStmt = preparedConnection.GetStatement(SQLitePreparedConnectionTestStatements.InsertValue);
            insertStmt.BindParameter("@value\0"u8, i);
            insertStmt.DoneStep();
            preparedConnection.Return(insertStmt);
        }

        preparedConnection.ExecuteNonQuery(SQLitePreparedConnectionTestStatements.InsertValue43);

        i = 1;
        SQLiteStatement selectStmt = preparedConnection.GetStatement(SQLitePreparedConnectionTestStatements.SelectValue);
        while (selectStmt.TryNewRowStep())
        {
            int value = selectStmt.GetColumnIntegerValue(0);
            Assert.Equal(i++, value);
        }

        Assert.Equal(44, i);
        preparedConnection.Return(selectStmt);

        Assert.Equal(1, _openDatabaseCalls);
    }

    private SQLiteConnection CreateSQLiteConnection()
    {
        _openDatabaseCalls++;
        return SQLiteConnection.OpenExistingDb(FileName);
    }
}
