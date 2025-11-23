// <copyright file="SQLiteDataType.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

namespace SQLiteLibrary;

/// <summary>
/// Fundamental SQLite datatypes.
/// </summary>
public enum SQLiteDataType
{
    /// <summary>
    /// 64-bit signed integer datatype.
    /// </summary>
#pragma warning disable CA1720 // Identifier contains type name
    Integer,
#pragma warning restore CA1720 // Identifier contains type name

    /// <summary>
    /// 64-bit IEEE floating point number datatype.
    /// </summary>
#pragma warning disable CA1720 // Identifier contains type name
    Float,
#pragma warning restore CA1720 // Identifier contains type name

    /// <summary>
    /// String datatype.
    /// </summary>
    Text,

    /// <summary>
    /// Blob datatype.
    /// </summary>
    Blob,

    /// <summary>
    /// NULL datatype.
    /// </summary>
    Null,
}
