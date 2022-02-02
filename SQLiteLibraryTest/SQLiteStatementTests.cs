// <copyright file="SQLiteStatementTests.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

using SQLiteLibrary;
using Xunit;

namespace SQLiteLibraryTest;
public class SQLiteStatementTests
{
    [Fact]
    public void Step_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT 1;");
        SQLiteStepResult res = stmt.Step();
        Assert.Equal(SQLiteStepResult.NewRow, res);

        res = stmt.Step();
        Assert.Equal(SQLiteStepResult.Done, res);
    }

    [Fact]
    public void Step_After_PrepareStatementAndNewRowStep_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT 1;");

        SQLiteStepResult res = stmt.Step();
        Assert.Equal(SQLiteStepResult.Done, res);
    }

    [Theory]
    [InlineData("1", SQLiteDataType.Integer)]
    [InlineData("1.0", SQLiteDataType.Float)]
    [InlineData("'1'", SQLiteDataType.Text)]
    [InlineData("X'FF'", SQLiteDataType.Blob)]
    [InlineData("NULL", SQLiteDataType.Null)]
    public void GetColumnDataType_Works(string value, SQLiteDataType expectedType)
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT " + value);

        SQLiteDataType type = stmt.GetColumnDataType(0);
        Assert.Equal(expectedType, type);

        stmt.DoneStep();
    }

    [Fact]
    public void GetColumnDataType_On_Invalid_Column_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT 42;");

        SQLiteDataType type = stmt.GetColumnDataType(1);
        Assert.Equal(SQLiteDataType.Null, type);

        stmt.DoneStep();
    }

    [Fact]
    public void GetColumnDoubleValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT 4.2;");

        double value = stmt.GetColumnDoubleValue(0);
        Assert.Equal(4.2, value);

        stmt.DoneStep();
    }

    [Fact]
    public void GetColumnIntegerValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT 42;");

        int value = stmt.GetColumnIntegerValue(0);
        Assert.Equal(42, value);

        stmt.DoneStep();
    }

    [Fact]
    public void GetColumnLongValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT 4242424242;");

        long value = stmt.GetColumnLongValue(0);
        Assert.Equal(4242424242L, value);

        stmt.DoneStep();
    }

    [Fact]
    public void GetColumnStringValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT 'Adams_42';");

        string value = stmt.GetColumnStringValue(0);
        Assert.Equal("Adams_42", value);

        stmt.DoneStep();
    }

    [Fact]
    public void GetColumnBlobValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT X'AF42FA';");

        byte[] value = stmt.GetColumnBlobValue(0);
        Assert.Equal(3, value.Length);
        Assert.Equal(0xAF, value[0]);
        Assert.Equal(0x42, value[1]);
        Assert.Equal(0xFA, value[2]);

        stmt.DoneStep();
    }

    [Fact]
    public void GetColumnNullableDoubleValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT NULL;");

        double? value = stmt.GetColumnNullableDoubleValue(0);
        Assert.Null(value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindDoubleValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;");

        stmt.BindParameter(1, 4.2);

        stmt.NewRowStep();

        double value = stmt.GetColumnDoubleValue(0);
        Assert.Equal(4.2, value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindDoubleValue_NamedParameter_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;");

        Assert.Throws<ArgumentNullException>(() => stmt.BindParameter(null!, 4.2));
        stmt.BindParameter("@value", 4.2);

        stmt.NewRowStep();

        double value = stmt.GetColumnDoubleValue(0);
        Assert.Equal(4.2, value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindIntegerValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;");

        stmt.BindParameter(1, 42);

        stmt.NewRowStep();

        int value = stmt.GetColumnIntegerValue(0);
        Assert.Equal(42, value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindIntegerValue_NamedParameter_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;");

        Assert.Throws<ArgumentNullException>(() => stmt.BindParameter(null!, 42));
        stmt.BindParameter("@value", 42);

        stmt.NewRowStep();

        int value = stmt.GetColumnIntegerValue(0);
        Assert.Equal(42, value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindLongValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;");

        stmt.BindParameter(1, 4242424242L);

        stmt.NewRowStep();

        long value = stmt.GetColumnLongValue(0);
        Assert.Equal(4242424242L, value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindLongValue_NamedParameter_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;");

        Assert.Throws<ArgumentNullException>(() => stmt.BindParameter(null!, 4242424242L));
        stmt.BindParameter("@value", 4242424242L);

        stmt.NewRowStep();

        long value = stmt.GetColumnLongValue(0);
        Assert.Equal(4242424242L, value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindStringValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;");

        Assert.Throws<ArgumentNullException>(() => stmt.BindParameter(1, (string)null!));
        stmt.BindParameter(1, "Adams_42");

        stmt.NewRowStep();

        string value = stmt.GetColumnStringValue(0);
        Assert.Equal("Adams_42", value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindStringValue_NamedParameter_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;");

        Assert.Throws<ArgumentNullException>(() => stmt.BindParameter(null!, "Adams_42"));
        stmt.BindParameter("@value", "Adams_42");

        stmt.NewRowStep();

        string value = stmt.GetColumnStringValue(0);
        Assert.Equal("Adams_42", value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindStringValue_NamedParameter_Comparison_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT 1 WHERE 'Adams_42' = @value;");

        stmt.BindParameter("@value", "Adams_42");

        stmt.NewRowStep();

        int value = stmt.GetColumnIntegerValue(0);
        Assert.Equal(1, value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindBlobValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;");

        stmt.BindParameter(1, new byte[] { 0xAF, 0x42, 0xFA });

        stmt.NewRowStep();

        byte[] value = stmt.GetColumnBlobValue(0);
        Assert.Equal(3, value.Length);
        Assert.Equal(0xAF, value[0]);
        Assert.Equal(0x42, value[1]);
        Assert.Equal(0xFA, value[2]);

        stmt.DoneStep();
    }

    [Fact]
    public void BindBlobValue_NamedParameter_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;");

        stmt.BindParameter("@value", new byte[] { 0xAF, 0x42, 0xFA });

        stmt.NewRowStep();

        byte[] value = stmt.GetColumnBlobValue(0);
        Assert.Equal(3, value.Length);
        Assert.Equal(0xAF, value[0]);
        Assert.Equal(0x42, value[1]);
        Assert.Equal(0xFA, value[2]);

        stmt.DoneStep();
    }

    [Fact]
    public void TryNewStep_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        conn.ExecuteNonQuery("CREATE TABLE t(x INTEGER);");
        conn.ExecuteNonQuery("INSERT INTO t VALUES (42);");
        long rowid = conn.GetLastInsertRowid();
        Assert.Equal(1, rowid);
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT 424 FROM t WHERE (x = @testX);");
        stmt.BindParameter("@testX", 0);

        bool res = stmt.TryNewRowStep();
        Assert.False(res);

        stmt.Reset();

        stmt.BindParameter("@testX", 42);

        res = stmt.TryNewRowStep();
        Assert.True(res);

        int value = stmt.GetColumnIntegerValue(0);
        Assert.Equal(424, value);
        stmt.DoneStep();
    }
}
