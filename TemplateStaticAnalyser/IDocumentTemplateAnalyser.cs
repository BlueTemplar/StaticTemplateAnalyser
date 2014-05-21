using System;
using System.Collections.Generic;
using TemplateStaticAnalyser.Models;

namespace TemplateStaticAnalyser
{
    public interface IDocumentTemplateAnalyser
    {
        event EventHandler<TemplateParsedEventArgs> OnTemplateParsed;

        Dictionary<TemplateModel, List<FieldCodeSummaryModel>> ProcessDocumentTemplates(string connectionString);
    }
}