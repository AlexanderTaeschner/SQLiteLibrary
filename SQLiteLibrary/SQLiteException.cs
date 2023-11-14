// <copyright file="SQLiteException.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

namespace SQLiteLibrary;

/// <summary>
/// Exception representing the native SQLite library failures.
/// </summary>
public class SQLiteException : Exception
{
    internal SQLiteException()
    {
    }

    internal SQLiteException(string message)
        : base(message)
    {
    }

    internal SQLiteException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    private SQLiteException(string message, int resultCode, string nativeMethod, string resultCodeName, string nativeErrorMessage, string? sqlStatement = null)
        : base(message)
    {
        ResultCode = resultCode;
        NativeMethod = nativeMethod;
        ResultCodeName = resultCodeName;
        NativeErrorMessage = nativeErrorMessage;
        SqlStatement = sqlStatement;
    }

    /// <summary>
    /// Gets the extended result code.
    /// </summary>
    public int ResultCode { get; }

    /// <summary>
    /// Gets the name of the native method which returned the error code.
    /// </summary>
    public string ResultCodeName { get; } = string.Empty;

    /// <summary>
    /// Gets the name of the native method which returned the error code.
    /// </summary>
    public string NativeMethod { get; } = string.Empty;

    /// <summary>
    /// Gets the name of the native method which returned the error code.
    /// </summary>
    public string NativeErrorMessage { get; } = string.Empty;

    /// <summary>
    /// Gets the text used as SQL statement calling the native method.
    /// </summary>
    public string? SqlStatement { get; }

    internal static SQLiteException Create(int resultCode, string nativeMethod, SQLiteConnectionHandle? connectionHandle, string? sqlStatement = null)
    {
        IntPtr utf8Text = NativeMethods.sqlite3_errstr(resultCode);
        string? resultCodeName = NativeMethods.FromUtf8(utf8Text);
        string? nativeErrorMessage = string.Empty;
        if (connectionHandle != null)
        {
            utf8Text = NativeMethods.sqlite3_errmsg(connectionHandle);
            nativeErrorMessage = NativeMethods.FromUtf8(utf8Text);
        }

        string? message = $"SQLiteLibrary.SQLiteException: Native method {nativeMethod} returned error code {resultCodeName}({resultCode}): '{nativeErrorMessage}'!";
        return new SQLiteException(message, resultCode, nativeMethod, resultCodeName, nativeErrorMessage, sqlStatement);
    }
}
