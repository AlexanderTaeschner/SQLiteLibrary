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
#pragma warning disable DNSQLL001 // Type or member is obsolete
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT 1;");
#pragma warning restore DNSQLL001 // Type or member is obsolete
        SQLiteStepResult res = stmt.Step();
        Assert.Equal(SQLiteStepResult.NewRow, res);

        res = stmt.Step();
        Assert.Equal(SQLiteStepResult.Done, res);
    }

    [Fact]
    public void Step_U8_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT 1;"u8);
        SQLiteStepResult res = stmt.Step();
        Assert.Equal(SQLiteStepResult.NewRow, res);

        res = stmt.Step();
        Assert.Equal(SQLiteStepResult.Done, res);
    }

    [Fact]
    public void Step_After_PrepareStatementAndNewRowStep_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
#pragma warning disable DNSQLL001 // Type or member is obsolete
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT 1;");
#pragma warning restore DNSQLL001 // Type or member is obsolete

        SQLiteStepResult res = stmt.Step();
        Assert.Equal(SQLiteStepResult.Done, res);
    }

    [Fact]
    public void Step_After_PrepareStatementAndNewRowStep_U8_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT 1;"u8);

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
#pragma warning disable DNSQLL001 // Type or member is obsolete
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT " + value);
#pragma warning restore DNSQLL001 // Type or member is obsolete

        SQLiteDataType type = stmt.GetColumnDataType(0);
        Assert.Equal(expectedType, type);

        stmt.DoneStep();
    }

    [Fact]
    public void GetColumnDataType_On_Invalid_Column_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT 42;"u8);

        SQLiteDataType type = stmt.GetColumnDataType(1);
        Assert.Equal(SQLiteDataType.Null, type);

        stmt.DoneStep();
    }

    [Fact]
    public void GetColumnDoubleValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT 4.2;"u8);

        double value = stmt.GetColumnDoubleValue(0);
        Assert.Equal(4.2, value);
        double? value2 = stmt.GetColumnNullableDoubleValue(0);
        Assert.Equal(4.2, value2.GetValueOrDefault());

        stmt.DoneStep();
    }

    [Fact]
    public void GetColumnIntegerValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT 42;"u8);

        int value = stmt.GetColumnIntegerValue(0);
        Assert.Equal(42, value);
        int? value2 = stmt.GetColumnNullableIntegerValue(0);
        Assert.Equal(42, value2.GetValueOrDefault());

        stmt.DoneStep();
    }

    [Fact]
    public void GetColumnLongValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT 4242424242;"u8);

        long value = stmt.GetColumnLongValue(0);
        Assert.Equal(4242424242L, value);
        long? value2 = stmt.GetColumnNullableLongValue(0);
        Assert.Equal(4242424242L, value2.GetValueOrDefault());

        stmt.DoneStep();
    }

    [Fact]
    public void GetColumnStringValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT 'Adams_42';"u8);

        string value = stmt.GetColumnStringValue(0);
        Assert.Equal("Adams_42", value);
        string? value2 = stmt.GetColumnNullableStringValue(0);
        Assert.Equal("Adams_42", value2);

        stmt.DoneStep();
    }

    [Fact]
    public void GetColumnBlobValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatementAndNewRowStep("SELECT X'AF42FA';"u8);

        byte[] value = stmt.GetColumnBlobValue(0);
        Assert.Equal(3, value.Length);
        Assert.Equal(0xAF, value[0]);
        Assert.Equal(0x42, value[1]);
        Assert.Equal(0xFA, value[2]);
        byte[]? value2 = stmt.GetColumnNullableBlobValue(0);

        Assert.NotNull(value2);
        Assert.Equal(3, value2.Length);
        Assert.Equal(0xAF, value2[0]);
        Assert.Equal(0x42, value2[1]);
        Assert.Equal(0xFA, value2[2]);

        stmt.DoneStep();
    }

    [Fact]
    public void BindDoubleValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;"u8);

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
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;"u8);

#pragma warning disable DNSQLL001 // Type or member is obsolete
        Assert.Throws<ArgumentNullException>(() => stmt.BindParameter((string)null!, 4.2));
        stmt.BindParameter("@value", 4.2);
