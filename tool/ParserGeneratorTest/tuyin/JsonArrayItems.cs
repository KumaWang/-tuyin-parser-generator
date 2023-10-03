namespace ParserGeneratorTest
{
    internal class JsonArrayItems : List<JsonItem>
    {
        public JsonArrayItems()
        {
        }

        public JsonArrayItems(JsonItem loc_6_0)
        {
            base.Add(loc_6_0);
        }

        public new JsonArrayItems Add(JsonItem item)
        {
            base.Add(item);
            return this;
        }
    }
}