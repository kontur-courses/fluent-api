using System;
using System.Globalization;
using System.Linq.Expressions;
using Homework.CultureContexts;
using Homework.IgnoreContexts;
using Homework.SerialisationContexts;

namespace Homework
{
    public class PrinterConfigurator<TOwner> : IPrinterConfigurator<TOwner>, 
        ICultureConfigurator<TOwner>, ICultureIntermediateConfigurator<TOwner>,
        IIgnoreConfigurator<TOwner>, IIgnoreTypeIntermediateConfigurator<TOwner>, IIgnoreTypeConfigurator<TOwner>,
        ISerialisationTargetConfigurator<TOwner>, ISerialisationIntermediateConfigurator<TOwner>, IStringSerialisationConfigurator<TOwner>
    {
        
        private readonly PrintingRules rules;
        private readonly SerialisationRule? lastSerialisationRule;
        private Type? lastType;
        
        public ICultureConfigurator<TOwner> And => this;
        IIgnoreConfigurator<TOwner> IIgnoreTypeConfigurator<TOwner>.And => this;
        IIgnoreConfigurator<TOwner> IIgnoreTypeIntermediateConfigurator<TOwner>.And => this;
        ISerialisationTargetConfigurator<TOwner> ISerialisationIntermediateConfigurator<TOwner>.And => this;
        
        internal PrinterConfigurator(PrintingRules rules)
        {
            this.rules = rules;
        }
        
        internal PrinterConfigurator(PrintingRules rules, SerialisationRule lastSerialisationRule)
        {
            this.lastSerialisationRule = lastSerialisationRule;
            this.rules = rules;
        }

        public static IPrinterConfigurator<TOwner> CreateConfig() => new PrinterConfigurator<TOwner>(new PrintingRules());
        public ICultureConfigurator<TOwner> SetCulture() => this;
        public IIgnoreConfigurator<TOwner> Ignore() => this;
        public ISerialisationTargetConfigurator<TOwner> SetAlternativeSerialisation() => this;
        
        
        public ICultureIntermediateConfigurator<TOwner> For<TType>(CultureInfo currentCulture)
            where TType : IConvertible
        {
            rules.CheckForInvalidOperations(type: typeof(TType));
            if (!rules.TrySetCulture(typeof(TType), currentCulture))
                throw new InvalidOperationException($"culture for type {nameof(TType)} already setted");
            return this;
        }

        public IPrinterConfigurator<TOwner> ForAllOthers(CultureInfo culture)
        {
            if (rules.DefaultCulture.Setted)
                throw new InvalidOperationException("default culture already setted");
            rules.DefaultCulture.Value = culture;
            return this;
        }

        public ObjectPrinter<TOwner> Configure() => new((PrintingRules)rules.Clone());

        public IIgnoreTypeConfigurator<TOwner> Type<T>()
        {
            rules.CheckForInvalidOperations(type: typeof(T));
            if (!rules.TrySetIgnore(typeof(T), false))
                throw new InvalidOperationException($"type {typeof(T).Name} already ignored");
            lastType = typeof(T);
            return this;
        }

        public IIgnoreTypeIntermediateConfigurator<TOwner> Property<T>(Expression<Func<TOwner, T>> extractor)
        {
            var propertyName = extractor.GetPropertyName();
            rules.CheckForInvalidOperations(propertyName, typeof(T));
            if (!rules.TrySetIgnore(propertyName))
                throw new InvalidOperationException($"property {propertyName} already ignored");
            lastType = typeof(T);
            return this;
        }
        
        public IIgnoreTypeIntermediateConfigurator<TOwner> InAllNestingLevels()
        {
            rules.TrySetIgnore(lastType!, true);
            return this;
        }

        public SerialisationConfigurator<TOwner, T> For<T>(Expression<Func<TOwner, T>> extractor)
        {
            rules.CheckForInvalidOperations(extractor.GetPropertyName(), typeof(T));
            return new SerialisationConfigurator<TOwner, T>(rules, extractor.GetPropertyName());
        }

        public SerialisationConfigurator<TOwner, T> For<T>()
        {
            rules.CheckForInvalidOperations(type: typeof(T));
            return new SerialisationConfigurator<TOwner, T>(rules);
        }
        
        public ISerialisationIntermediateConfigurator<TOwner> WithCharsLimit(int maxStringLenght)
        {
            lastSerialisationRule!.CharsLimit = maxStringLenght;
            return this;
        }
    }
}