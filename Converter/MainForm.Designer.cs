namespace Converter
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtSqlAddress = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSQLitePath = new System.Windows.Forms.TextBox();
            this.btnBrowseSQLitePath = new System.Windows.Forms.Button();
            this.btnSqlServerSQLite = new System.Windows.Forms.Button();
            this.SQLiteFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.SqlServerFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.label3 = new System.Windows.Forms.Label();
            this.cboDatabases = new System.Windows.Forms.ComboBox();
            this.btnSet = new System.Windows.Forms.Button();
            this.pbrProgress = new System.Windows.Forms.ProgressBar();
            this.lblMessage = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cbxEncrypt = new System.Windows.Forms.CheckBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.cbxIntegrated = new System.Windows.Forms.CheckBox();
            this.txtUserDB = new System.Windows.Forms.TextBox();
            this.txtPassDB = new System.Windows.Forms.TextBox();
            this.lblUser = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.cbxTriggers = new System.Windows.Forms.CheckBox();
            this.cbxCreateViews = new System.Windows.Forms.CheckBox();
            this.btnBrowseSqlServerPath = new System.Windows.Forms.Button();
            this.txtSqlServerPath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cboInstances = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnSQLiteSqlServer = new System.Windows.Forms.Button();
            this.cboWhatToCopy = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "SQL Server Address:";
            // 
            // txtSqlAddress
            // 
            this.txtSqlAddress.Location = new System.Drawing.Point(154, 12);
            this.txtSqlAddress.Name = "txtSqlAddress";
            this.txtSqlAddress.Size = new System.Drawing.Size(429, 20);
            this.txtSqlAddress.TabIndex = 1;
            this.txtSqlAddress.TextChanged += new System.EventHandler(this.txtSqlAddress_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 147);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(135, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "SQLite Database File Path:";
            // 
            // txtSQLitePath
            // 
            this.txtSQLitePath.Location = new System.Drawing.Point(154, 144);
            this.txtSQLitePath.Name = "txtSQLitePath";
            this.txtSQLitePath.Size = new System.Drawing.Size(429, 20);
            this.txtSQLitePath.TabIndex = 11;
            this.txtSQLitePath.TextChanged += new System.EventHandler(this.txtSQLitePath_TextChanged);
            // 
            // btnBrowseSQLitePath
            // 
            this.btnBrowseSQLitePath.Location = new System.Drawing.Point(589, 142);
            this.btnBrowseSQLitePath.Name = "btnBrowseSQLitePath";
            this.btnBrowseSQLitePath.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseSQLitePath.TabIndex = 12;
            this.btnBrowseSQLitePath.Text = "Browse...";
            this.btnBrowseSQLitePath.UseVisualStyleBackColor = true;
            this.btnBrowseSQLitePath.Click += new System.EventHandler(this.btnBrowseSQLitePath_Click);
            // 
            // btnSqlServerSQLite
            // 
            this.btnSqlServerSQLite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSqlServerSQLite.Location = new System.Drawing.Point(365, 363);
            this.btnSqlServerSQLite.Name = "btnSqlServerSQLite";
            this.btnSqlServerSQLite.Size = new System.Drawing.Size(198, 23);
            this.btnSqlServerSQLite.TabIndex = 17;
            this.btnSqlServerSQLite.Text = "SQL Server -> SQLite";
            this.btnSqlServerSQLite.UseVisualStyleBackColor = true;
            this.btnSqlServerSQLite.Click += new System.EventHandler(this.btnSqlServerSQLite_Click);
            // 
            // SQLiteFileDialog
            // 
            this.SQLiteFileDialog.CheckPathExists = false;
            this.SQLiteFileDialog.DefaultExt = "db";
            this.SQLiteFileDialog.Filter = "SQLite Files|*.db;*.sqlite|All Files|*.*";
            this.SQLiteFileDialog.RestoreDirectory = true;
            // 
            // SqlServerFileDialog
            // 
            this.SqlServerFileDialog.CheckPathExists = false;
            this.SqlServerFileDialog.DefaultExt = "mdf";
            this.SqlServerFileDialog.Filter = "SQL Server Files|*.mdf|All Files|*.*";
            this.SqlServerFileDialog.RestoreDirectory = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 94);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Select DB:";
            // 
            // cboDatabases
            // 
            this.cboDatabases.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDatabases.Enabled = false;
            this.cboDatabases.FormattingEnabled = true;
            this.cboDatabases.Location = new System.Drawing.Point(154, 91);
            this.cboDatabases.Name = "cboDatabases";
            this.cboDatabases.Size = new System.Drawing.Size(429, 21);
            this.cboDatabases.TabIndex = 4;
            this.cboDatabases.SelectedIndexChanged += new System.EventHandler(this.cboDatabases_SelectedIndexChanged);
            // 
            // btnSet
            // 
            this.btnSet.Location = new System.Drawing.Point(589, 10);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(75, 23);
            this.btnSet.TabIndex = 2;
            this.btnSet.Text = "Set";
            this.btnSet.UseVisualStyleBackColor = true;
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // pbrProgress
            // 
            this.pbrProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pbrProgress.Location = new System.Drawing.Point(12, 342);
            this.pbrProgress.Name = "pbrProgress";
            this.pbrProgress.Size = new System.Drawing.Size(652, 18);
            this.pbrProgress.TabIndex = 16;
            // 
            // lblMessage
            // 
            this.lblMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMessage.Location = new System.Drawing.Point(12, 324);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(529, 13);
            this.lblMessage.TabIndex = 15;
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.Location = new System.Drawing.Point(569, 363);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(95, 23);
            this.btnCancel.TabIndex = 18;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // cbxEncrypt
            // 
            this.cbxEncrypt.AutoSize = true;
            this.cbxEncrypt.Location = new System.Drawing.Point(12, 172);
            this.cbxEncrypt.Name = "cbxEncrypt";
            this.cbxEncrypt.Size = new System.Drawing.Size(127, 17);
            this.cbxEncrypt.TabIndex = 13;
            this.cbxEncrypt.Text = "Encryption password:";
            this.cbxEncrypt.UseVisualStyleBackColor = true;
            this.cbxEncrypt.CheckedChanged += new System.EventHandler(this.cbxEncrypt_CheckedChanged);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(154, 170);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(197, 20);
            this.txtPassword.TabIndex = 14;
            this.txtPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
            // 
            // cbxIntegrated
            // 
            this.cbxIntegrated.Checked = true;
            this.cbxIntegrated.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxIntegrated.Location = new System.Drawing.Point(12, 117);
            this.cbxIntegrated.Name = "cbxIntegrated";
            this.cbxIntegrated.Size = new System.Drawing.Size(130, 21);
            this.cbxIntegrated.TabIndex = 5;
            this.cbxIntegrated.Text = "Integrated security";
            this.cbxIntegrated.UseVisualStyleBackColor = true;
            this.cbxIntegrated.CheckedChanged += new System.EventHandler(this.ChkIntegratedCheckedChanged);
            // 
            // txtUserDB
            // 
            this.txtUserDB.Location = new System.Drawing.Point(193, 118);
            this.txtUserDB.Name = "txtUserDB";
            this.txtUserDB.Size = new System.Drawing.Size(100, 20);
            this.txtUserDB.TabIndex = 7;
            this.txtUserDB.Visible = false;
            // 
            // txtPassDB
            // 
            this.txtPassDB.Location = new System.Drawing.Point(358, 118);
            this.txtPassDB.Name = "txtPassDB";
            this.txtPassDB.PasswordChar = '*';
            this.txtPassDB.Size = new System.Drawing.Size(113, 20);
            this.txtPassDB.TabIndex = 9;
            this.txtPassDB.Visible = false;
            // 
            // lblUser
            // 
            this.lblUser.AutoSize = true;
            this.lblUser.Location = new System.Drawing.Point(155, 121);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(32, 13);
            this.lblUser.TabIndex = 6;
            this.lblUser.Text = "User:";
            this.lblUser.Visible = false;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(299, 121);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(56, 13);
            this.lblPassword.TabIndex = 8;
            this.lblPassword.Text = "Password:";
            this.lblPassword.Visible = false;
            // 
            // cbxTriggers
            // 
            this.cbxTriggers.AutoSize = true;
            this.cbxTriggers.Location = new System.Drawing.Point(12, 195);
            this.cbxTriggers.Name = "cbxTriggers";
            this.cbxTriggers.Size = new System.Drawing.Size(201, 17);
            this.cbxTriggers.TabIndex = 19;
            this.cbxTriggers.Text = "Create triggers enforcing foreign keys";
            this.cbxTriggers.UseVisualStyleBackColor = true;
            this.cbxTriggers.CheckedChanged += new System.EventHandler(this.cbxTriggers_CheckedChanged);
            // 
            // cbxCreateViews
            // 
            this.cbxCreateViews.AutoSize = true;
            this.cbxCreateViews.Location = new System.Drawing.Point(225, 196);
            this.cbxCreateViews.Name = "cbxCreateViews";
            this.cbxCreateViews.Size = new System.Drawing.Size(249, 17);
            this.cbxCreateViews.TabIndex = 20;
            this.cbxCreateViews.Text = "Try to create views (works only in simple cases)";
            this.cbxCreateViews.UseVisualStyleBackColor = true;
            this.cbxCreateViews.CheckedChanged += new System.EventHandler(this.cbxCreateViews_CheckedChanged);
            // 
            // btnBrowseSqlServerPath
            // 
            this.btnBrowseSqlServerPath.Location = new System.Drawing.Point(589, 63);
            this.btnBrowseSqlServerPath.Name = "btnBrowseSqlServerPath";
            this.btnBrowseSqlServerPath.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseSqlServerPath.TabIndex = 23;
            this.btnBrowseSqlServerPath.Text = "Browse...";
            this.btnBrowseSqlServerPath.UseVisualStyleBackColor = true;
            this.btnBrowseSqlServerPath.Click += new System.EventHandler(this.btnBrowseSqlServerPath_Click);
            // 
            // txtSqlServerPath
            // 
            this.txtSqlServerPath.Location = new System.Drawing.Point(154, 65);
            this.txtSqlServerPath.Name = "txtSqlServerPath";
            this.txtSqlServerPath.Size = new System.Drawing.Size(429, 20);
            this.txtSqlServerPath.TabIndex = 22;
            this.txtSqlServerPath.TextChanged += new System.EventHandler(this.txtSqlServerPath_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(133, 13);
            this.label4.TabIndex = 21;
            this.label4.Text = "SQL Server Database File:";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // cboInstances
            // 
            this.cboInstances.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInstances.Enabled = false;
            this.cboInstances.FormattingEnabled = true;
            this.cboInstances.Location = new System.Drawing.Point(154, 38);
            this.cboInstances.Name = "cboInstances";
            this.cboInstances.Size = new System.Drawing.Size(429, 21);
            this.cboInstances.TabIndex = 25;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 41);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(118, 13);
            this.label5.TabIndex = 24;
            this.label5.Text = "Select Server Instance:";
            // 
            // btnSQLiteSqlServer
            // 
            this.btnSQLiteSqlServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSQLiteSqlServer.Location = new System.Drawing.Point(161, 363);
            this.btnSQLiteSqlServer.Name = "btnSQLiteSqlServer";
            this.btnSQLiteSqlServer.Size = new System.Drawing.Size(198, 23);
            this.btnSQLiteSqlServer.TabIndex = 26;
            this.btnSQLiteSqlServer.Text = "SQLite -> SQL Server";
            this.btnSQLiteSqlServer.UseVisualStyleBackColor = true;
            this.btnSQLiteSqlServer.Click += new System.EventHandler(this.btnSQLiteSqlServer_Click);
            // 
            // cboWhatToCopy
            // 
            this.cboWhatToCopy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboWhatToCopy.FormattingEnabled = true;
            this.cboWhatToCopy.Items.AddRange(new object[] {
            "Database structure and data",
            "Database structure only",
            "Data into existing database"});
            this.cboWhatToCopy.Location = new System.Drawing.Point(154, 218);
            this.cboWhatToCopy.Name = "cboWhatToCopy";
            this.cboWhatToCopy.Size = new System.Drawing.Size(429, 21);
            this.cboWhatToCopy.TabIndex = 28;
            this.cboWhatToCopy.SelectedIndexChanged += new System.EventHandler(this.cboWhatToCopy_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 221);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 13);
            this.label6.TabIndex = 27;
            this.label6.Text = "Copy:";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(676, 389);
            this.Controls.Add(this.cboWhatToCopy);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnSQLiteSqlServer);
            this.Controls.Add(this.cboInstances);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnBrowseSqlServerPath);
            this.Controls.Add(this.txtSqlServerPath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbxCreateViews);
            this.Controls.Add(this.cbxTriggers);
            this.Controls.Add(this.txtPassDB);
            this.Controls.Add(this.txtUserDB);
            this.Controls.Add(this.cbxIntegrated);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.cbxEncrypt);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.pbrProgress);
            this.Controls.Add(this.btnSet);
            this.Controls.Add(this.cboDatabases);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.lblUser);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnSqlServerSQLite);
            this.Controls.Add(this.btnBrowseSQLitePath);
            this.Controls.Add(this.txtSQLitePath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtSqlAddress);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "SQL Server To SQLite DB Converter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.TextBox txtPassDB;
        private System.Windows.Forms.TextBox txtUserDB;
        private System.Windows.Forms.CheckBox cbxIntegrated;

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSqlAddress;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSQLitePath;
        private System.Windows.Forms.Button btnBrowseSQLitePath;
        private System.Windows.Forms.Button btnSqlServerSQLite;
        private System.Windows.Forms.OpenFileDialog SQLiteFileDialog;
        private System.Windows.Forms.OpenFileDialog SqlServerFileDialog;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboDatabases;
        private System.Windows.Forms.Button btnSet;
        private System.Windows.Forms.ProgressBar pbrProgress;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox cbxEncrypt;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.CheckBox cbxTriggers;
        private System.Windows.Forms.CheckBox cbxCreateViews;
        private System.Windows.Forms.Button btnBrowseSqlServerPath;
        private System.Windows.Forms.TextBox txtSqlServerPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cboInstances;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnSQLiteSqlServer;
        private System.Windows.Forms.ComboBox cboWhatToCopy;
        private System.Windows.Forms.Label label6;
    }
}

