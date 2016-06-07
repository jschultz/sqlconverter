using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using DbAccess;
using Microsoft.SqlServer;

namespace Converter
{
    public partial class MainForm : Form
    {
        #region Constructor
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handler
        private void btnBrowseSQLitePath_Click(object sender, EventArgs e)
        {
            DialogResult res = SQLiteFileDialog.ShowDialog(this);
            if (res == DialogResult.Cancel)
                return;

            string fpath = SQLiteFileDialog.FileName;
            txtSQLitePath.Text = fpath;
            pbrProgress.Value = 0;
            lblMessage.Text = string.Empty;
        }
        
        private void btnBrowseMSSQLPath_Click(object sender, EventArgs e)
        {
            DialogResult res = MSSqlFileDialog.ShowDialog(this);
            if (res == DialogResult.Cancel)
                return;

            string fpath = MSSqlFileDialog.FileName;
            cboDatabases.SelectedIndex = 0;
            txtMSSQLPath.Text = fpath;
            pbrProgress.Value = 0;
            lblMessage.Text = string.Empty;
        }

        private void txtMSSQLPath_TextChanged(object sender, EventArgs e)
        {
            if (txtMSSQLPath.Text != string.Empty && txtSQLitePath.Text == string.Empty)
                try
                {
                    txtSQLitePath.Text = Path.ChangeExtension(txtMSSQLPath.Text, ".db");
                }
                catch
                {
                };
            UpdateSensitivity();
        }

        private void cboDatabases_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();
            pbrProgress.Value = 0;
            lblMessage.Text = string.Empty;
        }

