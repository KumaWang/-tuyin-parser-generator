namespace libfsm
{
    public struct FAValue<T>
    {
        public ushort   Shift   
        {
            get;
        }

        public ushort   Subset 
        {
            get;
        }

        public T        Metadata   
        {
            get;
        }

        public FAValue(ushort shift, ushort subset, T token)
        {
            Shift = shift;
            Subset = subset;
            Metadata = token;
        }

        public override int GetHashCode()
        {
            return Shift ^ Subset ^ Metadata.GetHashCode();
        }
    }
}
