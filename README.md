# SQLiteLibrary

[![Nuget](https://img.shields.io/nuget/v/DotNetSQLiteLibrary)](https://www.nuget.org/packages/DotNetSQLiteLibrary)

This library provides a thin C# wrapper around the prebuilt windows DLL of
the SQLite library <https://sqlite.org>.

The idea is to provide a .NET friendly interface to the C API without extending
its functionality. Therefore only the default SQLite datatypes are supported.
For other data types (e.g. DateTime) the conversion algorithm has to be
specified in the calls. At the moment only the basic functionality of the
C API is implemented (see the SQLiteLibraryTest project for typical use cases).

Since the connection object of the SQLite library is not thread safe, it is
important that in C# for each thread a new `SQLiteConnection` is created.
Sharing a `SQLiteConnection` between threads will cause database corruption
in the long run - do not do this!
