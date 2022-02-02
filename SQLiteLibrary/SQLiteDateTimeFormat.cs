// <copyright file="SQLiteDateTimeFormat.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

namespace SQLiteLibrary;

/// <summary>
/// Database representation format of a <see cref="System.DateTime"/> instance.
/// </summary>
public enum SQLiteDateTimeFormat
{
    /// <summary>
    /// TEXT as ISO8601 strings ("YYYY-MM-DD HH:MM:SS.SSS").
    /// </summary>
    ISO8601Text,

    /// <summary>
    /// REAL as Julian day numbers, the number of days since noon in Greenwich on November 24, 4714 B.C. according to the proleptic Gregorian calendar.
    /// </summary>
    JulianDateReal,

    /// <summary>
    /// INTEGER as Unix Time, the number of seconds since 1970-01-01 00:00:00 UTC.
    /// </summary>
    UnixTimeInteger,
}
