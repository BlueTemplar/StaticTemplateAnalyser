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
        private readonly IDocumentTemplateAnalyser _analyser;
        private readonly IAnalysisDataParser _analysisDataParser;

        public Form1()
            : this(new DatabaseHelper(), new DocumentTemplateAnalyser(), new AnalysisDataParser())
        {
        }

        public Form1(IDatabaseHelper databaseHelper, IDocumentTemplateAnalyser analyser, IAnalysisDataParser analysisDataParser)
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

                StartAnalysis();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An exception occurred\r\n" + ex.Message, "Something went wrong", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveCsv(Dictionary<TemplateModel, List<FieldCodeSummaryModel>> analysisData)
        {
            var dlg = CreateSaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                WriteCsv(analysisData, dlg);
            }
            ReEnableUi();
        }

        private SaveFileDialog CreateSaveFileDialog()
        {
            return
                new SaveFileDialog
                {
                    AddExtension = true,
                    OverwritePrompt = true,
                    FileName = "static-analysis.csv",
                    DefaultExt = "csv",
                    Filter = "CSV Files | *.csv",
                    SupportMultiDottedExtensions = false,
                    Title = "Save static analysis?"
                };
        }

        private void ReEnableUi()
        {
            AnalyseButton.Enabled = true;
            ProgressBar.Visible = false;
        }

        private void WriteCsv(Dictionary<TemplateModel, List<FieldCodeSummaryModel>> analysisData, SaveFileDialog dlg)
        {
            File.WriteAllText(dlg.FileName, _analysisDataParser.ToCsv(analysisData));
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

        private void StartAnalysis()
        {
            LockDownUi();
            var connectionString = _dbHelper.ConnectionString(SqlCredentials(), DatabaseNameComboBox.Text);
            
            _analyser.OnTemplateParsed += UpdateProgress;

            Dictionary<TemplateModel, List<FieldCodeSummaryModel>> analysisedData = null;

            BackgroundWorker.RunWorkerCompleted += (sender, args) => SaveCsv(analysisedData);
            BackgroundWorker.DoWork += (o, args) => analysisedData = _analyser.ProcessDocumentTemplates(connectionString); 
            BackgroundWorker.ProgressChanged += (sender, args) => ProgressBar.Value = args.ProgressPercentage;
            BackgroundWorker.RunWorkerAsync();
        }

        private void LockDownUi()
        {
            ProgressBar.Value = 0;
            AnalyseButton.Enabled = false;
            ProgressBar.Visible = true;
        }

        private void UpdateProgress(object sender, TemplateParsedEventArgs args)
        {
            BackgroundWorker.ReportProgress(100 * args.TemplateIndex / args.TemplateCount);
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
                    UserName = "IRISLaw",
                    Password = "1r15l4w09!",
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