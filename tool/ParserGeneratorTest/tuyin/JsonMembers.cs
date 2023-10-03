namespace ParserGeneratorTest
{
    internal class JsonMembers : List<JsonMember>
    {
        public JsonMembers()
        {
        }

        public JsonMembers(JsonMember loc_3_0)
        {
            base.Add(loc_3_0);
        }

        public new JsonMembers Add(JsonMember member) 
        {
            base.Add(member);
            return this;
        }
    }
}