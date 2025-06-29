// <copyright file="SQLiteStatementColumnValues.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

namespace SQLiteLibrary;

/// <summary>
/// Prepared SQL statement.
/// </summary>
public sealed partial class SQLiteStatement : IDisposable
{
    /// <summary>
    /// Return the value of a single column of the current result row of a query.
    /// </summary>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns>The value of a single column of the current result row of a query.</returns>
    public double GetColumnDoubleValue(int columnIndex)
    {
        SQLiteDataType type = GetColumnDataType(columnIndex);
        return DoGetColumnDoubleValue(columnIndex, type);
    }

    /// <summary>
    /// Return the value of a single column of the current result row of a query.
    /// </summary>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns>The value of a single column of the current result row of a query.</returns>
    public double? GetColumnNullableDoubleValue(int columnIndex)
    {
        SQLiteDataType type = GetColumnDataType(columnIndex);
        return type == SQLiteDataType.Null ? (double?)null : DoGetColumnDoubleValue(columnIndex, type);
    }

    /// <summary>
    /// Return the value of a single column of the current result row of a query.
    /// </summary>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns>The value of a single column of the current result row of a query.</returns>
    public int GetColumnIntegerValue(int columnIndex)
    {
        SQLiteDataType type = GetColumnDataType(columnIndex);
        return DoGetColumnIntegerValue(columnIndex, type);
    }

    /// <summary>
    /// Return the value of a single column of the current result row of a query.
    /// </summary>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns>The value of a single column of the current result row of a query.</returns>
    public int? GetColumnNullableIntegerValue(int columnIndex)
    {
        SQLiteDataType type = GetColumnDataType(columnIndex);
        return type == SQLiteDataType.Null ? (int?)null : DoGetColumnIntegerValue(columnIndex, type);
    }

    /// <summary>
    /// Return the value of a single column of the current result row of a query.
    /// </summary>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns>The value of a single column of the current result row of a query.</returns>
    public long GetColumnLongValue(int columnIndex)
    {
        SQLiteDataType type = GetColumnDataType(columnIndex);
        return DoGetColumnLongValue(columnIndex, type);
    }

    /// <summary>
    /// Return the value of a single column of the current result row of a query.
    /// </summary>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns>The value of a single column of the current result row of a query.</returns>
    public long? GetColumnNullableLongValue(int columnIndex)
    {
        SQLiteDataType type = GetColumnDataType(columnIndex);
        return type == SQLiteDataType.Null ? (long?)null : DoGetColumnLongValue(columnIndex, type);
    }

    /// <summary>
    /// Return the value of a single column of the current result row of a query.
    /// </summary>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns>The value of a single column of the current result row of a query.</returns>
    public string GetColumnStringValue(int columnIndex)
    {
        SQLiteDataType type = GetColumnDataType(columnIndex);
        return DoGetColumnStringValue(columnIndex, type);
    }

    /// <summary>
    /// Return the value of a single column of the current result row of a query.
    /// </summary>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns>The value of a single column of the current result row of a query.</returns>
    public string? GetColumnNullableStringValue(int columnIndex)
    {
        SQLiteDataType type = GetColumnDataType(columnIndex);
        return type == SQLiteDataType.Null ? (string?)null : DoGetColumnStringValue(columnIndex, type);
    }

    /// <summary>
    /// Return the value of a single column of the current result row of a query.
    /// </summary>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns>The value of a single column of the current result row of a query.</returns>
    public byte[] GetColumnBlobValue(int columnIndex)
    {
        SQLiteDataType type = GetColumnDataType(columnIndex);
        return DoGetColumnBlobValue(columnIndex, type);
    }

    /// <summary>
    /// Return the value of a single column of the current result row of a query.
    /// </summary>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns>The value of a single column of the current result row of a query.</returns>
    public byte[]? GetColumnNullableBlobValue(int columnIndex)
    {
        SQLiteDataType type = GetColumnDataType(columnIndex);
        return type == SQLiteDataType.Null ? (byte[]?)null : DoGetColumnBlobValue(columnIndex, type);
    }
}
