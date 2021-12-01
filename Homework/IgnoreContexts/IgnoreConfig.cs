using System;
using System.Linq.Expressions;

namespace Homework.IgnoreContexts
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
            rules.CheckForInvalidOperations(type: typeof(T));
            if (!rules.TrySetIgnore(typeof(T), false))
                throw new InvalidOperationException($"type {typeof(T).Name} already ignored");
            return new IgnoreTypeConfig<TOwner, T>(rules);
        }

        public IgnoreTypeIntermediateConfig<TOwner> Property<T>(Expression<Func<TOwner, T>> extractor)
        {
            var propertyName = extractor.GetPropertyName();
            rules.CheckForInvalidOperations(propertyName, typeof(T));
            if (!rules.TrySetIgnore(propertyName))
                throw new InvalidOperationException($"property {propertyName} already ignored");
            return new IgnoreTypeIntermediateConfig<TOwner>(rules);
        }
    }
}