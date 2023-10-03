namespace librule.generater
{
    struct TokenMetadata : IEquatable<TokenMetadata>
    {
        public short Value;
        public ushort Token;

        public TokenMetadata(short value, ushort token)
        {
            Value = value;
            Token = token;
        }

        public override bool Equals(object obj)
        {
            return obj is TokenMetadata metadata &&
                   Value == metadata.Value &&
                   Token == metadata.Token;
        }

        public bool Equals(TokenMetadata other)
        {
            return other.Value == Value && other.Token == Token;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Token);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
