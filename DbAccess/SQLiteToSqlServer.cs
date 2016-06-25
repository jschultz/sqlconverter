using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using log4net;

namespace DbAccess
{
    /// <summary>
    /// This class is resposible to take a single SQL Server database
    /// and convert it to an SQLite database file.
    /// </summary>
    /// <remarks>The class knows how to convert table and index structures only.</remarks>
    public class SQLiteToSqlServer
    {
        #region Public Properties
        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        public static bool IsActive
        {
            get { return _isActive; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Cancels the conversion.
        /// </summary>
        public static void CancelConversion()
        {
            _cancelled = true;
        }

        /// <summary>
        /// This method takes as input the connection string to an SQL Server database
        /// and creates a corresponding SQLite database file with a schema derived from
        /// the SQL Server database.
        /// </summary>
        /// <param name="sqlServerConnString">The connection string to the SQL Server database.</param>
        /// <param name="sqlitePath">The path to the SQLite database file that needs to get created.</param>
        /// <param name="password">The password to use or NULL if no password should be used to encrypt the DB</param>
        /// <param name="handler">A handler delegate for progress notifications.</param>
        /// <param name="selectionHandler">The selection handler that allows the user to select which
        /// tables to convert</param>
        /// <remarks>The method continues asynchronously in the background and the caller returned
        /// immediatly.</remarks>
        public static void ConvertSQLiteToSqlServerDatabase(string sqlServerConnString,
            string sqlitePath, string password, SqlConversionHandler handler,
            SqlTableSelectionHandler selectionHandler,
            FailedViewDefinitionHandler viewFailureHandler,
            bool copyStructure, bool copyData)
        {
            // Clear cancelled flag
            _cancelled = false;

            WaitCallback wc = new WaitCallback(delegate(object state)
            {
                try
                {
                    _isActive = true;
                    ConvertSQLiteToSqlServerDatabaseFile(sqlServerConnString, sqlitePath, password, handler, selectionHandler, viewFailureHandler, copyStructure, copyData);
                    _isActive = false;
                    handler(true, true, 100, "Finished converting database");
                }
                catch (Exception ex)
                {
                    _log.Error("Failed to convert SQL Server database to SQLite database", ex);
                    _isActive = false;
                    handler(true, false, 100, ex.Message);
                } // catch
            });
            ThreadPool.QueueUserWorkItem(wc);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Do the entire process of first reading the SQL Server schema, creating a corresponding
        /// SQLite schema, and copying all rows from the SQL Server database to the SQLite database.
        /// </summary>
        /// <param name="sqlConnString">The SQL Server connection string</param>
        /// <param name="sqlitePath">The path to the generated SQLite database file</param>
        /// <param name="password">The password to use or NULL if no password should be used to encrypt the DB</param>
        /// <param name="handler">A handler to handle progress notifications.</param>
        /// <param name="selectionHandler">The selection handler which allows the user to select which tables to 
        /// convert.</param>
        private static void ConvertSQLiteToSqlServerDatabaseFile(
            string sqlConnString, string sqlitePath, string password, SqlConversionHandler handler,
            SqlTableSelectionHandler selectionHandler,
            FailedViewDefinitionHandler viewFailureHandler,
            bool copyStructure, bool copyData)
        {
            // Read the schema of the SQL Server database into a memory structure
            DatabaseSchema ds = ReadSQLiteSchema(sqlitePath, password, handler, selectionHandler);
            if (ds == null) {
                CancelConversion();
                CheckCancelled();
                return;
            }

            // Create the SQLite database and apply the schema
            if (copyStructure) {
                CreateSqlServerDatabase(sqlConnString, ds, handler, viewFailureHandler);
                CreateSqlServerForeignKeys(sqlConnString, ds, handler, viewFailureHandler);
            }

            // Copy all rows from SQL Server tables to the newly created SQLite database
            if (copyData) {
                CopySQLiteRowsToSqlServerDB(sqlitePath, sqlConnString, ds.Tables, password, handler);
            }
        }

        /// <summary>
        /// Copies table rows from the SQL Server database to the SQLite database.
        /// </summary>
        /// <param name="sqlConnString">The SQL Server connection string</param>
        /// <param name="sqlitePath">The path to the SQLite database file.</param>
        /// <param name="schema">The schema of the SQL Server database.</param>
        /// <param name="password">The password to use for encrypting the file</param>
        /// <param name="handler">A handler to handle progress notifications.</param>
        private static void CopySQLiteRowsToSqlServerDB(
            string sqlitePath, string sqlConnString, List<TableSchema> schema,
            string password, SqlConversionHandler handler)
        {
            CheckCancelled();
            handler(false, true, 0, "Preparing to insert tables...");
            _log.Debug("preparing to insert tables ...");

            // Connect to the SQL Server database
            using (SqlConnection SqlServerConn = new SqlConnection(sqlConnString))
            {
                SqlServerConn.Open();

                // Connect to the SQLite database next
                string sqliteConnString = CreateSQLiteConnectionString(sqlitePath, password);
                using (SQLiteConnection SQLiteConn = new SQLiteConnection(sqliteConnString))
                {
                    SQLiteConn.Open();

                    // Go over all tables in the schema and copy their rows
                    for (int i = 0; i < schema.Count; i++)
                    {
                        SqlTransaction tx = SqlServerConn.BeginTransaction();
                        try
                        {
                            string tableQuery = BuildSQLiteTableQuery(schema[i]);
                            SQLiteCommand query = new SQLiteCommand(tableQuery, SQLiteConn);
                            using (SQLiteDataReader reader = query.ExecuteReader())
                            {
                                SqlCommand insert = BuildSqlServerInsert(schema[i]);
                                int counter = 0;
                                while (reader.Read())
                                {
                                    insert.Connection = SqlServerConn;
                                    insert.Transaction = tx;
                                    List<string> pnames = new List<string>();
                                    for (int j = 0; j < schema[i].Columns.Count; j++)
                                    {
                                        string pname = "@" + GetNormalizedName(schema[i].Columns[j].ColumnName, pnames);
                                        insert.Parameters[pname].Value = CastValueForColumn(reader[j], schema[i].Columns[j]);
                                        pnames.Add(pname);
                                    }
                                    insert.ExecuteNonQuery();
                                    counter++;
                                    if (counter % 1000 == 0)
                                    {
                                        CheckCancelled();
                                        tx.Commit();
                                        handler(false, true, (int)(100.0 * i / schema.Count),
                                            "Added " + counter + " rows to table " + schema[i].TableName + " so far");
                                        tx = SqlServerConn.BeginTransaction();
                                    }
                                } // while
                            } // using

                            CheckCancelled();
                            tx.Commit();

                            handler(false, true, (int)(100.0 * i / schema.Count), "Finished inserting rows for table " + schema[i].TableName);
                            _log.Debug("finished inserting all rows for table [" + schema[i].TableName + "]");
                        }
                        catch (Exception ex)
                        {
                            _log.Error("unexpected exception", ex);
                            tx.Rollback();
                            throw;
                        } // catch
                    }
                } // using
                SqlConnection.ClearPool(SqlServerConn);
            } // using
        }

        /// <summary>
        /// Used in order to adjust the value received from SQL Servr for the SQLite database.
        /// </summary>
        /// <param name="val">The value object</param>
        /// <param name="columnSchema">The corresponding column schema</param>
        /// <returns>SQLite adjusted value.</returns>
        private static object CastValueForColumn(object val, ColumnSchema columnSchema)
        {
            if (val is DBNull)
                return DBNull.Value;

            SqlDbType dt = GetDbTypeOfColumn(columnSchema);

            switch (dt)
            {
                case SqlDbType.Int:
                    if (val is short)
                        return (int)(short)val;
                    if (val is byte)
                        return (int)(byte)val;
                    if (val is long)
                        return (int)(long)val;
                    if (val is decimal)
                        return (int)(decimal)val;
                    break;

                case SqlDbType.SmallInt:
                    if (val is int)
                        return (short)(int)val;
                    if (val is byte)
                        return (short)(byte)val;
                    if (val is long)
                        return (short)(long)val;
                    if (val is decimal)
                        return (short)(decimal)val;
                    break;

                case SqlDbType.BigInt:
                    if (val is int)
                        return (long)(int)val;
                    if (val is short)
                        return (long)(short)val;
                    if (val is byte)
                        return (long)(byte)val;
                    if (val is decimal)
                        return (long)(decimal)val;
                    break;

                case SqlDbType.Real:
                    if (val is double)
                        return (float)(double)val;
                    if (val is decimal)
                        return (float)(decimal)val;
                    break;

                case SqlDbType.Float:
                    if (val is float)
                        return (double)(float)val;
                    if (val is double)
                        return (double)val;
                    if (val is decimal)
                        return (double)(decimal)val;
                    break;

                case SqlDbType.Text:
                    if (val is Guid)
                        return ((Guid)val).ToString();
                    break;

                case SqlDbType.UniqueIdentifier:
                    if (val is string)
                        return ParseStringAsGuid((string)val);
                    if (val is byte[])
                        return ParseBlobAsGuid((byte[])val);
                    break;

                case SqlDbType.Binary:
                case SqlDbType.Bit:
                case SqlDbType.DateTime:
                    break;

                default:
                    _log.Error("argument exception - illegal database type");
                    throw new ArgumentException("Illegal database type [" + Enum.GetName(typeof(SqlDbType), dt) + "]");
            } // switch

            return val;
        }

        private static Guid ParseBlobAsGuid(byte[] blob)
        {
            byte[] data = blob;
            if (blob.Length > 16)
            {
                data = new byte[16];
                for (int i = 0; i < 16; i++)
                    data[i] = blob[i];
            }
            else if (blob.Length < 16)
            {
                data = new byte[16];
                for (int i = 0; i < blob.Length; i++)
                    data[i] = blob[i];
            }

            return new Guid(data);
        }

        private static Guid ParseStringAsGuid(string str)
        {
            try
            {
                return new Guid(str);
            }
            catch (Exception ex)
            {
                return Guid.Empty;
            } // catch
        }

        /// <summary>
        /// Creates a command object needed to insert values into a specific SQLite table.
        /// </summary>
        /// <param name="ts">The table schema object for the table.</param>
        /// <returns>A command object with the required functionality.</returns>
        private static SqlCommand BuildSqlServerInsert(TableSchema ts)
        {
            SqlCommand res = new SqlCommand();

            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO [" + ts.TableName + "] (");
            for (int i = 0; i < ts.Columns.Count; i++)
            {
                sb.Append("[" + ts.Columns[i].ColumnName + "]");
                if (i < ts.Columns.Count - 1)
                    sb.Append(", ");
            } // for
            sb.Append(") VALUES (");

            List<string> pnames = new List<string>();
            for (int i = 0; i < ts.Columns.Count; i++)
            {
                string pname = "@" + GetNormalizedName(ts.Columns[i].ColumnName, pnames);
                sb.Append(pname);
                if (i < ts.Columns.Count - 1)
                    sb.Append(", ");

                SqlDbType dbType = GetDbTypeOfColumn(ts.Columns[i]);
                //SqlParameter prm = new SqlParameter(pname, dbType, ts.Columns[i].ColumnName);
                SqlParameter prm = new SqlParameter(pname, dbType);
                res.Parameters.Add(prm);

                // Remember the parameter name in order to avoid duplicates
                pnames.Add(pname);
            } // for
            sb.Append(")");
            res.CommandText = sb.ToString();
            res.CommandType = CommandType.Text;
            return res;
        }

        /// <summary>
        /// Used in order to avoid breaking naming rules (e.g., when a table has
        /// a name in SQL Server that cannot be used as a basis for a matching index
        /// name in SQLite).
        /// </summary>
        /// <param name="str">The name to change if necessary</param>
        /// <param name="names">Used to avoid duplicate names</param>
        /// <returns>A normalized name</returns>
        private static string GetNormalizedName(string str, List<string> names)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                if (Char.IsLetterOrDigit(str[i]) || str[i] == '_')
                    sb.Append(str[i]);
                else
                    sb.Append("_");
            } // for

            // Avoid returning duplicate name
            if (names.Contains(sb.ToString()))
                return GetNormalizedName(sb.ToString() + "_", names);
            else
                return sb.ToString();
        }

        /// <summary>
        /// Matches SQL Server types to general DB types
        /// </summary>
        /// <param name="cs">The column schema to use for the match</param>
        /// <returns>The matched DB type</returns>
        private static SqlDbType GetDbTypeOfColumn(ColumnSchema cs)
        {
            if (cs.ColumnType == "tinyint")
                return SqlDbType.TinyInt;
            if (cs.ColumnType == "int")
                return SqlDbType.Int;
            if (cs.ColumnType == "smallint")
                return SqlDbType.SmallInt;
            if (cs.ColumnType == "bigint")
                return SqlDbType.BigInt;
            if (cs.ColumnType == "bit")
                return SqlDbType.Bit;
            if (cs.ColumnType == "nvarchar" || cs.ColumnType == "varchar" ||
                cs.ColumnType == "text" || cs.ColumnType == "ntext")
                return SqlDbType.Text;
            if (cs.ColumnType == "float")
                return SqlDbType.Float;
            if (cs.ColumnType == "real")
                return SqlDbType.Real;
            if (cs.ColumnType == "blob" || cs.ColumnType == "varbinary")
                return SqlDbType.Binary;
            if (cs.ColumnType == "numeric")
                return SqlDbType.Float;
            if (cs.ColumnType == "timestamp" || cs.ColumnType == "datetime" || cs.ColumnType == "datetime2" || cs.ColumnType == "date" || cs.ColumnType == "time")
                return SqlDbType.DateTime;
            if (cs.ColumnType == "nchar" || cs.ColumnType == "char")
                return SqlDbType.Text;
            if (cs.ColumnType == "uniqueidentifier" || cs.ColumnType == "guid" || cs.ColumnType == "uuid" || cs.ColumnType == "uuidtext")
                return SqlDbType.UniqueIdentifier ;
            if (cs.ColumnType == "xml")
                return SqlDbType.Xml;
            if (cs.ColumnType == "sql_variant")
                return SqlDbType.Variant;
            if (cs.ColumnType == "integer")
                return SqlDbType.Int;

            _log.Error("illegal db type found");
            throw new ApplicationException("Illegal DB type found (" + cs.ColumnType + ")");
        }

        /// <summary>
        /// Builds a SELECT query for a specific table. Needed in the process of copying rows
        /// from the SQL Server database to the SQLite database.
        /// </summary>
        /// <param name="ts">The table schema of the table for which we need the query.</param>
        /// <returns>The SELECT query for the table.</returns>
        private static string BuildSQLiteTableQuery(TableSchema ts)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            for (int i = 0; i < ts.Columns.Count; i++)
            {
                sb.Append("[" + ts.Columns[i].ColumnName + "]");
                if (i < ts.Columns.Count - 1)
                    sb.Append(", ");
            } // for
            sb.Append(" FROM [" + ts.TableName + "]");
            return sb.ToString();
        }

        /// <summary>
        /// Creates the SQLite database from the schema read from the SQL Server.
        /// </summary>
        /// <param name="sqlitePath">The path to the generated DB file.</param>
        /// <param name="schema">The schema of the SQL server database.</param>
        /// <param name="password">The password to use for encrypting the DB or null if non is needed.</param>
        /// <param name="handler">A handle for progress notifications.</param>
        private static void CreateSqlServerDatabase(string sqlConnString, DatabaseSchema schema, 
            SqlConversionHandler handler,
            FailedViewDefinitionHandler viewFailureHandler)
        {
            _log.Debug("Creating SQL Server database...");

            // Connect to the newly created database
            using (SqlConnection SqlServerConn = new SqlConnection(sqlConnString))
            {
                SqlServerConn.Open();

                // Create all tables in the new database
                int count = 0;
                foreach (TableSchema dt in schema.Tables)
                {
                    try
                    {
                        AddSqlServerTable(SqlServerConn, dt);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("AddSqlServerTable failed", ex);
                        throw;
                    }
                    count++;
                    CheckCancelled();
                    handler(false, true, (int)(count * 50.0 / schema.Tables.Count), "Added table " + dt.TableName + " to the SQLite database");

                    _log.Debug("added schema for SQLite table [" + dt.TableName + "]");
                } // foreach
                SqlConnection.ClearPool(SqlServerConn);
            } // using

            _log.Debug("finished adding all table/view schemas for SQL Server database");
        }

        /// <summary>
        /// Creates the CREATE TABLE DDL for SQLite and a specific table.
        /// </summary>
        /// <param name="conn">The SQLite connection</param>
        /// <param name="dt">The table schema object for the table to be generated.</param>
        private static void AddSqlServerTable(SqlConnection SqlServerConn, TableSchema dt)
        {
            // Prepare a CREATE TABLE DDL statement
            string stmt = BuildCreateTableQuery(dt);

            _log.Info("\n\n" + stmt + "\n\n");

            // Execute the query in order to actually create the table.
            SqlCommand cmd = new SqlCommand(stmt, SqlServerConn);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// returns the CREATE TABLE DDL for creating the SQLite table from the specified
        /// table schema object.
        /// </summary>
        /// <param name="ts">The table schema object from which to create the SQL statement.</param>
        /// <returns>CREATE TABLE DDL for the specified table.</returns>
        private static string BuildCreateTableQuery(TableSchema ts)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("CREATE TABLE [" + ts.TableName + "] (\n");

            bool pkey = false;
            for (int i = 0; i < ts.Columns.Count; i++)
            {
                ColumnSchema col = ts.Columns[i];
                string cline = BuildColumnStatement(col, ts, ref pkey);
                sb.Append(cline);
                if (i < ts.Columns.Count - 1)
                    sb.Append(",\n");
            } // foreach

            // add primary keys...
            if (ts.PrimaryKey != null && ts.PrimaryKey.Count > 0 & !pkey)
            {
                sb.Append(",\n");
                sb.Append("    PRIMARY KEY (");
                for (int i = 0; i < ts.PrimaryKey.Count; i++)
                {
                    sb.Append("[" + ts.PrimaryKey[i] + "]");
                    if (i < ts.PrimaryKey.Count - 1)
                        sb.Append(", ");
                } // for
                sb.Append(")\n");
            }
            else
                sb.Append("\n");

            sb.Append("\n");
            sb.Append(");\n");

            // Create any relevant indexes
            if (ts.Indexes != null)
            {
                for (int i = 0; i < ts.Indexes.Count; i++)
                {
                    string stmt = BuildCreateIndex(ts.TableName, ts.Indexes[i]);
                    sb.Append(stmt + ";\n");
                } // for
            } // if

            string query = sb.ToString();
            return query;
        }

        /// <summary>
        /// Creates a CREATE INDEX DDL for the specified table and index schema.
        /// </summary>
        /// <param name="tableName">The name of the indexed table.</param>
        /// <param name="indexSchema">The schema of the index object</param>
        /// <returns>A CREATE INDEX DDL (SQLite format).</returns>
        private static string BuildCreateIndex(string tableName, IndexSchema indexSchema)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE ");
            if (indexSchema.IsUnique)
                sb.Append("UNIQUE ");
            sb.Append("INDEX [" + tableName + "_" + indexSchema.IndexName + "]\n");
            sb.Append("ON [" + tableName + "]\n");
            sb.Append("(");
            for (int i = 0; i < indexSchema.Columns.Count; i++)
            {
                sb.Append("[" + indexSchema.Columns[i].ColumnName + "]");
                if (!indexSchema.Columns[i].IsAscending)
                    sb.Append(" DESC");
                if (i < indexSchema.Columns.Count - 1)
                    sb.Append(", ");
            } // for
            sb.Append(")");

            return sb.ToString();
        }

