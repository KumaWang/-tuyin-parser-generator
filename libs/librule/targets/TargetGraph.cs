using librule.generater;

namespace librule.targets
{
    internal class TargetGraph : GraphBox<TargetMetadatas, object>
    {
        public override Lexicon Lexicon => throw new NotImplementedException();

        public override TargetMetadatas CreateMetadata(GraphEdge<TargetMetadatas> edge, object action, bool skipToken)
        {
            throw new NotImplementedException();
        }

        public override int GetMetadataCompreValue(TargetMetadatas metadata)
        {
            throw new NotImplementedException();
        }

        public override GraphEdgeValue GetMissValue()
        {
            throw new NotImplementedException();
        }

        public override TargetMetadatas MergeMetadatas(IEnumerable<TargetMetadatas> metadatas)
        {
            throw new NotImplementedException();
        }

        public override GraphTable<TargetMetadatas> Tabulate()
        {
            throw new NotImplementedException();
        }

        public override void TokenEdge(GraphEdge<TargetMetadatas> edge, ushort token)
        {
            throw new NotImplementedException();
        }
    }
}