#pragma warning restore DNSQLL001 // Type or member is obsolete

        stmt.NewRowStep();

        double value = stmt.GetColumnDoubleValue(0);
        Assert.Equal(4.2, value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindDoubleValue_NamedParameter_U8_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;"u8);
        stmt.BindParameter("@value"u8, 4.2);

        stmt.NewRowStep();

        double value = stmt.GetColumnDoubleValue(0);
        Assert.Equal(4.2, value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindIntegerValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;"u8);

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
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;"u8);

#pragma warning disable DNSQLL001 // Type or member is obsolete
        Assert.Throws<ArgumentNullException>(() => stmt.BindParameter((string)null!, 42));
        stmt.BindParameter("@value", 42);
#pragma warning restore DNSQLL001 // Type or member is obsolete

        stmt.NewRowStep();

        int value = stmt.GetColumnIntegerValue(0);
        Assert.Equal(42, value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindIntegerValue_NamedParameter_U8_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;"u8);

        stmt.BindParameter("@value"u8, 42);

        stmt.NewRowStep();

        int value = stmt.GetColumnIntegerValue(0);
        Assert.Equal(42, value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindLongValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;"u8);

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
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;"u8);

#pragma warning disable DNSQLL001 // Type or member is obsolete
        Assert.Throws<ArgumentNullException>(() => stmt.BindParameter((string)null!, 4242424242L));
        stmt.BindParameter("@value", 4242424242L);
#pragma warning restore DNSQLL001 // Type or member is obsolete

        stmt.NewRowStep();

        long value = stmt.GetColumnLongValue(0);
        Assert.Equal(4242424242L, value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindLongValue_NamedParameter_U8_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;"u8);

        stmt.BindParameter("@value"u8, 4242424242L);

        stmt.NewRowStep();

        long value = stmt.GetColumnLongValue(0);
        Assert.Equal(4242424242L, value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindStringValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;"u8);

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
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;"u8);

#pragma warning disable DNSQLL001 // Type or member is obsolete
        Assert.Throws<ArgumentNullException>(() => stmt.BindParameter((string)null!, "Adams_42"));
        stmt.BindParameter("@value", "Adams_42");
#pragma warning restore DNSQLL001 // Type or member is obsolete

        stmt.NewRowStep();

        string value = stmt.GetColumnStringValue(0);
        Assert.Equal("Adams_42", value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindStringValue_NamedParameter_U8_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;"u8);

        stmt.BindParameter("@value"u8, "Adams_42");

        stmt.NewRowStep();

        string value = stmt.GetColumnStringValue(0);
        Assert.Equal("Adams_42", value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindStringValue_NamedParameter_U8U8_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;"u8);

        stmt.BindParameter("@value"u8, "Adams_42"u8);

        stmt.NewRowStep();

        string value = stmt.GetColumnStringValue(0);
        Assert.Equal("Adams_42", value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindStringValue_NamedParameter_Comparison_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT 1 WHERE 'Adams_42' = @value;"u8);

#pragma warning disable DNSQLL001 // Type or member is obsolete
        stmt.BindParameter("@value", "Adams_42");
#pragma warning restore DNSQLL001 // Type or member is obsolete

        stmt.NewRowStep();

        int value = stmt.GetColumnIntegerValue(0);
        Assert.Equal(1, value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindStringValue_NamedParameter_Comparison_U8_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT 1 WHERE 'Adams_42' = @value;"u8);

        stmt.BindParameter("@value"u8, "Adams_42");

        stmt.NewRowStep();

        int value = stmt.GetColumnIntegerValue(0);
        Assert.Equal(1, value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindStringValue_NamedParameter_Comparison_U8U8_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT 1 WHERE 'Adams_42' = @value;"u8);

        stmt.BindParameter("@value"u8, "Adams_42"u8);

        stmt.NewRowStep();

        int value = stmt.GetColumnIntegerValue(0);
        Assert.Equal(1, value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindBlobValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;"u8);

        Assert.Throws<ArgumentNullException>(() => stmt.BindParameter(1, (byte[])null!));
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
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;"u8);

#pragma warning disable DNSQLL001 // Type or member is obsolete
        Assert.Throws<ArgumentNullException>(() => stmt.BindParameter((string)null!, new byte[] { 0xAF, 0x42, 0xFA }));
        stmt.BindParameter("@value", new byte[] { 0xAF, 0x42, 0xFA });
#pragma warning restore DNSQLL001 // Type or member is obsolete

        stmt.NewRowStep();

        byte[] value = stmt.GetColumnBlobValue(0);
        Assert.Equal(3, value.Length);
        Assert.Equal(0xAF, value[0]);
        Assert.Equal(0x42, value[1]);
        Assert.Equal(0xFA, value[2]);

        stmt.DoneStep();
    }

    [Fact]
    public void BindBlobValue_NamedParameter_U8_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;"u8);

        stmt.BindParameter("@value"u8, new byte[] { 0xAF, 0x42, 0xFA });

        stmt.NewRowStep();

        byte[] value = stmt.GetColumnBlobValue(0);
        Assert.Equal(3, value.Length);
        Assert.Equal(0xAF, value[0]);
        Assert.Equal(0x42, value[1]);
        Assert.Equal(0xFA, value[2]);

        stmt.DoneStep();
    }

    [Fact]
    public void BindZeroedBlobValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;"u8);

        stmt.BindZeroedBlobParameter(1, 3);

        stmt.NewRowStep();

        byte[] value = stmt.GetColumnBlobValue(0);
        Assert.Equal(3, value.Length);
        Assert.Equal(0, value[0]);
        Assert.Equal(0, value[1]);
        Assert.Equal(0, value[2]);

        stmt.DoneStep();
    }

    [Fact]
    public void BindZeroedBlob64Value_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;"u8);

        stmt.BindZeroedBlobParameter(1, 3L);

        stmt.NewRowStep();

        byte[] value = stmt.GetColumnBlobValue(0);
        Assert.Equal(3, value.Length);
        Assert.Equal(0, value[0]);
        Assert.Equal(0, value[1]);
        Assert.Equal(0, value[2]);

        stmt.DoneStep();
    }

    [Fact]
    public void BindDateTimeValue_ISO8601Text_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;"u8);

        Assert.Throws<ArgumentOutOfRangeException>(() => stmt.BindParameter(1, new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), (SQLiteDateTimeFormat)int.MaxValue));
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        stmt.BindParameter(1, dateTime, SQLiteDateTimeFormat.ISO8601Text);

        stmt.NewRowStep();

        string value = stmt.GetColumnStringValue(0);
        Assert.Equal("1970-01-01 00:00:00Z", value);

        DateTime dateTimeValue = stmt.GetColumnDateTimeValue(0, SQLiteDateTimeFormat.ISO8601Text);
        Assert.Equal(dateTime, dateTimeValue, new TimeSpan(0));

        stmt.DoneStep();
    }

    [Fact]
    public void BindDateTimeValue_ISO8601Text_LocalTime_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;"u8);

        Assert.Throws<ArgumentOutOfRangeException>(() => stmt.BindParameter(1, new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), (SQLiteDateTimeFormat)int.MaxValue));
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
        stmt.BindParameter(1, dateTime, SQLiteDateTimeFormat.ISO8601Text);

        stmt.NewRowStep();

        DateTime dateTimeValue = stmt.GetColumnDateTimeValue(0, SQLiteDateTimeFormat.ISO8601Text);
        Assert.Equal(dateTime, dateTimeValue, new TimeSpan(0));

        stmt.DoneStep();
    }

    [Fact]
    public void BindDateTimeValue_NamedParameter_ISO8601Text_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;"u8);

        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#pragma warning disable DNSQLL001 // Type or member is obsolete
        Assert.Throws<ArgumentNullException>(() => stmt.BindParameter((string)null!, dateTime, SQLiteDateTimeFormat.ISO8601Text));
        stmt.BindParameter("@value", dateTime, SQLiteDateTimeFormat.ISO8601Text);
#pragma warning restore DNSQLL001 // Type or member is obsolete

        stmt.NewRowStep();

        string value = stmt.GetColumnStringValue(0);
        Assert.Equal("1970-01-01 00:00:00Z", value);

        DateTime dateTimeValue = stmt.GetColumnDateTimeValue(0, SQLiteDateTimeFormat.ISO8601Text);
        Assert.Equal(dateTime, dateTimeValue, new TimeSpan(0));

        stmt.DoneStep();
    }

    [Fact]
    public void BindDateTimeValue_NamedParameter_ISO8601Text_U8_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;"u8);

        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        stmt.BindParameter("@value"u8, dateTime, SQLiteDateTimeFormat.ISO8601Text);

        stmt.NewRowStep();

        string value = stmt.GetColumnStringValue(0);
        Assert.Equal("1970-01-01 00:00:00Z", value);

        DateTime dateTimeValue = stmt.GetColumnDateTimeValue(0, SQLiteDateTimeFormat.ISO8601Text);
        Assert.Equal(dateTime, dateTimeValue, new TimeSpan(0));

        stmt.DoneStep();
    }

    [Fact]
    public void BindDateTimeValue_UnixTimeInteger_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;"u8);

        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        stmt.BindParameter(1, dateTime, SQLiteDateTimeFormat.UnixTimeInteger);

        stmt.NewRowStep();

        long value = stmt.GetColumnLongValue(0);
        Assert.Equal(0, value);

        DateTime dateTimeValue = stmt.GetColumnDateTimeValue(0, SQLiteDateTimeFormat.UnixTimeInteger);
        Assert.Equal(dateTime, dateTimeValue);

        stmt.DoneStep();
    }

    [Fact]
    public void BindDateTimeValue_JulianDateReal_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;"u8);

        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        stmt.BindParameter(1, dateTime, SQLiteDateTimeFormat.JulianDateReal);

        stmt.NewRowStep();

        double value = stmt.GetColumnDoubleValue(0);
        Assert.Equal(2440587.5, value);

        DateTime dateTimeValue = stmt.GetColumnDateTimeValue(0, SQLiteDateTimeFormat.JulianDateReal);
        Assert.Equal(dateTime, dateTimeValue);

        stmt.DoneStep();
    }

    [Fact]
    public void BindNullValue_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT ?1;"u8);

        stmt.BindNullToParameter(1);

        stmt.NewRowStep();

        string? stringValue = stmt.GetColumnNullableStringValue(0);
        Assert.Null(stringValue);
        byte[]? blobValue = stmt.GetColumnNullableBlobValue(0);
        Assert.Null(blobValue);
        double? doubleValue = stmt.GetColumnNullableDoubleValue(0);
        Assert.Null(doubleValue);
        int? integerValue = stmt.GetColumnNullableIntegerValue(0);
        Assert.Null(integerValue);
        long? longValue = stmt.GetColumnNullableLongValue(0);
        Assert.Null(longValue);

        stmt.DoneStep();
    }

    [Fact]
    public void BindNullValue_NamedParameter_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;"u8);