        /// <summary>
        /// Used when creating the CREATE TABLE DDL. Creates a single row
        /// for the specified column.
        /// </summary>
        /// <param name="col">The column schema</param>
        /// <returns>A single column line to be inserted into the general CREATE TABLE DDL statement</returns>
        private static string BuildColumnStatement(ColumnSchema col, TableSchema ts, ref bool pkey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\t[" + col.ColumnName + "]\t");

            // Special treatment for IDENTITY columns
            if (col.IsIdentity)
            {
                if (ts.PrimaryKey.Count == 1 && (col.ColumnType == "tinyint" || col.ColumnType == "int" || col.ColumnType == "smallint" ||
                    col.ColumnType == "bigint" || col.ColumnType == "integer"))
                {
                    sb.Append("integer PRIMARY KEY AUTOINCREMENT");
                    pkey = true;
                }
                else
                    sb.Append("integer");
            }
            else
            {
                if (col.ColumnType == "int")
                    sb.Append("integer");
                else
                {
                    sb.Append(col.ColumnType);
                }
                if (col.Length == -1)
                    sb.Append("(max)");
                else if (col.Length > 0)
                    sb.Append("(" + col.Length + ")");
            }
            if (!col.IsNullable)
                sb.Append(" NOT NULL");

            if (col.IsCaseSensitivite.HasValue && !col.IsCaseSensitivite.Value)
                sb.Append(" COLLATE NOCASE");

            string defval = StripParens(col.DefaultValue);
            defval = DiscardNational(defval);
            _log.Debug("DEFAULT VALUE BEFORE [" + col.DefaultValue + "] AFTER [" + defval + "]");
            if (defval != string.Empty && defval.ToUpper().Contains("GETDATE"))
            {
                _log.Debug("converted SQL Server GETDATE() to CURRENT_TIMESTAMP for column [" + col.ColumnName + "]");
                sb.Append(" DEFAULT (CURRENT_TIMESTAMP)");
            }
            else if (defval != string.Empty && IsValidDefaultValue(defval))
                sb.Append(" DEFAULT " + defval);

            return sb.ToString();
        }

