using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private class TransformationEntry
        {
            internal TransformationEntry(ITransformator transformator, int transforamtionPriority)
            {
                Transformator = transformator;
                Priority = transforamtionPriority;
            }

            internal ITransformator Transformator { get; }
            internal int Priority { get; }
        }

        private readonly Dictionary<Type, TransformationEntry> typeTransformations;
        private readonly Dictionary<PropertyInfo, TransformationEntry> propertyTransformations;
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<PropertyInfo> excludedProperties;

        public PrintingConfig()
        {
            typeTransformations = new Dictionary<Type, TransformationEntry>();
            propertyTransformations = new Dictionary<PropertyInfo, TransformationEntry>();
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<PropertyInfo>();
        }

        public PrintingConfig<TOwner> Excluding<TExcluded>()
        {
            excludedTypes.Add(typeof(TExcluded));

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TProp>(Expression<Func<TOwner, TProp>> selector)
        {
            var propertyInfo = ((MemberExpression) selector.Body).Member as PropertyInfo;
            excludedProperties.Add(propertyInfo);

            return this;
        }

        public void SetTypeTransformationRule<T>(Func<T, string> transformFunction, TypeTransformations transformationType)
        {
            var newEntry = new TransformationEntry(Transformator.CreateFrom(transformFunction), (int) transformationType);
            if (!typeTransformations.TryGetValue(typeof(T), out var value) || value.Priority <= newEntry.Priority) typeTransformations[typeof(T)] = newEntry;
        }

        public void SetPropertyTransformationRule<TProp>(PropertyInfo propertyInfo, Func<TProp, string> transformFunction, PropertyTransformations transformationType)
        {
            var newEntry = new TransformationEntry(Transformator.CreateFrom(transformFunction), (int)transformationType);
            if (!propertyTransformations.TryGetValue(propertyInfo, out var value) || value.Priority <= newEntry.Priority) propertyTransformations[propertyInfo] = newEntry;
        }

        public TypePrintingConfig<TOwner, T> Printing<T>() => new TypePrintingConfig<TOwner, T>(this);

        public PropertyPrintingConfig<TOwner, TProp> Printing<TProp>(
            Expression<Func<TOwner, TProp>> selector
        )
        {
            var propertyInfo = ((MemberExpression) selector.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TProp>(this, propertyInfo);
        }

        public ObjectPrinter<TOwner> Build() => new ObjectPrinter<TOwner>(this);

        IEnumerable<Type> IPrintingConfig.ExcludedTypes => excludedTypes;

        IEnumerable<PropertyInfo> IPrintingConfig.ExcludedProperties => excludedProperties;
        
        IReadOnlyDictionary<Type, ITransformator> IPrintingConfig.TypeTransformators
            => typeTransformations.ToDictionary(pair => pair.Key, pair => pair.Value.Transformator);

        IReadOnlyDictionary<PropertyInfo, ITransformator> IPrintingConfig.PropertyTransformators
            => propertyTransformations.ToDictionary(pair => pair.Key, pair => pair.Value.Transformator);
    }

}