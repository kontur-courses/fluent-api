using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class IgnoreConfig<TOwner>
    {
        private readonly PrintingRules rules;

        internal IgnoreConfig(PrintingRules rules)
        {
            this.rules = rules;
        }
        
        public IgnoreTypeConfig<TOwner, T> Type<T>()
        {
            if (!rules.TrySetIgnore(typeof(T), false))
                throw new InvalidOperationException($"type {typeof(T).Name} already ignored");
            return new IgnoreTypeConfig<TOwner, T>(rules);
        }

        public IgnoreTypeIntermediateConfig<TOwner> Property<T>(Expression<Func<TOwner, T>> extractor)
        {
            var propertyName = extractor.GetPropertyName();
            if (!rules.TrySetIgnore(propertyName))
                throw new InvalidOperationException($"property {propertyName} already ignored");
            return new IgnoreTypeIntermediateConfig<TOwner>(rules);
        }
    }
}