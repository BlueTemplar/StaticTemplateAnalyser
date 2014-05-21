using System;
using System.Collections.Generic;
using System.Threading;
using TemplateStaticAnalyser.Models;

namespace TemplateStaticAnalyser
{
    public class Analyser : IAnalyser
    {
        private int _numberOfTemplates;

        public event EventHandler<TemplateParsedEventArgs> OnTemplateParsed;

        public Dictionary<TemplateModel, List<FieldCodeSummaryModel>> Analyse(string connectionString)
        {
            //Go off and get number of templates from db
            SetTemplateCount();

            var analysisData = new Dictionary<TemplateModel, List<FieldCodeSummaryModel>>();
            FireTemplateParsedEvent(-1);
            for (int i = 0; i < _numberOfTemplates; i++)
            {
                analysisData.Add(new TemplateModel {Name = "Template is " + i.ToString("N")}, ParseTemplate(i));
                FireTemplateParsedEvent(i);
            }
            return analysisData;
        }

        private void FireTemplateParsedEvent(int i)
        {
            if (OnTemplateParsed != null)
            {
                OnTemplateParsed(this, new TemplateParsedEventArgs(i + 1, _numberOfTemplates));
            }
        }

        private List<FieldCodeSummaryModel> ParseTemplate(int i)
        {
            Thread.Sleep(250);
            var random = new Random();
            return new List<FieldCodeSummaryModel>
            {
                new FieldCodeSummaryModel{ FieldCode="FC", Instances = random.Next(25)},
                new FieldCodeSummaryModel{ FieldCode="SC", Instances = random.Next(25)},
                new FieldCodeSummaryModel{ FieldCode="DC", Instances = random.Next(25)},
                new FieldCodeSummaryModel{ FieldCode="AB", Instances = random.Next(25)},
                new FieldCodeSummaryModel{ FieldCode="PH", Instances = random.Next(25)}
            };
        }

        private void SetTemplateCount()
        {
            _numberOfTemplates = 10;
        }
    }
}
