namespace librule.generater
{
    public class GraphAction
    {
        public GraphAction(string name, string content, bool goto0)
        {
            Name = name;
            Content = content;
            Goto0 = goto0;
            Paramters = new List<string>();
        }

        public string Name { get; }

        public string Content { get; }

        public bool Goto0 { get; }

        public List<string> Paramters { get; }
    }
}
