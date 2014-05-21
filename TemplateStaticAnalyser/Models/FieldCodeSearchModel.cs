namespace TemplateStaticAnalyser.Models
{
    public class FieldCodeSearchModel
    {
        public FieldCodeSearchModel(string regex, string friendlyName)
        {
            Regex = regex;
            FriendlyName = friendlyName;
        }
        public string Regex { get; private set; }
        public string FriendlyName { get; private set; }
    }
}