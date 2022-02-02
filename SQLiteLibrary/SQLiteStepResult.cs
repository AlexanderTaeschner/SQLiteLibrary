// <copyright file="SQLiteStepResult.cs" company="Alexander Täschner">
// Copyright (c) Alexander Täschner. All rights reserved.
// </copyright>

namespace SQLiteLibrary;

/// <summary>
/// Result of the step operation.
/// </summary>
public enum SQLiteStepResult
{
    /// <summary>
    /// The operation has completed.
    /// </summary>
    Done,

    /// <summary>
    /// The database engine was unable to acquire the database locks it needs to do its job. If the statement is a COMMIT
    /// or occurs outside of an explicit transaction, then you can retry the statement. If the statement is not a COMMIT
    /// and occurs within an explicit transaction then you should rollback the transaction before continuing.
    /// </summary>
    Busy,

    /// <summary>
    /// A new row of data is ready for processing.
    /// </summary>
    NewRow,
}
