using System.Drawing;

namespace Tuitor.packages.richtext.format
{
    abstract class FormatParser : IFormatParser
    {
        private Dictionary<int, int> mFormatIndex = new Dictionary<int, int>();
        private Dictionary<int, int> mStateIndex = new Dictionary<int, int>();

        protected void Metadata(int index, string val, ParseReport report)
        {
            var targetIndex = mFormatIndex.ContainsKey(index) ? 
                mFormatIndex[index] : 
                -1;

            var format = targetIndex == -1 ? 
                new SourceFormat(index, 0, 0) : 
                report.IndexFormats[targetIndex];

            var indent = format.Indent;
            var interval = format.Interval;
            var items = val.Split('|');
            for(var i = 0; i < items.Length; i++) 
            {
                var item = items[i];
                var value = int.Parse(item.Substring(1));
                switch (item[0]) 
                {
                    case 'F':
                        interval = value;
                        break;
                    case 'I':
                        indent = value;
                        break;
                }
            }

            mFormatIndex[index] = targetIndex == -1 ? 
                report.IndexFormats.Count : 
                targetIndex;

            format = new SourceFormat(index, indent, interval);
            if (targetIndex == -1)
                report.AddFormat(format);
            else
                report.SetFormat(targetIndex, format);
        }

        protected void State(int index, ushort state, ParseReport report)
        {
        }

        protected void State(int index, SourceState state, ParseReport report)
        {
        }

        protected SourceState GetState(int index, ParseReport report)
        {
            return default;
        }

        protected void OnMatch(int start, int end, ushort token, ParseReport report) 
        {
        }

        protected void ReportError(SourceSpan span, string message) 
        {
        }

        protected void Scope(int start, int end, string dispaly, ParseReport report) 
        {
        }

        protected unsafe abstract object InnerParse(char* input, int length);

        public unsafe virtual object Parse(char* input, int length)
        {
            mFormatIndex.Clear();
            mStateIndex.Clear();

            return InnerParse(input, length);
        }

        public unsafe virtual object Parse(string str)
        {
            fixed (char* input = str)
            {
                mFormatIndex.Clear();
                mStateIndex.Clear();

                return InnerParse(input, str.Length);
            }
        }

        public unsafe void Parse1000000(string str)
        {
            fixed (char* input = str)
            {
                for (var i = 0; i < 1000000; i++)
                {
                    InnerParse(input, str.Length);
                }
            }
        }

        public abstract Color GetTokenColor(ushort token);

        public virtual char LineBreak => '\n';

        public virtual bool IsFormatChar(char c) => default;

        public virtual bool IsWhitespaceChar(char c) => c == ' ';

        public abstract string GetTokenName(ushort token);

        public abstract bool IsSkipToken(ushort token);
    }
}
