// <copyright file="SQLiteException.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

using System.Runtime.Serialization;

namespace SQLiteLibrary;

/// <summary>
/// Exception representing the native SQLite library failures.
/// </summary>
[Serializable]
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

    /// <summary>
    /// Initializes a new instance of the <see cref="SQLiteException"/> class with serialized data.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
    /// object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
    ///    about the source or destination.</param>
    protected SQLiteException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    private SQLiteException(string message, int resultCode, string nativeMethod, string resultCodeName, string nativeErrorMessage)
        : base(message)
    {
        ResultCode = resultCode;
        NativeMethod = nativeMethod;
        ResultCodeName = resultCodeName;
        NativeErrorMessage = nativeErrorMessage;
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

    internal static SQLiteException Create(int resultCode, string nativeMethod, SQLiteConnectionHandle? connectionHandle)
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
        return new SQLiteException(message, resultCode, nativeMethod, resultCodeName, nativeErrorMessage);
    }
}
