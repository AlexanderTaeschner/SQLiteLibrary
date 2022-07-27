// <copyright file="NativeMethods.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;
using System.Text;

namespace SQLiteLibrary;

internal static partial class NativeMethods
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

    internal static unsafe byte* ToUtf8BytePtr(string s)
    {
        if (s is null)
        {
            return null;
        }

        int exactByteCount = checked(Encoding.UTF8.GetByteCount(s) + 1); // + 1 for null terminator
        byte* mem = (byte*)Marshal.AllocCoTaskMem(exactByteCount);
        Span<byte> buffer = new(mem, exactByteCount);

        int byteCount = Encoding.UTF8.GetBytes(s, buffer);
        buffer[byteCount] = 0; // null-terminate
        return mem;
    }

    internal static unsafe void FreeUtf8BytePtr(byte* ptr)
        => Marshal.FreeCoTaskMem((IntPtr)ptr);

    internal static unsafe string FromUtf8(byte* ptr) => FromUtf8((IntPtr)ptr);

    internal static string FromUtf8(IntPtr ptr)
        => Marshal.PtrToStringUTF8(ptr) ?? throw new ArgumentNullException(nameof(ptr));

    [LibraryImport(SQLiteLibraryFileName)]
    internal static unsafe partial int sqlite3_open_v2(byte* utf8Filename, out SQLiteConnectionHandle connectionHandle, int flags, IntPtr vfsModule);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_close(IntPtr connectionHandle);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_busy_timeout(SQLiteConnectionHandle connectionHandle, int timeoutInMilliSeconds);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_extended_result_codes(SQLiteConnectionHandle connectionHandle, int onoff);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial long sqlite3_last_insert_rowid(SQLiteConnectionHandle connectionHandle);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial IntPtr sqlite3_errmsg(SQLiteConnectionHandle connectionHandle);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial IntPtr sqlite3_errstr(int resultCode);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static unsafe partial int sqlite3_prepare_v2(SQLiteConnectionHandle connectionHandle, byte* utf8SQLStatement, int utf8SQLStatementByteLength, out SQLiteStatementHandle statementHandle, out byte* tail);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_finalize(IntPtr statementHandle);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_bind_blob(SQLiteStatementHandle statementHandle, int index, byte[] value, int valueByteLength, IntPtr destructor);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_bind_double(SQLiteStatementHandle statementHandle, int index, double value);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_bind_int(SQLiteStatementHandle statementHandle, int index, int value);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_bind_int64(SQLiteStatementHandle statementHandle, int index, long value);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_bind_null(SQLiteStatementHandle statementHandle, int index);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static unsafe partial int sqlite3_bind_text(SQLiteStatementHandle statementHandle, int index, byte* utf8Text, int utf8TextByteLength, IntPtr destructor);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static unsafe partial int sqlite3_bind_parameter_index(SQLiteStatementHandle statementHandle, byte* utf8ParameterName);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_bind_zeroblob(SQLiteStatementHandle statementHandle, int index, int blobLength);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_bind_zeroblob64(SQLiteStatementHandle statementHandle, int index, long blobLength);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_step(SQLiteStatementHandle statementHandle);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_reset(SQLiteStatementHandle statementHandle);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial IntPtr sqlite3_column_blob(SQLiteStatementHandle statementHandle, int columnIndex);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial double sqlite3_column_double(SQLiteStatementHandle statementHandle, int columnIndex);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_column_int(SQLiteStatementHandle statementHandle, int columnIndex);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial long sqlite3_column_int64(SQLiteStatementHandle statementHandle, int columnIndex);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial IntPtr sqlite3_column_text(SQLiteStatementHandle statementHandle, int columnIndex);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_column_bytes(SQLiteStatementHandle statementHandle, int columnIndex);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_column_type(SQLiteStatementHandle statementHandle, int columnIndex);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial int sqlite3_column_count(SQLiteStatementHandle statementHandle);

    [LibraryImport(SQLiteLibraryFileName)]
    internal static partial IntPtr sqlite3_column_name(SQLiteStatementHandle statementHandle, int columnIndex);

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr LoadLibrary(string dllToLoad);
}
