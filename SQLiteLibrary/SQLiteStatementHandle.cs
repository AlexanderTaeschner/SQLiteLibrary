// <copyright file="SQLiteStatementHandle.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;

namespace SQLiteLibrary;

internal sealed class SQLiteStatementHandle : SafeHandle
{
    public SQLiteStatementHandle()
    : base(nint.Zero, true)
    {
    }

    public override bool IsInvalid => handle == nint.Zero;

    protected override bool ReleaseHandle()
    {
        _ = NativeMethods.sqlite3_finalize(handle);
        return true;
    }
}
