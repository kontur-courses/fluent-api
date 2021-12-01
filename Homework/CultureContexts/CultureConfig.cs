using System;
using System.Globalization;

namespace Homework.CultureContexts
{
    public class CultureConfig<TOwner>
    {
        private readonly PrintingRules rules;

        internal CultureConfig(PrintingRules rules)
        {
            this.rules = rules;
        }

        public CultureIntermediateConfig<TOwner> For<TType>(CultureInfo currentCulture)
            where TType : IConvertible
        {
            rules.CheckForInvalidOperations(type: typeof(TType));
            if (!rules.TrySetCulture(typeof(TType), currentCulture))
                throw new InvalidOperationException($"culture for type {nameof(TType)} already setted");
            return new CultureIntermediateConfig<TOwner>(rules);
        }
        
        public Config<TOwner> ForAllOthers(CultureInfo culture)
        {
            if (rules.DefaultCulture.Setted)
                throw new InvalidOperationException("default culture already setted");
            rules.DefaultCulture.Value = culture;
            return new Config<TOwner>(rules);
        }
        
        public class CultureIntermediateConfig<T> : Config<T>
        {
            internal CultureIntermediateConfig(PrintingRules rules) : base(rules) { }
            public CultureConfig<T> And => new(Rules);
        }
    }
}