namespace librule.generater
{
    struct ProductionMetadata : IEquatable<ProductionMetadata>
    {
        public short Value;
        public ushort Token;
 
        public ProductionMetadata(short value, ushort token)
        {
            Value = value;
            Token = token;
        }

        public override bool Equals(object obj)
        {
            return obj is ProductionMetadata metadata &&
                   Value == metadata.Value &&
                   Token == metadata.Token;
        }

        public bool Equals(ProductionMetadata other)
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
