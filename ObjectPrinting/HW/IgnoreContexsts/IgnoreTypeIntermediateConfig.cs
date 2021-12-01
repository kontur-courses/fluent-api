namespace ObjectPrinting
{
    public class IgnoreTypeIntermediateConfig<TOwnerInner> : Config<TOwnerInner>
    {
        internal IgnoreTypeIntermediateConfig(PrintingRules rules) : base(rules) { }
        public IgnoreConfig<TOwnerInner> And => new(Rules);
    }
}