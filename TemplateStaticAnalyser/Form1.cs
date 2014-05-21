using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TemplateStaticAnalyser.Models;

namespace TemplateStaticAnalyser
{
    public partial class Form1 : Form
    {
        private readonly IDatabaseHelper _dbHelper;
        private readonly IAnalyser _analyser;
        private readonly IAnalysisDataParser _analysisDataParser;

        public Form1()
            : this(new DatabaseHelper(), new Analyser(), new AnalysisDataParser())
        {
        }

        public Form1(IDatabaseHelper databaseHelper, IAnalyser analyser, IAnalysisDataParser analysisDataParser)
        {
            _dbHelper = databaseHelper;
            _analyser = analyser;
            _analysisDataParser = analysisDataParser;
            InitializeComponent();
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
                var analysisData = StartAnalysis();
                SaveCsv(analysisData);
                AnalysisFinished();
            }
            catch (Exception ex)
            {
                throw; //tbd...
            }
        }

        private void SaveCsv(Dictionary<TemplateModel, List<FieldCodeSummaryModel>> analysisData)
        {
            var dlg = new SaveFileDialog
            {
                AddExtension = true,
                OverwritePrompt = true,
                FileName = "static-analysis.csv",
                DefaultExt = "csv",
                Filter = "CSV Files | *.csv",
                SupportMultiDottedExtensions = false,
                Title = "Save static analysis?"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(dlg.FileName, _analysisDataParser.ToCsv(analysisData));
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
            return !_dbHelper.ValidateLogin(SqlCredentials());
        }

        private void AnalysisFinished()
        {
            AnalyseButton.Enabled = true;
            ProgressBar.Visible = false;

            
        }

        private Dictionary<TemplateModel, List<FieldCodeSummaryModel>> StartAnalysis()
        {
            AnalyseButton.Enabled = false;
            ProgressBar.Visible = true;

            var connectionString = _dbHelper.ConnectionString(SqlCredentials());
            _analyser.OnTemplateParsed += UpdateProgress;
            var analysisedData = _analyser.Analyse(connectionString);
            _analyser.OnTemplateParsed -= UpdateProgress;
            return analysisedData;
        }

        private void UpdateProgress(object sender, TemplateParsedEventArgs args)
        {
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = args.TemplateCount;
            ProgressBar.Value = args.TemplateIndex;
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

            _dbHelper
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
                SafeExecute(() => _dbHelper.GetSqlServers().ForEach(i => DatabaseServerComboBox.Items.Add(i.ToString())));
            }
        }
    }
}