namespace Homework.IgnoreContexts
{
    public class IgnoreTypeConfig<TOwner, TType> : Config<TOwner>
    {
        public IgnoreConfig<TOwner> And => new(Rules);
        
        internal IgnoreTypeConfig(PrintingRules rules) : base(rules) { }
        
        public IgnoreTypeIntermediateConfig<TOwner> InAllNestingLevels()
        {
            Rules.TrySetIgnore(typeof(TType), true);
            return new IgnoreTypeIntermediateConfig<TOwner>(Rules);
        }
    }
}