// <copyright file="SQLiteConnectionPoolTests.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

using SQLiteLibrary;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace SQLiteLibraryTest;

[ExcludeFromCodeCoverage]
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
    public void SQLiteConnectionPool_Return_Null_Throws()
    {
        IReadOnlyDictionary<SQLiteConnectionPoolTestStatements, string> sqlTextDictionary = new Dictionary<SQLiteConnectionPoolTestStatements, string>()
        {
            { SQLiteConnectionPoolTestStatements.SelectValue, "SELECT 1;" },
        };

        using var connectionPool = new SQLiteConnectionPool<SQLiteConnectionPoolTestStatements>(sqlTextDictionary, SQLiteConnection.CreateTemporaryInMemoryDb);

        _ = Assert.Throws<ArgumentNullException>(() => connectionPool.Return(null!));
    }

    [Fact]
    public void SQLiteConnectionPool_Rent_Unknown_Key_Throws()
    {
        IReadOnlyDictionary<SQLiteConnectionPoolTestStatements, string> sqlTextDictionary = new Dictionary<SQLiteConnectionPoolTestStatements, string>()
        {
            { SQLiteConnectionPoolTestStatements.SelectValue, "SELECT 1;" },
        };

        using var connectionPool = new SQLiteConnectionPool<SQLiteConnectionPoolTestStatements>(sqlTextDictionary, SQLiteConnection.CreateTemporaryInMemoryDb);

        _ = Assert.Throws<KeyNotFoundException>(() => connectionPool.Rent((SQLiteConnectionPoolTestStatements)int.MaxValue));
    }

    [Fact]
    public void SQLiteConnectionPool_Return_Adds_Unknown_Key_To_Pool_And_Resets_Statement()
    {
        IReadOnlyDictionary<SQLiteConnectionPoolTestStatements, string> sqlTextDictionary = new Dictionary<SQLiteConnectionPoolTestStatements, string>()
        {
            { SQLiteConnectionPoolTestStatements.SelectValue, "SELECT 1;" },
        };

        using var connectionPool = new SQLiteConnectionPool<SQLiteConnectionPoolTestStatements>(
            sqlTextDictionary,
            () => throw new InvalidOperationException("A new connection should not be created when a returned statement is available."));

        SQLiteConnection connection = SQLiteConnection.CreateTemporaryInMemoryDb();
        SQLiteStatement statement = connection.PrepareStatement("SELECT 1;\0"u8);

        SQLiteStepResult firstStepResult = statement.Step();
        Assert.Equal(SQLiteStepResult.NewRow, firstStepResult);

        var pooledStatement = new SQLitePooledStatement<SQLiteConnectionPoolTestStatements>(SQLiteConnectionPoolTestStatements.InsertValue, connection, statement);
        connectionPool.Return(pooledStatement);

        SQLitePooledStatement<SQLiteConnectionPoolTestStatements> rentedStatement = connectionPool.Rent(SQLiteConnectionPoolTestStatements.InsertValue);
        Assert.Same(statement, rentedStatement.Statement);

        SQLiteStepResult stepResultAfterReset = rentedStatement.Statement.Step();
        Assert.Equal(SQLiteStepResult.NewRow, stepResultAfterReset);
        Assert.Equal(SQLiteStepResult.Done, rentedStatement.Statement.Step());

        connectionPool.Return(rentedStatement);
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
