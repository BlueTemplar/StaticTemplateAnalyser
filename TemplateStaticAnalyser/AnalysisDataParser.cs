using System.Collections.Generic;
using System.Linq;
using System.Text;
using TemplateStaticAnalyser.Models;

namespace TemplateStaticAnalyser
{
    class AnalysisDataParser : IAnalysisDataParser
    {
        public string ToCsv(Dictionary<TemplateModel, List<FieldCodeSummaryModel>> analysisData)
        {
            var sb = new StringBuilder();
            var distinctFieldCodes = GetDistinctFieldCodes(analysisData);

            CreateCsvHeader(sb, distinctFieldCodes);
            CreateCsvLine(analysisData, sb, distinctFieldCodes);
            return sb.ToString();
        }

        private static void CreateCsvLine(Dictionary<TemplateModel, List<FieldCodeSummaryModel>> analysisData, StringBuilder sb, string[] distinctFieldCodes)
        {
            foreach (var item in analysisData)
            {
                sb.Append(item.Key.Name);
                foreach (var fieldCode in distinctFieldCodes)
                {
                    var matchedFieldCode = item.Value.SingleOrDefault(v => v.FieldCode == fieldCode);
                    sb.AppendFormat(",{0}", matchedFieldCode == null ? 0 : matchedFieldCode.Instances);
                }
                NewLine(sb);
            }
        }

        private static void NewLine(StringBuilder sb)
        {
            sb.Append("\r\n");
        }

        private static void CreateCsvHeader(StringBuilder sb, string[] distinctFieldCodes)
        {
            sb.Append("Template name");
            foreach (var fieldCode in distinctFieldCodes)
            {
                sb.AppendFormat(",{0}", fieldCode);
            }
            NewLine(sb);
        }

        private string[] GetDistinctFieldCodes(Dictionary<TemplateModel, List<FieldCodeSummaryModel>> analysisData)
        {
            var distinctFieldCodes = new List<string>();
            foreach (var fieldCodeSummary in analysisData.SelectMany(i => i.Value.Where(fc => !distinctFieldCodes.Contains(fc.FieldCode))))
            {
                distinctFieldCodes.Add(fieldCodeSummary.FieldCode);
            }
            return distinctFieldCodes.OrderBy(fc => fc).ToArray();
        }
    }
}