        private void cboInstances_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtSqlAddress.Text = cboInstances.SelectedText;
            btnSet_Click(sender, e);
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            try
            {
            	string constr;
            	if (cbxIntegrated.Checked) {
            		constr = GetSqlServerConnectionString(txtSqlAddress.Text, "master");
            	} else {
            		constr = GetSqlServerConnectionString(txtSqlAddress.Text, "master", txtUserDB.Text, txtPassDB.Text);
            	}
                using (SqlConnection conn = new SqlConnection(constr))
                {
                    conn.Open();

                    // Get the names of all DBs in the database server. Use filename to work out data file location.
                    SqlCommand query = new SqlCommand(@"select distinct [name], [filename] from sysdatabases", conn);
                    bool SqlConverterDatabaseExists = false;
                    using (SqlDataReader reader = query.ExecuteReader())
                    {
                        cboDatabases.Items.Clear();
                        cboDatabases.Items.Add((string)"<Use database file>");
                        while (reader.Read())
                        {
                            if ((string)reader[0] == "SqlConverter")
                                SqlConverterDatabaseExists = true;
                            else
                                cboDatabases.Items.Add((string)reader[0]);
                        }
                        if (cboDatabases.Items.Count > 0)
                            cboDatabases.SelectedIndex = 0;
                    } // using
                    if (SqlConverterDatabaseExists) { 
                        SqlCommand dropquery = new SqlCommand(@"DROP DATABASE SqlConverter", conn);
                        dropquery.ExecuteNonQuery();
                    }
                } // using

                cboDatabases.Enabled = true;

                pbrProgress.Value = 0;
                lblMessage.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    ex.Message,
                    "Failed To Connect",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            } // catch
            UpdateSensitivity();
        }

        private void txtSQLitePath_TextChanged(object sender, EventArgs e)
        {
            if (txtSQLitePath.Text != string.Empty && txtMSSQLPath.Text == string.Empty)
                try
                {
                    txtMSSQLPath.Text = Path.ChangeExtension(txtSQLitePath.Text, ".mdf");
                }
                catch
                {
                };
            UpdateSensitivity();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateSensitivity();

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Text = "SQL Server To SQLite DB Converter (" + version + ")";

            DataTable dt = System.Data.Sql.SqlDataSourceEnumerator.Instance.GetDataSources();
            cboInstances.Items.Clear();
            foreach (DataRow dr in dt.Rows)
            {
                string InstanceName = dr["InstanceName"].ToString();
                string ServerName   = dr["ServerName"].ToString();
                cboInstances.Items.Add(ServerName + '\\' + InstanceName);
            }
            if (cboInstances.Items.Count > 0)
            {
                txtSqlAddress.Text = (string)cboInstances.Items[0];
                cboInstances.SelectedIndex = 0;
                btnSet_Click(sender, e);
            }
        }

		private void txtSqlAddress_TextChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            SqlServerToSQLite.CancelConversion();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SqlServerToSQLite.IsActive)
            {
                SqlServerToSQLite.CancelConversion();
                _shouldExit = true;
                e.Cancel = true;
            }
            else
                e.Cancel = false;
        }

        private void cbxEncrypt_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();
        }

        private void ChkIntegratedCheckedChanged(object sender, EventArgs e)
        {
            if (cbxIntegrated.Checked)
            {
                lblPassword.Visible = false;
                lblUser.Visible = false;
                txtPassDB.Visible = false;
                txtUserDB.Visible = false;
            }
            else
            {
                lblPassword.Visible = true;
                lblUser.Visible = true;
                txtPassDB.Visible = true;
                txtUserDB.Visible = true;
            }
        }

        private void dropSqlConverterDatabase()
        {
            if (txtMSSQLPath.Text != string.Empty) {
                string constr;
                if (cbxIntegrated.Checked) {
                    constr = GetSqlServerConnectionString(txtSqlAddress.Text, "master");
                }
                else {
                    constr = GetSqlServerConnectionString(txtSqlAddress.Text, "master", txtUserDB.Text, txtPassDB.Text);
                }
                using (SqlConnection conn = new SqlConnection(constr)) {
                    conn.Open();
                    //  This command forces connections to close. I gave up trying to work out how to close the connection cleanly.
                    SqlCommand query = new SqlCommand(@"ALTER DATABASE SqlConverter SET SINGLE_USER WITH ROLLBACK IMMEDIATE", conn);
                    query.ExecuteNonQuery();
                    query = new SqlCommand(@"DROP DATABASE SqlConverter", conn);
                    //query = new SqlCommand(@"EXEC sp_detach_db SqlConverter", conn);
                    query.ExecuteNonQuery();
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
        	string sqlConnString;
            string dbname;

            if (txtMSSQLPath.Text != string.Empty) {
            	string constr;
            	if (cbxIntegrated.Checked) {
            		constr = GetSqlServerConnectionString(txtSqlAddress.Text, "master");
            	} else {
            		constr = GetSqlServerConnectionString(txtSqlAddress.Text, "master", txtUserDB.Text, txtPassDB.Text);
            	}
                string DataFile = Environment.GetEnvironmentVariable("windir") + "\\TEMP\\" + Path.GetFileName(txtMSSQLPath.Text);
//                string DataFile = Path.GetTempPath() + Path.GetFileName(txtMSSQLPath.Text);
                DirectoryInfo dir = new DirectoryInfo(Environment.GetEnvironmentVariable("windir") + "\\TEMP\\");
                foreach (var file in dir.GetFiles(Path.GetFileNameWithoutExtension(txtMSSQLPath.Text) + "*"))
                    file.Delete();

                System.IO.File.Copy(txtMSSQLPath.Text, DataFile, true);

                File.SetAttributes(DataFile, File.GetAttributes(DataFile) & ~FileAttributes.ReadOnly);

                using (SqlConnection conn = new SqlConnection(constr)) {
                    conn.Open();
                    SqlCommand query = new SqlCommand(@"CREATE DATABASE SqlConverter on (FILENAME=N'" + DataFile + "') FOR ATTACH", conn);
                    query.ExecuteNonQuery();
                    dbname = "SqlConverter";
                }            
            }
            else
                dbname = (string)cboDatabases.SelectedItem;

        	if (cbxIntegrated.Checked) {
        		sqlConnString = GetSqlServerConnectionString(txtSqlAddress.Text, dbname);
        	} else {
        		sqlConnString = GetSqlServerConnectionString(txtSqlAddress.Text, dbname, txtUserDB.Text, txtPassDB.Text);
        	}
            bool createViews = cbxCreateViews.Checked;

        	
            string sqlitePath = txtSQLitePath.Text.Trim();
            this.Cursor = Cursors.WaitCursor;
            SqlConversionHandler handler = new SqlConversionHandler(delegate(bool done,
                bool success, int percent, string msg) {
                    Invoke(new MethodInvoker(delegate() {
                        UpdateSensitivity();
                        lblMessage.Text = msg;
                        pbrProgress.Value = percent;

                        if (done)
                        {
                            btnStart.Enabled = true;
                            this.Cursor = Cursors.Default;
                            UpdateSensitivity();
                            dropSqlConverterDatabase();

                            if (success)
                            {
                                MessageBox.Show(this,
                                    msg,
                                    "Conversion Finished",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                                pbrProgress.Value = 0;
                                lblMessage.Text = string.Empty;
                            }
                            else
                            {
                                if (!_shouldExit)
                                {
                                    MessageBox.Show(this,
                                        msg,
                                        "Conversion Failed",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                                    pbrProgress.Value = 0;
                                    lblMessage.Text = string.Empty;
                                }
                                else
                                    Application.Exit();
                            }
                        }
                    }));
            });
            SqlTableSelectionHandler selectionHandler = new SqlTableSelectionHandler(delegate(List<TableSchema> schema)
            {
                List<TableSchema> updated = null;
                Invoke(new MethodInvoker(delegate
                {
                    // Allow the user to select which tables to include by showing him the 
                    // table selection dialog.
                    TableSelectionDialog dlg = new TableSelectionDialog();
                    DialogResult res = dlg.ShowTables(schema, this);
                    if (res == DialogResult.OK)
                        updated = dlg.IncludedTables;
                }));
                return updated;
            });

            FailedViewDefinitionHandler viewFailureHandler = new FailedViewDefinitionHandler(delegate(ViewSchema vs)
            {
                string updated = null;
                Invoke(new MethodInvoker(delegate
                {
                    ViewFailureDialog dlg = new ViewFailureDialog();
                    dlg.View = vs;
                    DialogResult res = dlg.ShowDialog(this);
                    if (res == DialogResult.OK)
                        updated = dlg.ViewSQL;
                    else
                        updated = null;
                }));

                return updated;
            });

            string password = txtPassword.Text.Trim();
            if (!cbxEncrypt.Checked)
                password = null;
            SqlServerToSQLite.ConvertSqlServerToSQLiteDatabase(sqlConnString, sqlitePath, password, handler, 
                selectionHandler, viewFailureHandler, cbxTriggers.Checked, createViews);
        }


        private void btnSQLiteSqlServer_Click(object sender, EventArgs e)
        {
            string sqlConnString;
            string dbname;

            if (txtMSSQLPath.Text != string.Empty)
            {
                string constr;
                if (cbxIntegrated.Checked)
                {
                    constr = GetSqlServerConnectionString(txtSqlAddress.Text, "master");
                }
                else
                {
                    constr = GetSqlServerConnectionString(txtSqlAddress.Text, "master", txtUserDB.Text, txtPassDB.Text);
                }
                string DataFile = Environment.GetEnvironmentVariable("windir") + "\\TEMP\\" + Path.GetFileName(txtMSSQLPath.Text);
                DirectoryInfo dir = new DirectoryInfo(Environment.GetEnvironmentVariable("windir") + "\\TEMP\\");
                foreach (var file in dir.GetFiles(Path.GetFileNameWithoutExtension(txtMSSQLPath.Text) + "*"))
                    file.Delete();

                using (SqlConnection conn = new SqlConnection(constr))
                {
                    conn.Open();
                    SqlCommand query = new SqlCommand(@"CREATE DATABASE SqlConverter on (NAME=N'" + Path.GetFileNameWithoutExtension(txtMSSQLPath.Text) + "',FILENAME=N'" + DataFile + "')", conn);
                    query.ExecuteNonQuery();
                    dbname = "SqlConverter";
                }
            }
            else
                dbname = (string)cboDatabases.SelectedItem;

            if (cbxIntegrated.Checked)
            {
                sqlConnString = GetSqlServerConnectionString(txtSqlAddress.Text, dbname);
            }
            else
            {
                sqlConnString = GetSqlServerConnectionString(txtSqlAddress.Text, dbname, txtUserDB.Text, txtPassDB.Text);
            }
            bool createViews = cbxCreateViews.Checked;


            string sqlitePath = txtSQLitePath.Text.Trim();
            this.Cursor = Cursors.WaitCursor;
            SqlConversionHandler handler = new SqlConversionHandler(delegate(bool done,
                bool success, int percent, string msg)
            {
                Invoke(new MethodInvoker(delegate()
                {
                    UpdateSensitivity();
                    lblMessage.Text = msg;
                    pbrProgress.Value = percent;

                    if (done)
                    {
                        btnSQLiteSqlServer.Enabled = true;
                        this.Cursor = Cursors.Default;
                        UpdateSensitivity();
                        dropSqlConverterDatabase();

                        if (success)
                        {
                            string DataFile = Environment.GetEnvironmentVariable("windir") + "\\TEMP\\" + Path.GetFileName(txtMSSQLPath.Text);
                            System.IO.File.Copy(DataFile, txtMSSQLPath.Text, true);
                            DirectoryInfo dir = new DirectoryInfo(Environment.GetEnvironmentVariable("windir") + "\\TEMP\\");
                            foreach (var file in dir.GetFiles(Path.GetFileNameWithoutExtension(txtMSSQLPath.Text) + "*"))
                                file.Delete();

                            MessageBox.Show(this,
                                msg,
                                "Conversion Finished",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            pbrProgress.Value = 0;
                            lblMessage.Text = string.Empty;
                        }
                        else
                        {
                            if (!_shouldExit)
                            {
                                MessageBox.Show(this,
                                    msg,
                                    "Conversion Failed",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                                pbrProgress.Value = 0;
                                lblMessage.Text = string.Empty;
                            }
                            else
                                Application.Exit();
                        }
                    }
                }));
            });
            SqlTableSelectionHandler selectionHandler = new SqlTableSelectionHandler(delegate(List<TableSchema> schema)
            {
                List<TableSchema> updated = null;
                Invoke(new MethodInvoker(delegate
                {
                    // Allow the user to select which tables to include by showing him the 
                    // table selection dialog.
                    TableSelectionDialog dlg = new TableSelectionDialog();
                    DialogResult res = dlg.ShowTables(schema, this);
                    if (res == DialogResult.OK)
                        updated = dlg.IncludedTables;
                }));
                return updated;
            });

            FailedViewDefinitionHandler viewFailureHandler = new FailedViewDefinitionHandler(delegate(ViewSchema vs)
            {
                string updated = null;
                Invoke(new MethodInvoker(delegate
                {
                    ViewFailureDialog dlg = new ViewFailureDialog();
                    dlg.View = vs;
                    DialogResult res = dlg.ShowDialog(this);
                    if (res == DialogResult.OK)
                        updated = dlg.ViewSQL;
                    else
                        updated = null;
                }));

                return updated;
            });

            string password = txtPassword.Text.Trim();
            if (!cbxEncrypt.Checked)
                password = null;
            SQLiteToSqlServer.ConvertSQLiteToSqlServerDatabase(sqlConnString, sqlitePath, password, handler,
                selectionHandler, viewFailureHandler, cbxTriggers.Checked, createViews);
        }

        #endregion

        #region Private Methods
        private void UpdateSensitivity()
        {
            btnStart.Enabled = txtSQLitePath.Text.Trim().Length > 0 
                            && cboDatabases.Enabled && (!cbxEncrypt.Checked || txtPassword.Text.Trim().Length > 0)
                            && (cboDatabases.SelectedIndex == 0 || txtMSSQLPath.Text.Trim().Length > 0)
                            && !SqlServerToSQLite.IsActive && !SQLiteToSqlServer.IsActive;
            btnSQLiteSqlServer.Enabled = txtSQLitePath.Text.Trim().Length > 0
                            && cboDatabases.Enabled && (!cbxEncrypt.Checked || txtPassword.Text.Trim().Length > 0)
                            && cboDatabases.SelectedIndex == 0 && txtMSSQLPath.Text.Trim().Length > 0 
                            && !SqlServerToSQLite.IsActive && !SQLiteToSqlServer.IsActive;

            cboInstances.Enabled = cboInstances.Items.Count > 0 && !SqlServerToSQLite.IsActive && !SQLiteToSqlServer.IsActive;
            btnSet.Enabled = txtSqlAddress.Text.Trim().Length > 0 && !SqlServerToSQLite.IsActive && !SQLiteToSqlServer.IsActive;
            btnCancel.Visible = SqlServerToSQLite.IsActive;
            txtSqlAddress.Enabled = !SqlServerToSQLite.IsActive && !SQLiteToSqlServer.IsActive;
            txtSQLitePath.Enabled = !SqlServerToSQLite.IsActive && !SQLiteToSqlServer.IsActive;
            txtMSSQLPath.Enabled = (cboDatabases.SelectedIndex == 0) && !SqlServerToSQLite.IsActive && !SQLiteToSqlServer.IsActive;
            btnBrowseSQLitePath.Enabled = !SqlServerToSQLite.IsActive && !SQLiteToSqlServer.IsActive;
            btnBrowseMSSQLPath.Enabled = !SqlServerToSQLite.IsActive && !SQLiteToSqlServer.IsActive;
            cbxEncrypt.Enabled = !SqlServerToSQLite.IsActive && !SQLiteToSqlServer.IsActive;
            cboDatabases.Enabled = cboDatabases.Items.Count > 0 && !SqlServerToSQLite.IsActive && !SQLiteToSqlServer.IsActive;
            txtPassword.Enabled = cbxEncrypt.Checked && cbxEncrypt.Enabled;
            cbxIntegrated.Enabled = !SqlServerToSQLite.IsActive && !SQLiteToSqlServer.IsActive;
            cbxCreateViews.Enabled = !SqlServerToSQLite.IsActive && !SQLiteToSqlServer.IsActive;
            cbxTriggers.Enabled = !SqlServerToSQLite.IsActive && !SQLiteToSqlServer.IsActive;
            txtPassDB.Enabled = !SqlServerToSQLite.IsActive && !SQLiteToSqlServer.IsActive;
            txtUserDB.Enabled = !SqlServerToSQLite.IsActive && !SQLiteToSqlServer.IsActive;
        }

        private static string GetSqlServerConnectionString(string address, string db)
        {
            string res = @"Data Source=" + address.Trim() +
                    ";Initial Catalog="+db.Trim()+";Integrated Security=SSPI;";
            return res;
        }
        private static string GetSqlServerConnectionString(string address, string db, string user, string pass)
        {
            string res = @"Data Source=" + address.Trim() +
            	";Initial Catalog="+db.Trim()+";User ID=" + user.Trim() + ";Password=" + pass.Trim();
            return res;
        }
        #endregion

        #region Private Variables
        private bool _shouldExit = false;
        #endregion        

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}