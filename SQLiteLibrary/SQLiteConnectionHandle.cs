// <copyright file="SQLiteConnectionHandle.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;

namespace SQLiteLibrary;

internal sealed class SQLiteConnectionHandle : SafeHandle
{
    public SQLiteConnectionHandle()
    : base(nint.Zero, true)
    {
    }

    public override bool IsInvalid => handle == nint.Zero;

    protected override bool ReleaseHandle()
    {
        int result = NativeMethods.sqlite3_close(handle);
        return result == NativeMethods.SQLITE_OK;
    }
}
