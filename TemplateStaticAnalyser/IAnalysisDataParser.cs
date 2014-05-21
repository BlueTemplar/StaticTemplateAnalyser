using System.Collections.Generic;
using TemplateStaticAnalyser.Models;

namespace TemplateStaticAnalyser
{
    public interface IAnalysisDataParser
    {
        string ToCsv(Dictionary<TemplateModel, List<FieldCodeSummaryModel>> analysisData);
    }
}