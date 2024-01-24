// <copyright file="SQLitePreparedConnectionTests.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

using SQLiteLibrary;
using Xunit;

namespace SQLiteLibraryTest;

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
            conn.ExecuteNonQuery("CREATE TABLE t(x INTEGER);"u8);
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
            conn.ExecuteNonQuery("CREATE TABLE t(x INTEGER);"u8);
        }

        Assert.True(File.Exists(FileName));

        Dictionary<SQLitePreparedConnectionTestStatements, byte[]> sqlTextDictionary = new()
        {
            { SQLitePreparedConnectionTestStatements.InsertValue, "INSERT INTO t VALUES (@value);"u8.ToArray() },
            { SQLitePreparedConnectionTestStatements.InsertValue43, "INSERT INTO t VALUES (43);"u8.ToArray() },
            { SQLitePreparedConnectionTestStatements.SelectValue, "SELECT x FROM t;"u8.ToArray() },
        };

        _openDatabaseCalls = 0;
        using var preparedConnection = new SQLitePreparedConnection<SQLitePreparedConnectionTestStatements>(sqlTextDictionary, CreateSQLiteConnection);
        TestPreparedConnection(preparedConnection);
    }

    private void TestPreparedConnection(SQLitePreparedConnection<SQLitePreparedConnectionTestStatements> preparedConnection)
    {
        int i;
        for (i = 1; i <= 42; i++)
        {
            SQLiteStatement insertStmt = preparedConnection.GetStatement(SQLitePreparedConnectionTestStatements.InsertValue);
            insertStmt.BindParameter("@value"u8, i);
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
