using System.Drawing;
using Tuitor.packages.richtext.format;

namespace ParserGeneratorTest;

partial class JsonParser : FormatParser, IFollowScanner
{
    public override bool IsFormatChar(char c) => c == ',' || c == ':';

    public override Color GetTokenColor(ushort token) => GetMatchColor(token);

    public override string GetTokenName(ushort token) => GetMatchName(token);

    public override bool IsSkipToken(ushort token) => IsSkipMatch(token);

    protected override unsafe object InnerParse(char* input, int length)
    {
        return parse(input, length);
    }
}
