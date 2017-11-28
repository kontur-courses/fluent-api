using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly Dictionary<Type, ITransformator> typeTransformators;
        private readonly Dictionary<PropertyInfo, ITransformator> propertyTransformators;
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<PropertyInfo> excludedProperties;

        public PrintingConfig()
        {
            typeTransformators = new Dictionary<Type, ITransformator>();
            propertyTransformators = new Dictionary<PropertyInfo, ITransformator>();
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

        public void SetTypeTransformationRule<T>(Func<T, string> transformFunction, TransformationType transformationType)
        {
            var transformator = Transformator.CreateFrom(transformFunction, transformationType);
            if (!typeTransformators.TryGetValue(typeof(T), out var value) || value.TransformationType <= transformator.TransformationType) typeTransformators[typeof(T)] = transformator;
        }

        public void SetPropertyTransformationRule<TProp>(PropertyInfo propertyInfo, Func<TProp, string> transformFunction, TransformationType transformationType)
        {
            var transformator = Transformator.CreateFrom(transformFunction, transformationType);
            if (!propertyTransformators.TryGetValue(propertyInfo, out var value) || value.TransformationType <= transformator.TransformationType) propertyTransformators[propertyInfo] = transformator;
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

        IEnumerable<Type> IPrintingConfig<TOwner>.ExcludedTypes => excludedTypes;

        IEnumerable<PropertyInfo> IPrintingConfig<TOwner>.ExcludedProperties => excludedProperties;

        IEnumerable<Type> IPrintingConfig<TOwner>.FinalTypes
            => new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };

        IReadOnlyDictionary<Type, ITransformator> IPrintingConfig<TOwner>.TypeTransformators
            => typeTransformators;

        IReadOnlyDictionary<PropertyInfo, ITransformator> IPrintingConfig<TOwner>.PropertyTransformators
            => propertyTransformators;
    }
}