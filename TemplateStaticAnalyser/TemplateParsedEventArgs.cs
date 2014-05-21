using System;

namespace TemplateStaticAnalyser
{
    public class TemplateParsedEventArgs : EventArgs
    {
        public TemplateParsedEventArgs(int templateCount)
        {
            TemplateCount = templateCount;
        }

        public int TemplateCount { get; private set; }
    }
}