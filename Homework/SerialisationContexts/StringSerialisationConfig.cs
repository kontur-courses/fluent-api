namespace Homework.SerialisationContexts
{
    public class StringSerialisationConfig<TOwner> : SerialisationIntermediateConfig<TOwner>
    {
        private readonly SerialisationRule lastSerialisationRule;

        internal StringSerialisationConfig(PrintingRules rules, SerialisationRule lastSerialisationRule) : base(rules)
        {
            this.lastSerialisationRule = lastSerialisationRule;
        }
        
        public SerialisationIntermediateConfig<TOwner> WithCharsLimit(int maxStringLenght)
        {
            lastSerialisationRule.CharsLimit = maxStringLenght;
            return this;
        }
    }
}