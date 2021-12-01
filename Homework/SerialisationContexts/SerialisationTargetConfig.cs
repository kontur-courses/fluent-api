using System;
using System.Linq.Expressions;

namespace Homework.SerialisationContexts
{
    public class SerialisationTargetConfig<TOwner>
    {
        private readonly PrintingRules rules;
        internal SerialisationTargetConfig(PrintingRules rules)
        {
            this.rules = rules;
        }
        
        public SerialisationConfig<TOwner, T> For<T>(Expression<Func<TOwner, T>> extractor)
        {
            rules.CheckForInvalidOperations(extractor.GetPropertyName(), typeof(T));
            return new SerialisationConfig<TOwner, T>(rules, extractor.GetPropertyName());
        }

        public SerialisationConfig<TOwner, T> For<T>()
        {
            rules.CheckForInvalidOperations(type: typeof(T));
            return new SerialisationConfig<TOwner, T>(rules);
        }
    }
}