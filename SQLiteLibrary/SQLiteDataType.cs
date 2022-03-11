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
    Integer,

    /// <summary>
    /// 64-bit IEEE floating point number datatype.
    /// </summary>
    Float,

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
