using System.Drawing;
using Tuitor.utils;

namespace Tuitor.packages.richtext.format
{
    unsafe interface IFormatParser
    {
        char LineBreak { get; }

        bool IsFormatChar(char c);

        bool IsWhitespaceChar(char c);

        object Parse(char* input, int length);

        Color GetTokenColor(ushort token);

        string GetTokenName(ushort token);

        bool IsSkipToken(ushort token);

        IEnumerable<Word> GetFollows(int index)
        {
            var scanner = this as IFollowScanner;
            return null;
        }
    }
}