        private static void CreateSqlServerForeignKeys(string sqlConnString, DatabaseSchema schema,
            SqlConversionHandler handler,
            FailedViewDefinitionHandler viewFailureHandler)
        {
            _log.Debug("Creating SQL Server foreign keys...");

            // Connect to the newly created database
            using (SqlConnection SqlServerConn = new SqlConnection(sqlConnString))
            {
                SqlServerConn.Open();
                // Create all foreign keys
                int count = 0;
                foreach (TableSchema dt in schema.Tables)
                {
                    try
                    {
                        AddSqlServerForeignKeys(SqlServerConn, dt);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("AddSqlServerForeignKeys failed", ex);
                        throw;
                    }
                    count++;
                    CheckCancelled();
                    handler(false, true, (int)(count * 50.0 / schema.Tables.Count), "Added table " + dt.TableName + " to the SQLite database");

                    _log.Debug("added foreign keys for SQLite table [" + dt.TableName + "]");
                } // foreach
                SqlConnection.ClearPool(SqlServerConn);
            } // using

            _log.Debug("finished adding all table/view schemas for SQL Server database");
        }
        
        /// <summary>
        /// Creates the ALTER TABLE DDL for SQLite to add foreign keys. This is
        /// required because MS SQL Server requires that tables be created before
        /// foreign keys reference them.
        /// </summary>
        /// <param name="conn">The SQLite connection</param>
        /// <param name="dt">The table schema object for the table to be generated.</param>
        private static void AddSqlServerForeignKeys(SqlConnection SqlServerConn, TableSchema dt)
        {
            // Prepare a CREATE TABLE DDL statement
            string stmt = BuildCreateForeignKeyQuery(dt);

            _log.Info("\n\n" + stmt + "\n\n");

            // Execute the query in order to actually create the table.
            if (stmt != null)
            {
                SqlCommand cmd = new SqlCommand(stmt, SqlServerConn);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// returns the CREATE TABLE DDL for creating the SQLite table from the specified
        /// table schema object.
        /// </summary>
        /// <param name="ts">The table schema object from which to create the SQL statement.</param>
        /// <returns>CREATE TABLE DDL for the specified table.</returns>
        private static string BuildCreateForeignKeyQuery(TableSchema ts)
        {
            // add foreign keys...
            if (ts.ForeignKeys.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < ts.ForeignKeys.Count; i++)
                {
                    sb.Append("ALTER TABLE [" + ts.TableName + "]\n");
                    ForeignKeySchema foreignKey = ts.ForeignKeys[i];
                    string stmt = string.Format("    ADD CONSTRAINT [{0}] FOREIGN KEY ([{1}])\n        REFERENCES [{2}]([{3}])",
                                ts.TableName + "_" + foreignKey.ColumnName + "_" + foreignKey.ForeignTableName + "_" + foreignKey.ForeignColumnName,
                                foreignKey.ColumnName,
                                foreignKey.ForeignTableName,
                                foreignKey.ForeignColumnName);

                    sb.Append(stmt);
                    if (i < ts.ForeignKeys.Count - 1)
                        sb.Append("\n");
                } // for
                return sb.ToString();
            }
            else
                return null;
        }

        /// <summary>
        /// Discards the national prefix if exists (e.g., N'sometext') which is not
        /// supported in SQLite.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static string DiscardNational(string value)
        {
            Regex rx = new Regex(@"N\'([^\']*)\'");
            Match m = rx.Match(value);
            if (m.Success)
                return m.Groups[1].Value;
            else
                return value;
        }

        /// <summary>
        /// Check if the DEFAULT clause is valid by SQLite standards
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool IsValidDefaultValue(string value)
        {
            if (IsSingleQuoted(value))
                return true;

            double testnum;
            if (!double.TryParse(value, out testnum))
                return false;
            return true;
        }

        private static bool IsSingleQuoted(string value)
        {
            value = value.Trim();
            if (value.StartsWith("'") && value.EndsWith("'"))
                return true;
            return false;
        }

        /// <summary>
        /// Strip any parentheses from the string.
        /// </summary>
        /// <param name="value">The string to strip</param>
        /// <returns>The stripped string</returns>
        private static string StripParens(string value)
        {
            Regex rx = new Regex(@"\(([^\)]*)\)");
            Match m = rx.Match(value);
            if (!m.Success)
                return value;
            else
                return StripParens(m.Groups[1].Value);
        }

        /// <summary>
        /// Reads the entire SQLite DB schema using the specified connection string.
        /// </summary>
        /// <param name="connString">The connection string used for reading SQL Server schema.</param>
        /// <param name="handler">A handler for progress notifications.</param>
        /// <param name="selectionHandler">The selection handler which allows the user to select 
        /// which tables to convert.</param>
        /// <returns>database schema objects for every table/view in the SQL Server database.</returns>
        private static DatabaseSchema ReadSQLiteSchema(string sqlitePath, string password, SqlConversionHandler handler,
            SqlTableSelectionHandler selectionHandler)
        {
            // First step is to read the names of all tables in the database
            List<TableSchema> tables = new List<TableSchema>();
            string SQLiteConnString = CreateSQLiteConnectionString(sqlitePath, password);
            using (SQLiteConnection SQLiteConn = new SQLiteConnection(SQLiteConnString))
            {
                SQLiteConn.Open();

                List<string> tableNames = new List<string>();

                // This command will read the names of all tables in the database
                // Tables whose name begins 'sqlite_' are internal tables and should be ignored.
                SQLiteCommand cmd = new SQLiteCommand(@"select * from SQLITE_MASTER WHERE type = 'table' AND name NOT LIKE 'sqlite_%'", SQLiteConn);
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        tableNames.Add((string)reader["name"]);
                } // using

                // Next step is to query the schema of each table.
                int count = 0;
                for (int i = 0; i < tableNames.Count; i++)
                {
                    string tname = tableNames[i];
                    TableSchema ts = CreateTableSchema(SQLiteConn, tname);
                    CreateForeignKeySchema(SQLiteConn, ts);
                    tables.Add(ts);
                    count++;
                    CheckCancelled();
                    handler(false, true, (int)(count * 50.0 / tableNames.Count), "Parsed table " + tname);

                    _log.Debug("parsed table schema for [" + tname + "]");
                } // foreach
            } // using

            _log.Debug("finished parsing all tables in SQL Server schema");

            // Allow the user a chance to select which tables to convert
            if (selectionHandler != null)
            {
                List<TableSchema> updated = selectionHandler(tables);
                if (updated == null)
                    return null;
                else
                    tables = updated;
            } // if

            Regex removedbo = new Regex(@"dbo\.", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            DatabaseSchema ds = new DatabaseSchema();
            ds.Tables = tables;
            return ds;
        }

        /// <summary>
        /// Convenience method for checking if the conversion progress needs to be cancelled.
        /// </summary>
        private static void CheckCancelled()
        {
            if (_cancelled)
                throw new ApplicationException("User cancelled the conversion");
        }

        /// <summary>
        /// Creates a TableSchema object using the specified SQL Server connection
        /// and the name of the table for which we need to create the schema.
        /// </summary>
        /// <param name="conn">The SQL Server connection to use</param>
        /// <param name="tableName">The name of the table for which we wants to create the table schema.</param>
        /// <returns>A table schema object that represents our knowledge of the table schema</returns>
        private static TableSchema CreateTableSchema(SQLiteConnection SQLiteConn, string tableName)
        {
            TableSchema res = new TableSchema();
            res.TableName = tableName;
            res.Columns = new List<ColumnSchema>();
            ArrayList primaryKey = new ArrayList();
            SQLiteCommand cmd = new SQLiteCommand(@"PRAGMA table_info('" + tableName + "')", SQLiteConn);
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    object tmp = reader["name"];
                    if (tmp is DBNull)
                        continue;
                    string colName = (string)reader["name"];

                    tmp = reader["dflt_value"];
                    string colDefault;
                    if (tmp is DBNull)
                        colDefault = string.Empty;
                    else
                        colDefault = (string)tmp;

                    tmp = reader["notnull"];
                    bool isNullable = ((long)tmp == 0);
                    string fullDataType = (string)reader["type"];
                    string dataType;
                    int length;
                    Match match = Regex.Match(fullDataType, @"(.+)\(([0-9]+)\)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        dataType = match.Groups[1].Value.ToLower();
                        length = Convert.ToInt32(match.Groups[2].Value);
                    }
                    else
                    {
                        dataType = fullDataType.ToLower();
                        length = 0;
                    }

                    // Not sure how to calculate isIdentity field
                    bool isIdentity = false;

                    // These data types probably need some improvement
                    if (dataType == "int2" || dataType == "smallint")
                        dataType = "smallint";
                    else if (dataType == "int" || dataType == "integer" || dataType == "mediumint" || dataType == "int4")
                        dataType = "int";
                    else if (dataType == "int8" || dataType == "bigint")
                        dataType = "bigint";
                    else if (dataType == "char")
                        dataType = "char";
                    else if (dataType == "nchar")
                        dataType = "nchar";
                    else if (dataType == "varchar" || dataType == "tinytext" || dataType == "text" || dataType == "longtext" || dataType == "clob")
                        dataType = "varchar";
                    else if (dataType == "nvarchar")
                        dataType = "nvarchar";
                    else if (dataType == "blob")
                        dataType = "varbinary";
                    else if (dataType == "real" || dataType == "double" || dataType == "double precision"
                         || dataType == "float" || dataType == "numeric" || dataType == "decimal")
                        dataType = "float";
                    else if (dataType == "bit" || dataType == "boolean")
                    {
                        dataType = "bit";
                        {
                            if (colDefault == "('0')")
                                colDefault = "(False)";
                            else if (colDefault == "('1')")
                                colDefault = "(True)";
                        }
                    }
                    else if (dataType == "date")
                        dataType = "date";
                    else if (dataType == "datetime")
                        dataType = "datetime";
                    else if (dataType == "uniqueidentifier")
                        dataType = "uniqueidentifier";
                    else
                        throw new ApplicationException("Validation failed for data type [" + dataType + "]");

                    if ((dataType == "nchar" || dataType == "nvarchar" || dataType == "varchar" || dataType == "varbinary") && length == 0)
                        length = -1;    // Becomes MAX when column is created.

                    colDefault = FixDefaultValueString(colDefault);

                    ColumnSchema col = new ColumnSchema();
                    col.ColumnName = colName;
                    col.ColumnType = dataType;
                    col.Length = length;
                    col.IsNullable = isNullable;
                    col.IsIdentity = isIdentity;
                    col.DefaultValue = AdjustDefaultValue(colDefault);
                    res.Columns.Add(col);

                    if (reader["pk"] != DBNull.Value)
                    {
                        int pkIndex = Convert.ToInt16(reader["pk"]);
                        if (pkIndex > 0)
                        primaryKey.Insert(pkIndex - 1, colName);
                    }

                } // while
            } // using

