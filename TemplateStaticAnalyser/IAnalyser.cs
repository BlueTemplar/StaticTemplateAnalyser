using System;
using System.Collections.Generic;
using TemplateStaticAnalyser.Models;

namespace TemplateStaticAnalyser
{
    public interface IAnalyser
    {
        event EventHandler<TemplateParsedEventArgs> OnTemplateParsed;
        Dictionary<TemplateModel, List<FieldCodeSummaryModel>> Analyse(string connectionString);
    }
}