// <copyright file="SQLiteStatementHandle.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;

namespace SQLiteLibrary;

internal class SQLiteStatementHandle : SafeHandle
{
    public SQLiteStatementHandle()
    : base(IntPtr.Zero, true)
    {
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        _ = NativeMethods.sqlite3_finalize(handle);
        return true;
    }
}