            // Find PRIMARY KEY information
            res.PrimaryKey = new List<string>();
            for (int i = 0; i < primaryKey.Count; i++)
                res.PrimaryKey.Add((string)primaryKey[i]);

            try
            {
                // Find index information
                SQLiteCommand cmd3 = new SQLiteCommand(@"PRAGMA index_list ('" + tableName + "')", SQLiteConn);
                using (SQLiteDataReader reader = cmd3.ExecuteReader())
                {
                    res.Indexes = new List<IndexSchema>();
                    while (reader.Read())
                    {
                        string indexName = (string)reader["name"];
                        bool unique = ((long)reader["unique"] == 1);
                        bool primary = ((string)reader["origin"] == "pk");
                        IndexSchema index = BuildIndexSchema(indexName, unique, primary, SQLiteConn);
                        res.Indexes.Add(index);
                    } // while
                } // using
            }
            catch (Exception ex)        
            {
                _log.Warn("failed to read index information for table [" + tableName + "]");
            } // catch

            return res;
        }

        //  This is a naive attempt to do the referse of the equivalent
        //  function in SqlServerToSQLite
        private static string FixDefaultValueString(string colDefault)
        {
            if (colDefault == null || colDefault == string.Empty)
                return colDefault;
            else
                return "'" + colDefault + "'";
        }

