// <copyright file="NativeMethods.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace SQLiteLibrary;

/// <summary>
/// Represents a callback method that handles error logging by receiving an error code and a descriptive message.
/// </summary>
/// <param name="errorCode">The numeric code identifying the type or category of the error. Typically used to distinguish between different
/// error conditions.</param>
/// <param name="message">A descriptive message providing details about the error. This message is intended for logging or diagnostic
/// purposes.</param>
public delegate void ErrorLogCallback(int errorCode, string message);

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

    private const int SQLITE_CONFIG_LOG = 16;

    static NativeMethods()
    {
        string? path = new Uri(typeof(NativeMethods).Assembly.Location).LocalPath;
        string? folder = Path.GetDirectoryName(path) ?? throw new FileNotFoundException("Could not load native SQLite library!");

        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Native SQLite library is only included for Windows!");
        }

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

    private unsafe delegate void NativeErrorLogCallback(IntPtr data, int errorCode, byte* message);

    internal static void CheckResult(int result, string method, SQLiteConnectionHandle? connectionHandle)
    {
        if (result != SQLITE_OK)
        {
            ThrowSQLiteException(result, method, connectionHandle);
        }
    }

    internal static unsafe void CheckResult(int result, string method, SQLiteConnectionHandle connectionHandle, byte* utf8SQLStatement)
    {
        if (result != SQLITE_OK)
        {
            string sqlStatement = FromUtf8(utf8SQLStatement);
            ThrowSQLiteException(result, method, connectionHandle, sqlStatement);
        }
    }

    /// <summary>
    /// Opens a new SQLite database connection with the specified file name and flags.
    /// </summary>
    /// <returns>A handle to the newly opened SQLite database connection. The caller is responsible for managing the lifetime of
    /// the connection and ensuring it is properly disposed of when no longer needed.</returns>
    internal static SQLiteConnectionHandle Sqlite3OpenV2(string fileName, int flags)
    {
        int result;
        SQLiteConnectionHandle connectionHandle;
        unsafe
        {
            byte* utf8Filename = ToUtf8BytePtr(fileName);
            result = sqlite3_open_v2(utf8Filename, out connectionHandle, flags, IntPtr.Zero);
            FreeUtf8BytePtr(utf8Filename);
        }

        CheckResult(result, "sqlite3_open", null);
        result = sqlite3_extended_result_codes(connectionHandle, 1);
        CheckResult(result, "sqlite3_extended_result_codes", connectionHandle);

        return connectionHandle;
    }

    internal static SQLiteStatementHandle PrepareStatementHandle(SQLiteConnectionHandle connectionHandle, string sqlStatement)
    {
        SQLiteStatementHandle? statementHandle = null;
        unsafe
        {
            byte* utf8SQLStatement = ToUtf8BytePtr(sqlStatement);
            try
            {
                int result = sqlite3_prepare_v2(connectionHandle, utf8SQLStatement, -1, out statementHandle, out byte* tail);
                CheckResult(result, "sqlite3_prepare_v2", connectionHandle);
                if (*tail != 0)
                {
                    string remainder = FromUtf8(tail);
                    if (!string.IsNullOrEmpty(remainder))
                    {
                        throw new SQLiteException($"SQL statement contained more than a single SQL command. Additional text: '{remainder}'");
                    }
                }
            }
            catch (SQLiteException)
            {
                // Ensure that we clean up the statement handle if an exception occurs.
                statementHandle?.Dispose();
                throw;
            }
            finally
            {
                FreeUtf8BytePtr(utf8SQLStatement);
            }
        }

        return statementHandle;
    }

    internal static SQLiteStatementHandle PrepareStatementHandle(SQLiteConnectionHandle connectionHandle, ReadOnlySpan<byte> sqlStatement)
    {
        // The caller must ensure that the sqlStatement is not null and null-terminated!
        unsafe
        {
            fixed (byte* utf8SQLStatement = sqlStatement)
            {
                int result = sqlite3_prepare_v2(connectionHandle, utf8SQLStatement, -1, out SQLiteStatementHandle statementHandle, out byte* tail);
                CheckResult(result, "sqlite3_prepare_v2", connectionHandle, utf8SQLStatement);
                if (*tail != 0)
                {
                    string remainder = FromUtf8(tail);
                    if (!string.IsNullOrEmpty(remainder))
                    {
                        throw new SQLiteException($"SQL statement contained more than a single SQL command. Additional text: '{remainder}'");
                    }
                }

                return statementHandle;
            }
        }
    }

    internal static void BindText(SQLiteStatementHandle handle, SQLiteConnectionHandle connectionHandle, int index, ReadOnlySpan<byte> utf8Text)
    {
        // The callers have to ensure that utf8Text is not empty and null terminated!
        unsafe
        {
            fixed (byte* bytes = utf8Text)
            {
                int result = sqlite3_bind_text(handle, index, bytes, -1, SQLITE_TRANSIENT);
                CheckResult(result, "sqlite3_bind_text", connectionHandle);
            }
        }
    }

    internal static void BindText(SQLiteStatementHandle handle, SQLiteConnectionHandle connectionHandle, int index, string value)
    {
        unsafe
        {
            byte* bytes = ToUtf8BytePtr(value);
            int result = sqlite3_bind_text(handle, index, bytes, -1, SQLITE_TRANSIENT);
            FreeUtf8BytePtr(bytes);
            CheckResult(result, "sqlite3_bind_text", connectionHandle);
        }
    }

    internal static void BindBlob(SQLiteStatementHandle handle, SQLiteConnectionHandle connectionHandle, int index, ReadOnlySpan<byte> value)
    {
        unsafe
        {
            fixed (byte* bytes = value)
            {
                int result = sqlite3_bind_blob(handle, index, bytes, value.Length, SQLITE_TRANSIENT);
                CheckResult(result, "sqlite3_bind_blob", connectionHandle);
            }
        }
    }

    internal static int GetParameterIndex(SQLiteStatementHandle handle, string parameterName)
    {
        unsafe
        {
            byte* ut8Text = ToUtf8BytePtr(parameterName);
            int idx = sqlite3_bind_parameter_index(handle, ut8Text);
            FreeUtf8BytePtr(ut8Text);
            return idx == 0 ? throw new KeyNotFoundException($"Paramter name '{parameterName}' was not found in the prepared statement!") : idx;
        }
    }

    internal static int GetParameterIndex(SQLiteStatementHandle handle, ReadOnlySpan<byte> parameterName)
    {
        // The caller must ensure that the parameterName is not empty and null-terminated!
        unsafe
        {
            fixed (byte* ut8Text = parameterName)
            {
                int idx = sqlite3_bind_parameter_index(handle, ut8Text);
                return idx == 0 ? throw new KeyNotFoundException($"Paramter name '{NativeMethods.FromUtf8(ut8Text)}' was not found in the prepared statement!") : idx;
            }
        }
    }

    internal static string GetColumnText(SQLiteStatementHandle handle, int columnIndex)
    {
        IntPtr utf8Text = NativeMethods.sqlite3_column_text(handle, columnIndex);
        return NativeMethods.FromUtf8(utf8Text);
    }

    internal static string GetErrorMessage(SQLiteConnectionHandle connectionHandle)
    {
        IntPtr utf8Text = sqlite3_errmsg(connectionHandle);
        return FromUtf8(utf8Text);
    }

    internal static string GetErrorString(int resultCode)
    {
        IntPtr utf8Text = sqlite3_errstr(resultCode);
        return FromUtf8(utf8Text);
    }

    /// <summary>
    /// Converts the specified string to a UTF-8 encoded byte array with a null terminator.
    /// </summary>
    /// <remarks>The returned byte array includes a null terminator at the end, making it suitable for
    /// interoperation with APIs that expect null-terminated UTF-8 strings.</remarks>
    /// <param name="s">The string to convert. If <paramref name="s"/> is <see langword="null"/>, a byte array containing only the null
    /// terminator is returned.</param>
    /// <returns>A byte array containing the UTF-8 encoded representation of the input string, followed by a null terminator.</returns>
    internal static byte[] ToUtf8Bytes(string s)
    {
        if (s is null)
        {
            return [0];
        }

        int exactByteCount = checked(Encoding.UTF8.GetByteCount(s) + 1); // + 1 for null terminator
        byte[] buffer = new byte[exactByteCount];

        int byteCount = Encoding.UTF8.GetBytes(s, buffer);
        buffer[byteCount] = 0; // null-terminate
        return buffer;
    }

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial int sqlite3_close(IntPtr connectionHandle);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial int sqlite3_busy_timeout(SQLiteConnectionHandle connectionHandle, int timeoutInMilliSeconds);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial long sqlite3_last_insert_rowid(SQLiteConnectionHandle connectionHandle);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial int sqlite3_finalize(IntPtr statementHandle);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial int sqlite3_bind_double(SQLiteStatementHandle statementHandle, int index, double value);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial int sqlite3_bind_int(SQLiteStatementHandle statementHandle, int index, int value);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial int sqlite3_bind_int64(SQLiteStatementHandle statementHandle, int index, long value);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial int sqlite3_bind_null(SQLiteStatementHandle statementHandle, int index);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial int sqlite3_bind_zeroblob(SQLiteStatementHandle statementHandle, int index, int blobLength);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial int sqlite3_bind_zeroblob64(SQLiteStatementHandle statementHandle, int index, long blobLength);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial int sqlite3_step(SQLiteStatementHandle statementHandle);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial int sqlite3_reset(SQLiteStatementHandle statementHandle);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial IntPtr sqlite3_column_blob(SQLiteStatementHandle statementHandle, int columnIndex);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial double sqlite3_column_double(SQLiteStatementHandle statementHandle, int columnIndex);

    // Since an SQLite integer can be also an Int64, we use sqlite3_column_int64 and check the size of the value.
    ////[LibraryImport(SQLiteLibraryFileName)]
    ////[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    ////internal static partial int sqlite3_column_int(SQLiteStatementHandle statementHandle, int columnIndex);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial long sqlite3_column_int64(SQLiteStatementHandle statementHandle, int columnIndex);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial int sqlite3_column_bytes(SQLiteStatementHandle statementHandle, int columnIndex);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial int sqlite3_column_type(SQLiteStatementHandle statementHandle, int columnIndex);

    internal static void SetErrorLogCallback(ErrorLogCallback errorLogCallback)
    {
        unsafe
        {
            void nativeCallback(nint data, int errorCode, byte* message)
            {
                string msg = FromUtf8(message);
                errorLogCallback(errorCode, msg);
            }

            int result = sqlite3_config(SQLITE_CONFIG_LOG, nativeCallback, IntPtr.Zero);
            CheckResult(result, "sqlite3_config", null);
        }
    }

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    private static partial int sqlite3_config(int option, NativeErrorLogCallback errorLogCallback, IntPtr data);

    /* Not used at the moment, but might be useful in the future.
    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial int sqlite3_column_count(SQLiteStatementHandle statementHandle);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static partial IntPtr sqlite3_column_name(SQLiteStatementHandle statementHandle, int columnIndex);
    */

    /// <summary>
    /// Converts the specified string to a null-terminated UTF-8 encoded byte pointer.
    /// </summary>
    /// <remarks>The caller is responsible for freeing the allocated memory using <see
    /// cref="Marshal.FreeCoTaskMem(IntPtr)"/>  to avoid memory leaks. The returned pointer is allocated using COM task
    /// memory.</remarks>
    /// <param name="s">The string to convert. If <see langword="null"/>, the method returns <see langword="null"/>.</param>
    /// <returns>A pointer to a null-terminated UTF-8 encoded byte array representing the input string,  or <see
    /// langword="null"/> if the input string is <see langword="null"/>.</returns>
    private static unsafe byte* ToUtf8BytePtr(string s)
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

    private static unsafe void FreeUtf8BytePtr(byte* ptr)
        => Marshal.FreeCoTaskMem((IntPtr)ptr);

    private static unsafe string FromUtf8(byte* ptr) => FromUtf8((IntPtr)ptr);

    private static string FromUtf8(IntPtr ptr)
        => Marshal.PtrToStringUTF8(ptr) ?? throw new ArgumentNullException(nameof(ptr));

    [DoesNotReturn]
    private static void ThrowSQLiteException(int result, string method, SQLiteConnectionHandle? connectionHandle, string? sqlStatement = null)
        => throw SQLiteException.Create(result, method, connectionHandle, sqlStatement);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    private static unsafe partial int sqlite3_open_v2(byte* utf8Filename, out SQLiteConnectionHandle connectionHandle, int flags, IntPtr vfsModule);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    private static partial int sqlite3_extended_result_codes(SQLiteConnectionHandle connectionHandle, int onoff);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    private static partial IntPtr sqlite3_errmsg(SQLiteConnectionHandle connectionHandle);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    private static partial IntPtr sqlite3_errstr(int resultCode);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    private static unsafe partial int sqlite3_prepare_v2(SQLiteConnectionHandle connectionHandle, byte* utf8SQLStatement, int utf8SQLStatementByteLength, out SQLiteStatementHandle statementHandle, out byte* tail);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    private static unsafe partial int sqlite3_bind_blob(SQLiteStatementHandle statementHandle, int index, byte* value, int valueByteLength, IntPtr destructor);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    private static unsafe partial int sqlite3_bind_text(SQLiteStatementHandle statementHandle, int index, byte* utf8Text, int utf8TextByteLength, IntPtr destructor);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    private static unsafe partial int sqlite3_bind_parameter_index(SQLiteStatementHandle statementHandle, byte* utf8ParameterName);

    [LibraryImport(SQLiteLibraryFileName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    private static partial IntPtr sqlite3_column_text(SQLiteStatementHandle statementHandle, int columnIndex);

    [LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial IntPtr LoadLibrary(string dllToLoad);
}
