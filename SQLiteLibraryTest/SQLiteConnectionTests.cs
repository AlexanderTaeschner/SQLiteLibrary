// <copyright file="SQLiteConnectionTests.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

using SQLiteLibrary;
using Xunit;

namespace SQLiteLibraryTest;

public class SQLiteConnectionTests
{
    [Fact]
    public void Open_Memory_Database_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        Assert.NotNull(conn);
    }

    [Fact]
    public void SetBusyTimeout_Database_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        conn.SetBusyTimeout(60000);
    }

    [Fact]
    public void Prepare_Statement_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
#pragma warning disable DNSQLL001 // Type or member is obsolete
        Assert.Throws<ArgumentNullException>(() => conn.PrepareStatement((string)null!));
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT 1;");
#pragma warning restore DNSQLL001 // Type or member is obsolete
        Assert.NotNull(stmt);
    }

    [Fact]
    public void Prepare_Statement_U8_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT 1;\0"u8);
        Assert.NotNull(stmt);
    }

    [Fact]
    public void Prepare_Statement_Throws_On_SQL_Error_Works()
    {
        List<(int ErrCode, string Message)> errors = [];
        SQLiteConnection.LogErrorMessage += (errorCode,message) => errors.Add((errorCode, message));
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        SQLiteException exception = Assert.Throws<SQLiteException>(() => conn.PrepareStatement("SELECT * FROM t;\0"u8));
        Assert.Equal("SELECT * FROM t;", exception.SqlStatement);
        Assert.Equal(1, exception.ResultCode);
        Assert.Equal("SQL logic error", exception.ResultCodeName);
        Assert.Equal("sqlite3_prepare_v2", exception.NativeMethod);
        Assert.Equal("no such table: t", exception.NativeErrorMessage);
        Assert.Equal("SQLiteLibrary.SQLiteException: Native method sqlite3_prepare_v2 returned error code SQL logic error(1): 'no such table: t'!", exception.Message);
        (int errCode, string message) = Assert.Single(errors);
        Assert.Equal(1, errCode);
        Assert.Equal("no such table: t in \"SELECT * FROM t;\"", message);
    }

    [Fact]
    public void Prepare_Statement_Throws_On_Not_Null_Terminated_String()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        SQLiteException exception = Assert.Throws<SQLiteException>(() => conn.PrepareStatement("SELECT 1;"u8));
        Assert.Null(exception.SqlStatement);
        Assert.Equal(0, exception.ResultCode);
        Assert.Equal(string.Empty, exception.ResultCodeName);
        Assert.Equal(string.Empty, exception.NativeMethod);
        Assert.Equal(string.Empty, exception.NativeErrorMessage);
        Assert.Equal("The UTF8 text in parameter 'sqlStatement' must be null terminated!", exception.Message);
    }

    [Fact]
    public void Prepare_Statement_Throws_For_Multiple_Statements_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
#pragma warning disable DNSQLL001 // Type or member is obsolete
        SQLiteException exception = Assert.Throws<SQLiteException>(() => conn.PrepareStatement("SELECT 1; SELECT 2;"));
#pragma warning restore DNSQLL001 // Type or member is obsolete
        Assert.Equal("SQL statement contained more than a single SQL command. Additional text: ' SELECT 2;'", exception.Message);
    }

    [Fact]
    public void Prepare_Statement_Throws_For_Multiple_Statements_U8_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        SQLiteException exception = Assert.Throws<SQLiteException>(() => conn.PrepareStatement("SELECT 1; SELECT 2;\0"u8));
        Assert.Equal("SQL statement contained more than a single SQL command. Additional text: ' SELECT 2;'", exception.Message);
    }

    [Fact]
    public void ExecuteNonQuery_Works()
    {
#pragma warning disable DNSQLL001 // Type or member is obsolete
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        conn.ExecuteNonQuery("CREATE TABLE t(x INTEGER);");
        conn.ExecuteNonQuery("INSERT INTO t VALUES (42);");
        long rowid = conn.GetLastInsertRowid();
        Assert.Equal(1, rowid);
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT x FROM t;");
        int value = stmt.GetColumnIntegerValue(0);
        Assert.Equal(42, value);
        stmt.DoneStep();
#pragma warning restore DNSQLL001 // Type or member is obsolete
    }

    [Fact]
    public void ExecuteNonQuery_U8_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        conn.ExecuteNonQuery("CREATE TABLE t(x INTEGER);\0"u8);
        conn.ExecuteNonQuery("INSERT INTO t VALUES (42);\0"u8);
        long rowid = conn.GetLastInsertRowid();
        Assert.Equal(1, rowid);
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT x FROM t;\0"u8);
        int value = stmt.GetColumnIntegerValue(0);
        Assert.Equal(42, value);
        stmt.DoneStep();
    }

    [Fact]
    public void ExecuteScalarStringQuery_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
#pragma warning disable DNSQLL001 // Type or member is obsolete
        string value = conn.ExecuteScalarStringQuery("SELECT 'Adams_42';");
#pragma warning restore DNSQLL001 // Type or member is obsolete
        Assert.Equal("Adams_42", value);
    }

    [Fact]
    public void ExecuteScalarStringQuery_U8_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        string value = conn.ExecuteScalarStringQuery("SELECT 'Adams_42';\0"u8);
        Assert.Equal("Adams_42", value);
    }

    [Fact]
    public void Expected_Version_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        string value = conn.ExecuteScalarStringQuery("SELECT sqlite_version();\0"u8);
        Assert.Equal("3.51.2", value);
    }

    [Fact]
    public void Open_File_Works()
    {
        string fileName = "SQLiteConnectionTests_TestDb.sqlite";
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        using (var conn = SQLiteConnection.CreateNewOrOpenExistingDb(fileName))
        {
            conn.ExecuteNonQuery("CREATE TABLE t(x INTEGER);\0"u8);
            conn.ExecuteNonQuery("INSERT INTO t VALUES (42);\0"u8);
            long rowid = conn.GetLastInsertRowid();
            Assert.Equal(1, rowid);
        }

        Assert.True(File.Exists(fileName));

        using (var conn = SQLiteConnection.OpenExistingDbReadOnly(fileName))
        {
            using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT x FROM t;\0"u8);
            int value = stmt.GetColumnIntegerValue(0);
            Assert.Equal(42, value);
            stmt.DoneStep();
        }
    }
}
