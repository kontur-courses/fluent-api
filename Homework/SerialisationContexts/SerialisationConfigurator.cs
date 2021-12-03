using System;

namespace Homework.SerialisationContexts
{
    public class SerialisationConfigurator<TOwner, TType>
    {
        private readonly PrintingRules rules;
        private readonly string? propertyName;
        
        internal SerialisationConfigurator(PrintingRules rules)
        {
            this.rules = rules;
        }
        
        internal SerialisationConfigurator(PrintingRules rules, string propertyName)
        {
            this.rules = rules;
            this.propertyName = propertyName;
        }
        
        private SerialisationRule SetMethod<T>(Func<T, string> serialisationMethod)
        {
            var serialisationRule = new SerialisationRule(typeof(T), o => serialisationMethod((T)o!));
            
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
        
        public ISerialisationIntermediateConfigurator<TOwner> WithMethod(Func<TType, string> serialisationMethod)
        {
            SetMethod(serialisationMethod);
            return new PrinterConfigurator<TOwner>(rules);
        }
        
        public IStringSerialisationConfigurator<TOwner> WithMethod(Func<string, string> serialisationMethod)
        {
            return new PrinterConfigurator<TOwner>(rules, SetMethod(serialisationMethod));
        }
    }
}