#pragma warning disable DNSQLL001 // Type or member is obsolete
        Assert.Throws<ArgumentNullException>(() => stmt.BindNullToParameter((string)null!));
        stmt.BindNullToParameter("@value");
#pragma warning restore DNSQLL001 // Type or member is obsolete

        stmt.NewRowStep();

        string? value = stmt.GetColumnNullableStringValue(0);
        Assert.Null(value);

        stmt.DoneStep();
    }

    [Fact]
    public void BindNullValue_NamedParameter_U8_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT @value;"u8);

        stmt.BindNullToParameter("@value"u8);

        stmt.NewRowStep();

        string? value = stmt.GetColumnNullableStringValue(0);
        Assert.Null(value);

        stmt.DoneStep();
    }

    [Fact]
    public void TryNewStep_Works()
    {
        using var conn = SQLiteConnection.CreateTemporaryInMemoryDb();
        conn.ExecuteNonQuery("CREATE TABLE t(x INTEGER);"u8);
        conn.ExecuteNonQuery("INSERT INTO t VALUES (42);"u8);
        long rowid = conn.GetLastInsertRowid();
        Assert.Equal(1, rowid);
        using SQLiteStatement stmt = conn.PrepareStatement("SELECT 424 FROM t WHERE (x = @testX);"u8);
        stmt.BindParameter("@testX"u8, 0);

        bool res = stmt.TryNewRowStep();
        Assert.False(res);

        stmt.Reset();

        stmt.BindParameter("@testX"u8, 42);

        res = stmt.TryNewRowStep();
        Assert.True(res);

        int value = stmt.GetColumnIntegerValue(0);
        Assert.Equal(424, value);
        stmt.DoneStep();
    }
}
