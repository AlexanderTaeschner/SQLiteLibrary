// <copyright file="SQLiteStatement.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SQLiteLibrary;

/// <summary>
/// Prepared SQL statement.
/// </summary>
public sealed partial class SQLiteStatement : IDisposable
{
    private const string ISO8601TextFormat = "yyyy-MM-dd HH:mm:ss.FFFFFFFK";

    private readonly SQLiteStatementHandle _handle;
    private readonly SQLiteConnectionHandle _connectionHandle;

    private SQLiteStatement(SQLiteStatementHandle statementHandle, SQLiteConnectionHandle connectionHandle)
    {
        _handle = statementHandle;
        _connectionHandle = connectionHandle;
    }

    private SQLiteStatement() => throw new NotSupportedException();

    /// <inheritdoc/>
    public void Dispose() => _handle.Dispose();

    /// <summary>
    /// Binds the parameter with a text value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="utf8Text">UTF8 text value of the parameter.</param>
    public void BindTextParameter(ReadOnlySpan<byte> parameterName, ReadOnlySpan<byte> utf8Text)
    {
        SQLiteException.ThrowIfNotNullTerminated(parameterName);
        SQLiteException.ThrowIfNotNullTerminated(utf8Text);
        int idx = NativeMethods.GetParameterIndex(_handle, parameterName);
        NativeMethods.BindText(_handle, _connectionHandle, idx, utf8Text);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="index">Index of the parameter.</param>
    /// <param name="utf8Text">UTF8 text value of the parameter.</param>
    public void BindTextParameter(int index, ReadOnlySpan<byte> utf8Text)
    {
        SQLiteException.ThrowIfNotNullTerminated(utf8Text);
        NativeMethods.BindText(_handle, _connectionHandle, index, utf8Text);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    [Obsolete("Use UTF8 string method instead.", DiagnosticId = "DNSQLL001")]
    public void BindBlobParameter(string parameterName, ReadOnlySpan<byte> value)
    {
        ArgumentNullException.ThrowIfNull(parameterName);
        int idx = NativeMethods.GetParameterIndex(_handle, parameterName);
        BindBlobParameter(idx, value);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    public void BindBlobParameter(ReadOnlySpan<byte> parameterName, ReadOnlySpan<byte> value)
    {
        SQLiteException.ThrowIfNotNullTerminated(parameterName);
        int idx = NativeMethods.GetParameterIndex(_handle, parameterName);
        BindBlobParameter(idx, value);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="index">Index of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    public void BindBlobParameter(int index, ReadOnlySpan<byte> value)
    {
        if (value.IsEmpty)
        {
            ThrowIfValueIsEmpty(nameof(value));
        }

        NativeMethods.BindBlob(_handle, _connectionHandle, index, value);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="index">Index of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    /// <param name="format">Database representation format of the value.</param>
    public void BindParameter(int index, DateTime value, SQLiteDateTimeFormat format)
    {
        if (format == SQLiteDateTimeFormat.ISO8601Text)
        {
            string? text = value.ToString(ISO8601TextFormat, CultureInfo.InvariantCulture);
            BindParameter(index, text);
        }
        else if (format == SQLiteDateTimeFormat.JulianDateReal)
        {
            double r = ToJulianDate(value.ToUniversalTime());
            BindParameter(index, r);
        }
        else if (format == SQLiteDateTimeFormat.UnixTimeInteger)
        {
            long l = new DateTimeOffset(value.ToUniversalTime()).ToUnixTimeSeconds();
            BindParameter(index, l);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(format));
        }
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    /// <param name="format">Database representation format of the value.</param>
    [Obsolete("Use UTF8 string method instead.", DiagnosticId = "DNSQLL001")]
    public void BindParameter(string parameterName, DateTime value, SQLiteDateTimeFormat format)
    {
        ArgumentNullException.ThrowIfNull(parameterName);
        int idx = NativeMethods.GetParameterIndex(_handle, parameterName);
        BindParameter(idx, value, format);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    /// <param name="format">Database representation format of the value.</param>
    public void BindParameter(ReadOnlySpan<byte> parameterName, DateTime value, SQLiteDateTimeFormat format)
    {
        SQLiteException.ThrowIfNotNullTerminated(parameterName);
        int idx = NativeMethods.GetParameterIndex(_handle, parameterName);
        BindParameter(idx, value, format);
    }

    /// <summary>
    /// Binds the parameter with a NULL value.
    /// </summary>
    /// <param name="index">Index of the parameter.</param>
    public void BindNullToParameter(int index)
    {
        int result = NativeMethods.sqlite3_bind_null(_handle, index);
        NativeMethods.CheckResult(result, "sqlite3_bind_null", _connectionHandle);
    }

    /// <summary>
    /// Binds the parameter with a NULL value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    [Obsolete("Use UTF8 string method instead.", DiagnosticId = "DNSQLL001")]
    public void BindNullToParameter(string parameterName)
    {
        ArgumentNullException.ThrowIfNull(parameterName);
        int idx = NativeMethods.GetParameterIndex(_handle, parameterName);
        BindNullToParameter(idx);
    }

    /// <summary>
    /// Binds the parameter with a NULL value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    public void BindNullToParameter(ReadOnlySpan<byte> parameterName)
    {
        SQLiteException.ThrowIfNotNullTerminated(parameterName);
        int idx = NativeMethods.GetParameterIndex(_handle, parameterName);
        BindNullToParameter(idx);
    }

    /// <summary>
    /// Binds the parameter with a BLOB filled with zeroes.
    /// </summary>
    /// <param name="index">Index of the parameter.</param>
    /// <param name="blobLength">Length of the BLOB.</param>
    public void BindZeroedBlobParameter(int index, int blobLength)
    {
        int result = NativeMethods.sqlite3_bind_zeroblob(_handle, index, blobLength);
        NativeMethods.CheckResult(result, "sqlite3_bind_zeroblob", _connectionHandle);
    }

    /// <summary>
    /// Binds the parameter with a BLOB filled with zeroes.
    /// </summary>
    /// <param name="index">Index of the parameter.</param>
    /// <param name="blobLength">Length of the BLOB.</param>
    public void BindZeroedBlobParameter(int index, long blobLength)
    {
        int result = NativeMethods.sqlite3_bind_zeroblob64(_handle, index, blobLength);
        NativeMethods.CheckResult(result, "sqlite3_bind_zeroblob64", _connectionHandle);
    }

    /// <summary>
    /// Evaluate a prepared statement.
    /// </summary>
    /// <returns>The resulting state of the operation.</returns>
    public SQLiteStepResult Step()
    {
        int res = NativeMethods.sqlite3_step(_handle);
        return res switch
        {
            NativeMethods.SQLITE_DONE => SQLiteStepResult.Done,
            NativeMethods.SQLITE_BUSY => SQLiteStepResult.Busy,
            NativeMethods.SQLITE_ROW => SQLiteStepResult.NewRow,
            _ => throw SQLiteException.Create(res, "sqlite3_step", _connectionHandle),
        };
    }

    /// <summary>
    /// Evaluate a prepared statement expecting a new row result.
    /// </summary>
    /// <exception cref="InvalidOperationException">The step result was not a new step.</exception>
    public void NewRowStep()
    {
        SQLiteStepResult res = Step();
        if (res != SQLiteStepResult.NewRow)
        {
            throw new InvalidOperationException($"Expected new row, but step result was {res}!");
        }
    }

    /// <summary>
    /// Evaluate a prepared statement expecting a new row result or done.
    /// </summary>
    /// <exception cref="InvalidOperationException">The step result was not a new step.</exception>
    /// <returns>True, if the step result was a new row and false if the result is Done.</returns>
    public bool TryNewRowStep()
    {
        SQLiteStepResult res = Step();
        return res switch
        {
            SQLiteStepResult.NewRow => true,
            SQLiteStepResult.Done => false,
            _ => throw new InvalidOperationException($"Expected new row or done, but step result was {res}!"),
        };
    }

    /// <summary>
    /// Evaluate a prepared statement expecting a done result.
    /// </summary>
    /// <exception cref="InvalidOperationException">The step result was not done.</exception>
    public void DoneStep()
    {
        SQLiteStepResult res = Step();
        if (res != SQLiteStepResult.Done)
        {
            throw new InvalidOperationException($"Expected done, but step result was {res}!");
        }
    }

    /// <summary>
    /// Reset the prepared statement, so that it can be reused.
    /// </summary>
    public void Reset()
    {
        int res = NativeMethods.sqlite3_reset(_handle);
        NativeMethods.CheckResult(res, "sqlite3_reset", _connectionHandle);
    }

    /// <summary>
    /// Returns the datatype code for the initial data type of the result column.
    /// </summary>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns>The datatype code for the initial data type of the result column.</returns>
    public SQLiteDataType GetColumnDataType(int columnIndex)
    {
        int res = NativeMethods.sqlite3_column_type(_handle, columnIndex);
        return res switch
        {
            NativeMethods.SQLITE_INTEGER => SQLiteDataType.Integer,
            NativeMethods.SQLITE_FLOAT => SQLiteDataType.Float,
            NativeMethods.SQLITE_TEXT => SQLiteDataType.Text,
            NativeMethods.SQLITE_BLOB => SQLiteDataType.Blob,
            NativeMethods.SQLITE_NULL => SQLiteDataType.Null,
            _ => throw SQLiteException.Create(res, "sqlite3_column_type", _connectionHandle),
        };
    }

    /// <summary>
    /// Return the value of a single column of the current result row of a query.
    /// </summary>
    /// <param name="columnIndex">Index of the column.</param>
    /// <param name="format">Database representation format of the value.</param>
    /// <returns>The value of a single column of the current result row of a query.</returns>
    public DateTime GetColumnDateTimeValue(int columnIndex, SQLiteDateTimeFormat format)
    {
        if (format == SQLiteDateTimeFormat.ISO8601Text)
        {
            string? text = GetColumnStringValue(columnIndex);
            DateTimeStyles style = (text[^1] == 'Z') ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None;
            return DateTime.ParseExact(text, ISO8601TextFormat, CultureInfo.InvariantCulture, style);
        }
        else if (format == SQLiteDateTimeFormat.JulianDateReal)
        {
            double r = GetColumnDoubleValue(columnIndex);
            return FromJulianDate(r);
        }
        else if (format == SQLiteDateTimeFormat.UnixTimeInteger)
        {
            long l = GetColumnLongValue(columnIndex);
            return DateTimeOffset.FromUnixTimeSeconds(l).UtcDateTime;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(format));
        }
    }

    /// <summary>
    /// Creates a SQLite statement.
    /// </summary>
    /// <param name="connectionHandle">SQLite connection handle.</param>
    /// <param name="sqlStatement">The SQL statement.</param>
    /// <returns>The prepared SQL statement and part of the statement after the first SQL command.</returns>
    /// <exception cref="SQLiteException">Thrown when the native SQLite library returns an error.</exception>
    [Obsolete("Use UTF8 string method instead.", DiagnosticId = "DNSQLL001")]
    internal static SQLiteStatement Create(SQLiteConnectionHandle connectionHandle, string sqlStatement)
    {
        SQLiteStatementHandle statementHandle = NativeMethods.PrepareStatementHandle(connectionHandle, sqlStatement);
        return new SQLiteStatement(statementHandle, connectionHandle);
    }

    /// <summary>
    /// Creates a SQLite statement.
    /// </summary>
    /// <param name="connectionHandle">SQLite connection handle.</param>
    /// <param name="sqlStatement">The null terminated SQL statement.</param>
    /// <returns>The prepared SQL statement and part of the statement after the first SQL command.</returns>
    /// <exception cref="SQLiteException">Thrown when the native SQLite library returns an error.</exception>
    internal static SQLiteStatement CreateCore(SQLiteConnectionHandle connectionHandle, ReadOnlySpan<byte> sqlStatement)
    {
        // The caller must ensure that the sqlStatement is not null and null-terminated!
        SQLiteStatementHandle statementHandle = NativeMethods.PrepareStatementHandle(connectionHandle, sqlStatement);
        return new SQLiteStatement(statementHandle, connectionHandle);
    }

    private static double ToJulianDate(DateTime dateTime)
    {
        // computeJD
        int y = dateTime.Year;
        int m = dateTime.Month;
        int d = dateTime.Day;

        if (m <= 2)
        {
            y--;
            m += 12;
        }

        int a = y / 100;
        int b = 2 - a + (a / 4);
        int x1 = 36525 * (y + 4716) / 100;
        int x2 = 306001 * (m + 1) / 10000;
        long iJD = (long)((x1 + x2 + d + b - 1524.5) * 86400000);

        iJD += (dateTime.Hour * 3600000) + (dateTime.Minute * 60000) + (long)((dateTime.Second + (dateTime.Millisecond / 1000.0)) * 1000);

        return iJD / 86400000.0;
    }

    private static DateTime FromJulianDate(double julianDate)
    {
        // computeYMD
        long iJD = (long)((julianDate * 86400000.0) + 0.5);
        int z = (int)((iJD + 43200000) / 86400000);
        int a = (int)((z - 1867216.25) / 36524.25);
        a = z + 1 + a - (a / 4);
        int b = a + 1524;
        int c = (int)((b - 122.1) / 365.25);
        int d = 36525 * (c & 32767) / 100;
        int e = (int)((b - d) / 30.6001);
        int x1 = (int)(30.6001 * e);
        int day = b - d - x1;
        int month = e < 14 ? e - 1 : e - 13;
        int year = month > 2 ? c - 4716 : c - 4715;

        // computeHMS
        int s = (int)((iJD + 43200000) % 86400000);
        double fracSecond = s / 1000.0;
        s = (int)fracSecond;
        fracSecond -= s;
        int hour = s / 3600;
        s -= hour * 3600;
        int minute = s / 60;
        fracSecond += s - (minute * 60);

        int second = (int)fracSecond;
        int millisecond = (int)Math.Round((fracSecond - second) * 1000.0);

        return new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
    }

    [DoesNotReturn]
    private static void ThrowIfValueIsEmpty(string paramName)
        => throw new ArgumentNullException(paramName);

    private double DoGetColumnDoubleValue(int columnIndex, SQLiteDataType type)
    {
        return type != SQLiteDataType.Float
            ? throw new InvalidDataException($"Expected type float, but column is of type {type}!")
            : NativeMethods.sqlite3_column_double(_handle, columnIndex);
    }

    private int DoGetColumnIntegerValue(int columnIndex, SQLiteDataType type)
    {
        long value = DoGetColumnLongValue(columnIndex, type);
        return value is < int.MinValue or > int.MaxValue
            ? throw new InvalidCastException("Integer value does not fit in 32bit int.")
            : (int)value;
    }

    private long DoGetColumnLongValue(int columnIndex, SQLiteDataType type)
    {
        return type != SQLiteDataType.Integer
            ? throw new InvalidDataException($"Expected type integer, but column is of type {type}!")
            : NativeMethods.sqlite3_column_int64(_handle, columnIndex);
    }

    private string DoGetColumnStringValue(int columnIndex, SQLiteDataType type)
    {
        if (type != SQLiteDataType.Text)
        {
            throw new InvalidDataException($"Expected type text, but column is of type {type}!");
        }

        return NativeMethods.GetColumnText(_handle, columnIndex);
    }

    private byte[] DoGetColumnBlobValue(int columnIndex, SQLiteDataType type)
    {
        if (type != SQLiteDataType.Blob)
        {
            throw new InvalidDataException($"Expected type blob, but column is of type {type}!");
        }

        int size = NativeMethods.sqlite3_column_bytes(_handle, columnIndex);
        if (size < 0)
        {
            throw new NotSupportedException();
        }

        if (size == 0)
        {
            return [];
        }

        byte[]? bytes = new byte[size];
        IntPtr ptr = NativeMethods.sqlite3_column_blob(_handle, columnIndex);

        Marshal.Copy(ptr, bytes, 0, size);
        return bytes;
    }
}
