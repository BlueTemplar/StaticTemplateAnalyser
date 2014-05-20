using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using TemplateStaticAnalyser.Models;

namespace TemplateStaticAnalyser
{
    public partial class Form1 : Form
    {
        private Thread _analyseThread;
        private readonly IDatabaseHelper _dbInstances;

        public Form1()
            : this(new DatabaseHelper())
        {
        }

        public Form1(IDatabaseHelper databaseHelper)
        {
            _dbInstances = databaseHelper;
            InitializeComponent();
            CancelButton.Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PopulateDatabasesForServer();
        }

        private void AnalyseButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (InValidateCredentials())
                {
                    ShowInvalidCredentialsMessage();
                    return;
                }
                StartAnalysis();
            }
            catch (Exception ex)
            {
                throw;  //tbd...
            }
            finally
            {
                AnalysisFinished();
            }
        }

        private void ShowInvalidCredentialsMessage()
        {
            MessageBox.Show(
                "The supplied database information is incorrect. Please ensure the server name, database name, username and password are correct.",
                "Invalid database connection",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private bool InValidateCredentials()
        {
            return !_dbInstances.ValidateLogin(SqlCredentials());
        }

        private void AnalysisFinished()
        {
            AnalyseButton.Enabled = true;
            CancelButton.Visible = false;
        }

        private void StartAnalysis()
        {
            AnalyseButton.Enabled = false;
            CancelButton.Visible = true;

            _analyseThread = new Thread(() => Thread.Sleep(5000));
            _analyseThread.Start();
            _analyseThread.Join();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (_analyseThread != null && _analyseThread.IsAlive)
            {
                _analyseThread.Abort();
            }
        }

        private void DatabaseServerComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateDatabasesForServer();
        }

        private void PopulateDatabasesForServer()
        {
            SafeExecute(GetDatabasesForServer);
        }

        private void GetDatabasesForServer()
        {
            Cursor = Cursors.WaitCursor;
            DatabaseNameComboBox.Text = string.Empty;
            DatabaseNameComboBox.Items.Clear();

            _dbInstances
                .GetServerDatabases(SqlCredentials())
                .ForEach(AddDatabaseNameToComboBox);
        }

        private void AddDatabaseNameToComboBox(string databaseName)
        {
            DatabaseNameComboBox.Items.Add(databaseName);
            PreSelectItem(databaseName);
        }

        private void PreSelectItem(string databaseName)
        {
            if (databaseName == "IrisLawBusiness")
            {
                DatabaseNameComboBox.SelectedIndex = DatabaseNameComboBox.Items.Count - 1;
            }
        }

        private SqlAuthConnectionModel SqlCredentials()
        {
            return
                new SqlAuthConnectionModel
                {
                    UserName = UserNameTextBox.Text,
                    Password = PasswordTextBox.Text,
                    ServerName = DatabaseServerComboBox.Text
                };
        }

        private void SafeExecute(Action actionToExecute)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                actionToExecute();
            }
            catch (Exception ex)
            {
                //ignore error, simply write to debugger.
                Debug.WriteLine(ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void DatabaseServerComboBox_Leave(object sender, EventArgs e)
        {
            PopulateDatabasesForServer();
        }

        private void DatabaseServerComboBox_DropDown(object sender, EventArgs e)
        {
            //If no known SQL server instances in the list then search the network
            if (DatabaseServerComboBox.Items.Count <= 1)
            {
                SafeExecute(() => _dbInstances.GetSqlServers().ForEach(i => DatabaseServerComboBox.Items.Add(i.ToString())));
            }
        }
    }
}