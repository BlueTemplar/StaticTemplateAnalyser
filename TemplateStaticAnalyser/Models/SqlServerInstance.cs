using System;

namespace TemplateStaticAnalyser.Models
{
    public class SqlServerInstance
    {
        public String InstanceName { get; set; }

        public String ServerName { get; set; }

        public String Version { get; set; }

        public override String ToString()
        {
            return String.IsNullOrEmpty(InstanceName)
                ? ServerName
                : String.Format(@"{0}\{1}", ServerName, InstanceName);
        }
    }
}