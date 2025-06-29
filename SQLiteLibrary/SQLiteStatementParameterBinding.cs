// <copyright file="SQLiteStatementParameterBinding.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

namespace SQLiteLibrary;

/// <summary>
/// Prepared SQL statement.
/// </summary>
public sealed partial class SQLiteStatement : IDisposable
{
    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    [Obsolete("Use UTF8 string method instead.", DiagnosticId = "DNSQLL001")]
    public void BindParameter(string parameterName, double value)
    {
        int idx = GetParameterIndex(parameterName ?? throw new ArgumentNullException(nameof(parameterName)));
        BindParameter(idx, value);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    public void BindParameter(ReadOnlySpan<byte> parameterName, double value)
    {
        int idx = GetParameterIndex(parameterName);
        BindParameter(idx, value);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    public void BindNullableParameter(ReadOnlySpan<byte> parameterName, double? value)
    {
        if (value.HasValue)
        {
            BindParameter(parameterName, value.Value);
        }
        else
        {
            BindNullToParameter(parameterName);
        }
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="index">Index of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    public void BindParameter(int index, double value)
    {
        int result = NativeMethods.sqlite3_bind_double(_handle, index, value);
        NativeMethods.CheckResult(result, "sqlite3_bind_double", _connectionHandle);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    [Obsolete("Use UTF8 string method instead.", DiagnosticId = "DNSQLL001")]
    public void BindParameter(string parameterName, int value)
    {
        int idx = GetParameterIndex(parameterName ?? throw new ArgumentNullException(nameof(parameterName)));
        BindParameter(idx, value);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    public void BindParameter(ReadOnlySpan<byte> parameterName, int value)
    {
        int idx = GetParameterIndex(parameterName);
        BindParameter(idx, value);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    public void BindNullableParameter(ReadOnlySpan<byte> parameterName, int? value)
    {
        if (value.HasValue)
        {
            BindParameter(parameterName, value.Value);
        }
        else
        {
            BindNullToParameter(parameterName);
        }
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="index">Index of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    public void BindParameter(int index, int value)
    {
        int result = NativeMethods.sqlite3_bind_int(_handle, index, value);
        NativeMethods.CheckResult(result, "sqlite3_bind_int", _connectionHandle);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    [Obsolete("Use UTF8 string method instead.", DiagnosticId = "DNSQLL001")]
    public void BindParameter(string parameterName, long value)
    {
        int idx = GetParameterIndex(parameterName ?? throw new ArgumentNullException(nameof(parameterName)));
        BindParameter(idx, value);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    public void BindParameter(ReadOnlySpan<byte> parameterName, long value)
    {
        int idx = GetParameterIndex(parameterName);
        BindParameter(idx, value);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    public void BindNullableParameter(ReadOnlySpan<byte> parameterName, long? value)
    {
        if (value.HasValue)
        {
            BindParameter(parameterName, value.Value);
        }
        else
        {
            BindNullToParameter(parameterName);
        }
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="index">Index of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    public void BindParameter(int index, long value)
    {
        int result = NativeMethods.sqlite3_bind_int64(_handle, index, value);
        NativeMethods.CheckResult(result, "sqlite3_bind_int64", _connectionHandle);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    [Obsolete("Use UTF8 string method instead.", DiagnosticId = "DNSQLL001")]
    public void BindParameter(string parameterName, string value)
    {
        int idx = GetParameterIndex(parameterName ?? throw new ArgumentNullException(nameof(parameterName)));
        BindParameter(idx, value);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    public void BindParameter(ReadOnlySpan<byte> parameterName, string value)
    {
        int idx = GetParameterIndex(parameterName);
        BindParameter(idx, value);
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    public void BindNullableParameter(ReadOnlySpan<byte> parameterName, string? value)
    {
        if (value is not null)
        {
            BindParameter(parameterName, value);
        }
        else
        {
            BindNullToParameter(parameterName);
        }
    }

    /// <summary>
    /// Binds the parameter with a value.
    /// </summary>
    /// <param name="index">Index of the parameter.</param>
    /// <param name="value">Value of the parameter.</param>
    public void BindParameter(int index, string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        unsafe
        {
            byte* bytes = NativeMethods.ToUtf8BytePtr(value);
            int result = NativeMethods.sqlite3_bind_text(_handle, index, bytes, -1, NativeMethods.SQLITE_TRANSIENT);
            NativeMethods.FreeUtf8BytePtr(bytes);
            NativeMethods.CheckResult(result, "sqlite3_bind_text", _connectionHandle);
        }
    }
}
