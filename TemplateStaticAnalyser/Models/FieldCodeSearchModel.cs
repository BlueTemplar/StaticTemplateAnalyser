namespace TemplateStaticAnalyser.Models
{
    public class FieldCodeSearchModel
    {
        public FieldCodeSearchModel(string fieldCode, string friendlyName)
        {
            FieldCode = fieldCode;
            FriendlyName = friendlyName;
        }
        public string FieldCode { get; private set; }
        public string FriendlyName { get; private set; }
    }
}