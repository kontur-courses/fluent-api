namespace ObjectPrinting
{
    public class SerialisationIntermediateConfig<TOwner> : Config<TOwner>
    {
        internal SerialisationIntermediateConfig(PrintingRules rules) : base(rules) { }
        public SerialisationTargetConfig<TOwner> And => new(Rules);
    }
}