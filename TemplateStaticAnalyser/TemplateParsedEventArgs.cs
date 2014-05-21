using System;

namespace TemplateStaticAnalyser
{
    public class TemplateParsedEventArgs : EventArgs
    {
        public TemplateParsedEventArgs(int templateIndex, int templateCount)
        {
            TemplateIndex = templateIndex;
            TemplateCount = templateCount;
        }

        public int TemplateIndex { get; private set; }
        public int TemplateCount { get; private set; }
    }
}