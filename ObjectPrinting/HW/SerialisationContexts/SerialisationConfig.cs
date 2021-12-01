using System;

namespace ObjectPrinting
{
    public class SerialisationConfig<TOwner, TType>
    {
        private readonly PrintingRules rules;
        private readonly string? propertyName;
        
        internal SerialisationConfig(PrintingRules rules)
        {
            this.rules = rules;
        }
        
        internal SerialisationConfig(PrintingRules rules, string propertyName)
        {
            this.rules = rules;
            this.propertyName = propertyName;
        }
        
        private SerialisationRule SetMethod<T>(Func<T, string> serialisationMethod)
        {
            var serialisationRule = new SerialisationRule(o => serialisationMethod((T)o!));
            
            if (propertyName is null)
            {
                if (!rules.TrySetSerialisation(typeof(T), serialisationRule))
                    throw new InvalidOperationException($"serialisation for type {typeof(T).Name} already setted");
            }
            else
            {
                if (!rules.TrySetSerialisation(propertyName, serialisationRule))
                    throw new InvalidOperationException($"serialisation for property {propertyName} already setted");
            }
            
            return serialisationRule;
        }
        
        public SerialisationIntermediateConfig<TOwner> WithMethod(Func<TType, string> serialisationMethod)
        {
            SetMethod(serialisationMethod);
            return new SerialisationIntermediateConfig<TOwner>(rules);
        }
        
        public StringSerialisationConfig<TOwner> WithMethod(Func<string, string> serialisationMethod)
        {
            return new StringSerialisationConfig<TOwner>(rules, SetMethod(serialisationMethod));
        }
    }
}