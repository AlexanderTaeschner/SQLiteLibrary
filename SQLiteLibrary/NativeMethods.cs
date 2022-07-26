// <copyright file="NativeMethods.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;
using System.Text;

namespace SQLiteLibrary;

internal static class NativeMethods
{
    /// <summary>
    /// The SQLITE_OK result code means that the operation was successful and that there were no errors. Most other result codes indicate an error.
    /// </summary>
    internal const int SQLITE_OK = 0;

    /// <summary>
    /// The SQLITE_BUSY result code indicates that the database file could not be written (or in some cases read) because of concurrent activity
    /// by some other database connection, usually a database connection in a separate process.
    /// </summary>
    internal const int SQLITE_BUSY = 5;

    /// <summary>
    /// The SQLITE_ROW result code returned by sqlite3_step() indicates that another row of output is available.
    /// </summary>
    internal const int SQLITE_ROW = 100;

    /// <summary>
    /// The SQLITE_DONE result code indicates that an operation has completed.
    /// </summary>
    internal const int SQLITE_DONE = 101;

    /// <summary>
    /// 64-bit signed integer datatype.
    /// </summary>
    internal const int SQLITE_INTEGER = 1;

    /// <summary>
    /// 64-bit IEEE floating point number datatype.
    /// </summary>
    internal const int SQLITE_FLOAT = 2;

    /// <summary>
    /// String datatype.
    /// </summary>
    internal const int SQLITE_TEXT = 3;

    /// <summary>
    /// BLOB datatype.
    /// </summary>
    internal const int SQLITE_BLOB = 4;

    /// <summary>
    /// NULL datatype.
    /// </summary>
    internal const int SQLITE_NULL = 5;

    internal const int SQLITE_OPEN_READONLY = 0x00000001;
    internal const int SQLITE_OPEN_READWRITE = 0x00000002;
    internal const int SQLITE_OPEN_CREATE = 0x00000004;

    internal static readonly IntPtr SQLITE_TRANSIENT = new(-1);

    private const string SQLiteLibraryFileName = "sqlite3.dll";
    private static readonly Encoding s_utf8Encoder = Encoding.UTF8;

    static NativeMethods()
    {
        string? path = new Uri(typeof(NativeMethods).Assembly.Location).LocalPath;
        string? folder = Path.GetDirectoryName(path) ?? throw new FileNotFoundException("Could not load native SQLite library!");

        string? subFolder = Environment.Is64BitProcess ? @"runtimes\win-x64\native" : @"runtimes\win-x86\native";

        IntPtr res = LoadLibrary(Path.Combine(folder, subFolder, SQLiteLibraryFileName));
        if (res != IntPtr.Zero)
        {
            return;
        }

        subFolder = Environment.Is64BitProcess ? @"x64" : @"x86";

        res = LoadLibrary(Path.Combine(folder, subFolder, SQLiteLibraryFileName));
        if (res != IntPtr.Zero)
        {
            return;
        }

        throw new FileNotFoundException("Could not load native SQLite library!");
    }

    internal static void CheckResult(int result, string method, SQLiteConnectionHandle? connectionHandle)
    {
        if (result != SQLITE_OK)
        {
            throw SQLiteException.Create(result, method, connectionHandle);
        }
    }

    internal static byte[] ToUtf8(string s)
    {
        int length = s_utf8Encoder.GetByteCount(s) + 1; // length including appended null char
        byte[] bytes = new byte[length];
        int written = s_utf8Encoder.GetBytes(s, 0, s.Length, bytes, 0);
        if (written != length - 1)
        {
            throw new NotSupportedException();
        }

        bytes[^1] = 0;
        return bytes;
    }

    internal static string FromUtf8(IntPtr ptr)
        => Marshal.PtrToStringUTF8(ptr) ?? throw new ArgumentNullException(nameof(ptr));

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_open_v2(byte[] utf8Filename, out SQLiteConnectionHandle connectionHandle, int flags, IntPtr vfsModule);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_close(IntPtr connectionHandle);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_busy_timeout(SQLiteConnectionHandle connectionHandle, int timeoutInMilliSeconds);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_extended_result_codes(SQLiteConnectionHandle connectionHandle, int onoff);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern long sqlite3_last_insert_rowid(SQLiteConnectionHandle connectionHandle);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern IntPtr sqlite3_errmsg(SQLiteConnectionHandle connectionHandle);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern IntPtr sqlite3_errstr(int resultCode);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_prepare_v2(SQLiteConnectionHandle connectionHandle, byte[] utf8SQLStatement, int utf8SQLStatementByteLength, out SQLiteStatementHandle statementHandle, out IntPtr tail);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_finalize(IntPtr statementHandle);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_bind_blob(SQLiteStatementHandle statementHandle, int index, byte[] value, int valueByteLength, IntPtr destructor);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_bind_double(SQLiteStatementHandle statementHandle, int index, double value);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_bind_int(SQLiteStatementHandle statementHandle, int index, int value);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_bind_int64(SQLiteStatementHandle statementHandle, int index, long value);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_bind_null(SQLiteStatementHandle statementHandle, int index);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_bind_text(SQLiteStatementHandle statementHandle, int index, byte[] utf8Text, int utf8TextByteLength, IntPtr destructor);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_bind_parameter_index(SQLiteStatementHandle statementHandle, byte[] utf8ParameterName);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_bind_zeroblob(SQLiteStatementHandle statementHandle, int index, int blobLength);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_bind_zeroblob64(SQLiteStatementHandle statementHandle, int index, long blobLength);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_step(SQLiteStatementHandle statementHandle);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_reset(SQLiteStatementHandle statementHandle);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern IntPtr sqlite3_column_blob(SQLiteStatementHandle statementHandle, int columnIndex);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern double sqlite3_column_double(SQLiteStatementHandle statementHandle, int columnIndex);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_column_int(SQLiteStatementHandle statementHandle, int columnIndex);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern long sqlite3_column_int64(SQLiteStatementHandle statementHandle, int columnIndex);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern IntPtr sqlite3_column_text(SQLiteStatementHandle statementHandle, int columnIndex);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_column_bytes(SQLiteStatementHandle statementHandle, int columnIndex);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_column_type(SQLiteStatementHandle statementHandle, int columnIndex);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern int sqlite3_column_count(SQLiteStatementHandle statementHandle);

    [DllImport(SQLiteLibraryFileName)]
    internal static extern IntPtr sqlite3_column_name(SQLiteStatementHandle statementHandle, int columnIndex);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string dllToLoad);
}
