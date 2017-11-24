using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly Dictionary<Type, ITransformator> typeTransformators;
        private readonly Dictionary<PropertyInfo, ITransformator> propertyTransformators;
        private readonly List<Type> excludedTypes;
        private readonly List<PropertyInfo> excludedProperties;

        public PrintingConfig()
        {
            typeTransformators = new Dictionary<Type, ITransformator>();
            propertyTransformators = new Dictionary<PropertyInfo, ITransformator>();
            excludedTypes = new List<Type>();
            excludedProperties = new List<PropertyInfo>();
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

        public void SetTypeTransformationRule<T>(Func<T, string> transformFunction, TransformationType type)
        {
            var transformator = Transformator.CreateFrom(transformFunction, type);
            TryUpdateTransformator(typeTransformators, typeof(T), transformator);
        }

        public void SetPropertyTransformationRule<TProp>(PropertyInfo propertyInfo, Func<TProp, string> transformFunction, TransformationType type)
        {
            var transformator = Transformator.CreateFrom(transformFunction, type);
            TryUpdateTransformator(propertyTransformators, propertyInfo, transformator);
        }

        private void TryUpdateTransformator<T>(Dictionary<T, ITransformator> dict, T key, ITransformator transformator)
        {
            if (!dict.ContainsKey(key))
                dict[key] = transformator;
            else
            {
                var currentTransformator = dict[key];
                if (currentTransformator.TransformationType <= transformator.TransformationType)
                    dict[key] = transformator;
            }
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