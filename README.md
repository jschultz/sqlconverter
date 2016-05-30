# sqlconverter

This code was [originally published by Liron Levi](http://www.codeproject.com/Articles/26932/Convert-SQL-Server-DB-to-SQLite-DB) whose release notes are reproduced below. I moved the source to github in order to facilitate ongoing development.

### Introduction

I needed to convert the existing SQL server databases to SQLite databases as part of a DB migration program and did not find any decent free converter to do the job.

This is my attempt to solve the problem. I should warn you though that I did not have much time to test it on too many databases. In any case - the source code is very well documented and easy to understand, so if you do have a problem it should be relatively easy to fix. (Please send me the fixed source code. If you do so, I can update the software so that everybody can enjoy it.)

### Using the Code

The code is split between a dataaccess project (class library) that contains the conversion code itself and a converter project (WinForms) that drives the conversion code and provides a simple UI for user interaction.

The main class that performs the conversion is the sqlservertosqlite class. It does the conversion by doing the following steps:

1. Reading the designated SQL server schema and preparing a list of tableschema objects that contain the schema for each and every SQL server table (and associated structures like indexes).
2. Preparing an empty sqlite database file with the schema that was read from SQL server. In this step, the code may alter few SQL-server types that are not supported directly in sqlite.
3. Copying rows for each table from the SQL server database to the sqlite database.

Basically, that's it!

### Points of Interest

In order to read the SQL server DB schema, I was mainly using the pseudo information_schema.table table. You can find more information about it on the Internet if you want.

### History

    13th June, 2008: Initial version
    08th July, 2008: v 1.2
        Fixed a bug that caused unique indexes to be generated as non-unique in some cases
    08th July, 2008: v 1.3
        Fixed a bug that caused the utility to crash sometimes when processing index information
    17th July, 2008: v 1.4
        Fixed a bug that caused wrong columns to become primary keys on rare occasions and improved conversion performance
    20th July, 2008: v 1.5
        Added support for case-insensitive columns (COLLATE NOCASE)
    22nd July, 2008: v 1.6
        Added support for encrypting the resulting DB file (using the built-in encryption support that exists in the SQLite .NET provider)
    05th October, 2008: v 1.7
        Fixed information_schema references to use UPPER-CASE in order to resolve international character set issues (Turkish)
    14th December 2008: v1.8
        Integrated support for foreign keys from the revised version made by Yogesh Jagota
        Merged support for selective table import
    21st February 2009: v1.9
        Added contribution from johnny dickson cano that allows to select using SQL server integrated security or using user name /password instead
        Added support for converting IDENTITY columns to AUTOINCREMENT in SQLite (suggestion by Paul Shaffer)
    04th March 2009: v1.10
        Fixed a bug that caused the converter to crash when encountering a datetime field in the original SQL server schema. Thanks to bmcclint for sending me the bug with the correct bugfix.
    23rd May 2009: v1.11
        Added support for simulating foreign keys using triggers (contributed by Martijn Muurman)
        Added a small bugfix so that now an 'int' type is always converted to 'integer' type in sqlite. This was needed because sqlite will autoincrement only on 'integer' column types.
    04th June 2009: v1.12
        Fixed a bug in trigger generation code that caused schema generation to fail when more than a single column is referencing the same column in a foreign table
    20th September 2009: v1.13
        Fixed AUTOINCREMENT bug suggested by MAEP
        Fixed 64 bit support problem (thanks to Murry Gammash)
        Added support for converting SQL Server views (suggested by Richard Thurgood)
    22nd September 2009: v1.14
        Fixed a critical bug that caused the conversion process to fail on some SQL Server databases that used the [dbo] notation.
    25th September 2009: v1.15
        Fixed a critical bug that caused the primary keys to be discarded
        Fixed trigger generation bug
    4th December 2009: v1.16
        Fixed generation code to create GUID types for SQL-Server's uniqueidentifier type (instead of nvarchar as it was until now)
        Updated the solution to use the latest SQLite .NET provider library
    24th March, 2011
        Attached compiled version of the project for anyone that doesn't have Visual Studio and still needs to use the utility
    1st July, 2011
        Updated binary zip as the earlier one was missing the DLL file needed by the application
    15th Nov 2011: v1.17
        Fixed a bug that caused the software to crash when encountering NULL values in some of SQL Server meta data tables
    19th June 2012: v1.19
        Added support for ignoring views when creating the DB schema, Added support for blank characters inside column names.
     14th January 2013: v1.20
        Fixed problem with column names
        Added more width to the database names combo box. 
