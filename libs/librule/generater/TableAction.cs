namespace librule.generater
{
    struct TableAction : IEquatable<TableAction>
    {
        public TableAction(string context, int front, ushort token)
        {
            Context = context;
            Front = front;
            Token = token;
        }

        public string Context { get; }

        public int Front { get; }

        public ushort Token { get; }

        public override bool Equals(object obj)
        {
            return obj is TableAction action &&
                   Context == action.Context &&
                   Front == action.Front && 
                   Token == action.Token;
        }

        public bool Equals(TableAction other)
        {
            return Context.Equals(other.Context) && Front.Equals(other.Front) && Token.Equals(other.Token);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Context, Front, Token);
        }
    }
}
