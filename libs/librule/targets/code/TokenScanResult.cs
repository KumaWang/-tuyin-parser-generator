namespace librule.targets.code
{
    internal class TokenScanResult
    {
        public TokenScanResult(TargetGraph targetGraph)
        {
            TargetGraph = targetGraph;
        }

        public TargetGraph TargetGraph { get; }
    }
}
