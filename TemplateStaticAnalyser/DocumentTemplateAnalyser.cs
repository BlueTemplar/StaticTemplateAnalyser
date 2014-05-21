﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Office.Interop.Word;
using TemplateStaticAnalyser.Models;

namespace TemplateStaticAnalyser
{
    public class DocumentTemplateAnalyser : IDocumentTemplateAnalyser
    {
        public event EventHandler<TemplateParsedEventArgs> OnTemplateParsed;

        private readonly FieldCodeSearchModel[] _documentCodes =
        {
            new FieldCodeSearchModel("<FC", "Field Code"),
            new FieldCodeSearchModel("<FC>Name Block", "Name Block"),
            new FieldCodeSearchModel("<RQ", "Required Question (RQ)"),
            new FieldCodeSearchModel("<RS", "Required Sentence (RS)"),
            new FieldCodeSearchModel("<YR", "Your Reference (YR)"),
            new FieldCodeSearchModel("<SA", "Start Address (SA)"),
            new FieldCodeSearchModel("<OP", "Optional Paragraph (OP)"),
            new FieldCodeSearchModel("<RD", "Required Date (RD)"),
            new FieldCodeSearchModel("<TT", "Time Type (TT)"),
            new FieldCodeSearchModel("<TU", "Time Units (TU)"),
            new FieldCodeSearchModel("<DR", "Document Reference (DR)"),
            new FieldCodeSearchModel("<HD", "Stationery Template (HD)"),
            new FieldCodeSearchModel("<HQ", "Stationery Template (HQ)"),
            new FieldCodeSearchModel("<PH", "Imported Paragraph (PH)")
        };

        private Application Initialise()
        {
            return new Application {Visible = false};
        }

        private void Cleanup(Application wordApplication)
        {
            wordApplication.Quit();
        }

        public Dictionary<TemplateModel, List<FieldCodeSummaryModel>> ProcessDocumentTemplates(string connectionString)
        {
            Application wordApplication = this.Initialise();

            var analysisData = new Dictionary<TemplateModel, List<FieldCodeSummaryModel>>();
            try
            {
                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();

                    var noOfTemplates = GetNoOfTemplates(sqlConnection);
                    var reader = GetData(sqlConnection);

                    if (reader.HasRows)
                    {
                        var threads = new List<Thread>();
                        while (reader.Read())
                        {
                            var templateName = reader.GetString(4);
                            var docContent = (byte[])reader["DocContent"];

                            threads.Add(new Thread(() => ParseTemplate(analysisData, wordApplication, noOfTemplates, templateName, docContent)));
                        }

                        const int threadCount = 8;

                        for (var j = 0; j < Math.Ceiling((double)threads.Count / threadCount); j++)
                        {
                            var threadsToKickOff = new List<Thread>();
                            for (var k = 0; k < threadCount; k++)
                            {
                                var theIndexWeWant = (j * threadCount) + k;

                                Debug.WriteLine("index is {0}", theIndexWeWant);
                                var aThread = threads.ElementAtOrDefault(theIndexWeWant);
                                if (aThread != null)
                                {
                                    threadsToKickOff.Add(aThread);
                                }
                            }

                            threadsToKickOff.ForEach(t => t.Start());
                            threadsToKickOff.ForEach(t => t.Join());
                        }
                    }

                    return analysisData;
                }
            }
            finally
            {
                this.Cleanup(wordApplication);
            }
        }

        private void ParseTemplate(Dictionary<TemplateModel, List<FieldCodeSummaryModel>> analysisData, Application wordApplication, int noOfTemplates, string templateName, byte[] docContent)
        {
            analysisData.Add(
                new TemplateModel {Name = templateName},
                this.ParseDocumentTemplate(wordApplication, docContent));

            if (OnTemplateParsed != null)
            {
                OnTemplateParsed(this, new TemplateParsedEventArgs(noOfTemplates));
            }
        }

        private static SqlDataReader GetData(SqlConnection sqlConnection)
        {
            var sqlCommand = new SqlCommand("SELECT top 100 * FROM dbo.Docs WHERE [TemplateId] IS NOT NULL",
                sqlConnection);

            var reader = sqlCommand.ExecuteReader();
            return reader;
        }

        private int GetNoOfTemplates(SqlConnection sqlConnection)
        {
            var sqlCommand2 = new SqlCommand("SELECT COUNT(*) FROM dbo.Docs WHERE [TemplateId] IS NOT NULL", sqlConnection);
            return (int)sqlCommand2.ExecuteScalar();
        }

        private string WriteTemplateToDisk(byte[] documentTemplate)
        {
            var fileName = Path.GetTempFileName();

            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                fs.Write(documentTemplate, 0, documentTemplate.Length);
            }

            return fileName;
        }

        private List<FieldCodeSummaryModel> ParseDocumentTemplate(Application wordApplication, byte[] documentTemplate)
        {
            string fileName = null;
            var analysisData = new List<FieldCodeSummaryModel>();
            Document document = null;
            try
            {
                fileName = this.WriteTemplateToDisk(documentTemplate);
                document = wordApplication.Documents.Open(fileName);

                foreach (var code in this._documentCodes)
                {
                    ParseFieldCode(document, code, analysisData);
                }
                return analysisData;
            }
            finally
            {
                if (document != null)
                {
                    document.Close();
                }
                if (fileName != null && File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }

        private static void ParseFieldCode(Document document, FieldCodeSearchModel code,
            ICollection<FieldCodeSummaryModel> analysisData)
        {
            var matches = Regex.Matches(document.Range().Text, code.FieldCode);

            analysisData.Add(
                new FieldCodeSummaryModel
                {
                    FieldCode = code.FriendlyName,
                    Instances = matches.Count
                });
        }
    }

}