namespace Tuitor.packages.richtext.format
{
    public class Literal : Word
    {
        public Literal(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}
