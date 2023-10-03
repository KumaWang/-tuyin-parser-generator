namespace librule.targets
{
    record TargetMetadataStep
    {
        public TargetMetadataStep(string value, bool isResult, bool isAutoGenerate, bool isFormal=false)
        {
            Values = new List<string>();
            if(value != null)
                Values.Add(value);

            IsResult = isResult;
            IsFormal = isFormal;
            IsAutoGenerate = isAutoGenerate;
        }

        public List<string> Values { get; }

        public bool IsResult { get; }

        public bool IsFormal { get; }

        public bool IsAutoGenerate { get; }

        public override string ToString()
        {
            return string.Concat(Values);
        }
    }
}
