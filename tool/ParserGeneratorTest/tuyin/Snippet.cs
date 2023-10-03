namespace Tuitor.packages.richtext.format
{
    class Snippet : Word
    {
        public Snippet(string @short, string full)
        {
            Value = @short;
            Full = full;
        }

        public string Value { get; }

        public string Full { get; }
    }
}
