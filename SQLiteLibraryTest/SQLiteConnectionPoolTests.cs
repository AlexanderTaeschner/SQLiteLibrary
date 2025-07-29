// <copyright file="SQLiteConnectionPoolTests.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

using SQLiteLibrary;
using Xunit;

namespace SQLiteLibraryTest;

public class SQLiteConnectionPoolTests
{
    private const string FileName = "SQLiteConnectionPoolTests_TestDb.sqlite";
    private int _openDatabaseCalls;

    private enum SQLiteConnectionPoolTestStatements
    {
        InsertValue,
        SelectValue,
    }

    [Fact]
    public void SQLiteConnectionPool_Works()
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

        IReadOnlyDictionary<SQLiteConnectionPoolTestStatements, string> sqlTextDictionary = new Dictionary<SQLiteConnectionPoolTestStatements, string>()
        {
            { SQLiteConnectionPoolTestStatements.InsertValue, "INSERT INTO t VALUES (@value);" },
            { SQLiteConnectionPoolTestStatements.SelectValue, "SELECT x FROM t;" },
        };

        using var connectionPool = new SQLiteConnectionPool<SQLiteConnectionPoolTestStatements>(sqlTextDictionary, CreateSQLiteConnection);

        _openDatabaseCalls = 0;
        for (int i = 1; i <= 42; i++)
        {
            SQLitePooledStatement<SQLiteConnectionPoolTestStatements> pooledInsertStmt = connectionPool.Rent(SQLiteConnectionPoolTestStatements.InsertValue);

            SQLiteStatement insertStmt = pooledInsertStmt.Statement;
            insertStmt.BindParameter("@value\0"u8, i);

            connectionPool.Return(pooledInsertStmt);
        }

        for (int i = 1; i <= 42; i++)
        {
            SQLitePooledStatement<SQLiteConnectionPoolTestStatements> pooledSelectStmt = connectionPool.Rent(SQLiteConnectionPoolTestStatements.SelectValue);

            SQLiteStatement selectStmt = pooledSelectStmt.Statement;
            while (selectStmt.TryNewRowStep())
            {
                int value = selectStmt.GetColumnIntegerValue(0);
                Assert.Equal(i, value);
            }

            connectionPool.Return(pooledSelectStmt);
        }

        Assert.Equal(2, _openDatabaseCalls);
    }

    private SQLiteConnection CreateSQLiteConnection()
    {
        _openDatabaseCalls++;
        return SQLiteConnection.OpenExistingDb(FileName);
    }
}