        /// <summary>
        /// Add foreign key schema object from the specified components (Read from SQL Server).
        /// </summary>
        /// <param name="conn">The SQL Server connection to use</param>
        /// <param name="ts">The table schema to whom foreign key schema should be added to</param>
        private static void CreateForeignKeySchema(SQLiteConnection SQLiteConn, TableSchema ts)
        {
            ts.ForeignKeys = new List<ForeignKeySchema>();

            SQLiteCommand cmd = new SQLiteCommand(@"PRAGMA foreign_key_list('" + ts.TableName + "')", SQLiteConn);

            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    ForeignKeySchema fkc = new ForeignKeySchema();
                    fkc.ColumnName = (string)reader["from"];
                    fkc.ForeignTableName = (string)reader["table"];
                    fkc.ForeignColumnName = (string)reader["to"];
                    fkc.CascadeOnDelete = (string)reader["on_delete"] == "CASCADE";
                    fkc.CascadeOnUpdate = (string)reader["on_update"] == "CASCADE";
                    fkc.TableName = ts.TableName;
                    ts.ForeignKeys.Add(fkc);
                }
            }
        }

        /// <summary>
        /// Builds an index schema object from the specified components (Read from SQL Server).
        /// </summary>
        /// <param name="indexName">The name of the index</param>
        /// <param name="desc">The description of the index</param>
        /// <param name="keys">Key columns that are part of the index.</param>
        /// <returns>An index schema object that represents our knowledge of the index</returns>
        private static IndexSchema BuildIndexSchema(string indexName, bool unique, bool primary, SQLiteConnection SQLiteConn)
        {
            IndexSchema res = new IndexSchema();
            res.IndexName = indexName;
            res.IsUnique = unique;

            SQLiteCommand cmd = new SQLiteCommand(@"PRAGMA index_info('" + indexName + "')", SQLiteConn);
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                res.Columns = new List<IndexColumn>();
                while (reader.Read())
                {
                    IndexColumn ic = new IndexColumn();
                    ic.ColumnName = (string)reader["name"];
                    ic.IsAscending = true;   //  JS: TBC, also collation order.
                    res.Columns.Add(ic);
                }
            }
            return res;
        }

        /// <summary>
        /// More adjustments for the DEFAULT value clause.
        /// </summary>
        /// <param name="val">The value to adjust</param>
        /// <returns>Adjusted DEFAULT value string</returns>
        private static string AdjustDefaultValue(string val)
        {
            if (val == null || val == string.Empty)
                return val;

            Match m = _defaultValueRx.Match(val);
            if (m.Success)
                return m.Groups[1].Value;
            return val;
        }

        /// <summary>
        /// Creates SQLite connection string from the specified DB file path.
        /// </summary>
        /// <param name="sqlitePath">The path to the SQLite database file.</param>
        /// <returns>SQLite connection string</returns>
        private static string CreateSQLiteConnectionString(string sqlitePath, string password)
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = sqlitePath;
            if (password != null)
                builder.Password = password;
            builder.PageSize = 4096;
            builder.UseUTF16Encoding = true;
            string connstring = builder.ConnectionString;

            return connstring;
        }
        #endregion

        #region Private Variables
        private static bool _isActive = false;
        private static bool _cancelled = false;
        private static Regex _keyRx = new Regex(@"(([a-zA-Z_äöüÄÖÜß0-9\.]|(\s+))+)(\(\-\))?");
        private static Regex _defaultValueRx = new Regex(@"\(N(\'.*\')\)");
        private static ILog _log = LogManager.GetLogger(typeof(SQLiteToSqlServer));
        #endregion
    }

}
