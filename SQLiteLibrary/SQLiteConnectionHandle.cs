// <copyright file="SQLiteConnectionHandle.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;

namespace SQLiteLibrary;

internal class SQLiteConnectionHandle : SafeHandle
{
    public SQLiteConnectionHandle()
    : base(IntPtr.Zero, true)
    {
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        int result = NativeMethods.sqlite3_close(handle);
        return result == NativeMethods.SQLITE_OK;
    }
}
