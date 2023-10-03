using Tuitor.utils;

namespace Tuitor.packages.richtext.format
{
    sealed class ParseReport
    {
        private readonly List<ParseError> errors = new List<ParseError>();
        private readonly List<SourceScope> scopes = new List<SourceScope>();
        private readonly List<SourceState> states = new List<SourceState>();
        private readonly List<SourceFormat> formats = new List<SourceFormat>();
        private readonly DynamicArray<Match> matches;

        public ParseReport(DynamicArray<Match> matches) 
        {
            this.matches = matches;
        }

        public IReadOnlyList<ParseError> Errors => errors;

        public IReadOnlyArray<Match> Matches => matches;

        public IReadOnlyList<SourceScope> Scopes => scopes;

        public IReadOnlyList<SourceState> IndexStates => states;

        public IReadOnlyList<SourceFormat> IndexFormats => formats;

        internal void ReportError(SourceSpan span, string msg) => errors.Add(new ParseError(ParseErrorLevel.Error, 0, span, msg));

        internal void AddMatch(Match match) => matches[matches.Length] = match;

        internal void AddState(SourceState state) => states.Add(state);

        internal void SetState(int index, SourceState state) => states[index] = state;

        internal void AddFormat(SourceFormat format) => formats.Add(format);

        internal void SetFormat(int index, SourceFormat format) => formats[index] = format;

        internal void AddScope(int start, int end, string dispaly) => scopes.Add(new SourceScope(new SourceSpan(start, end), dispaly));
    }
}
