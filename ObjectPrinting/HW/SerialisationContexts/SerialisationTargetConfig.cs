using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class SerialisationTargetConfig<TOwner>
    {
        private readonly PrintingRules rules;
        internal SerialisationTargetConfig(PrintingRules rules)
        {
            this.rules = rules;
        }
        
        public SerialisationConfig<TOwner, T> For<T>(Expression<Func<TOwner, T>> extractor) 
            => new(rules, extractor.GetPropertyName());
        public SerialisationConfig<TOwner, T> For<T>() 
            => new(rules);
    }
}