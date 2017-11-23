using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private static readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private readonly Dictionary<Type, ITransformator> typeTransformators;
        private readonly Dictionary<string, ITransformator> propertyTransformators;

        public PrintingConfig()
        {
            typeTransformators = new Dictionary<Type, ITransformator>();
            propertyTransformators = new Dictionary<string, ITransformator>();
        }

        public PrintingConfig<TOwner> Exclude<TExcluded>()
        {
            SetTypeTransformationRule<TExcluded>(obj => null);

            return this;
        }

        public PrintingConfig<TOwner> Exclude<TProp>(Expression<Func<TOwner, TProp>> selector)
        {
            var propertyInfo = ((MemberExpression)selector.Body).Member as PropertyInfo;
            SetPropertyTransformationRule<TProp>(propertyInfo.Name, p => null);

            return this;
        }

        public void SetTypeTransformationRule<T>(Func<T, string> transformFunction)
        {
            var transformator = Transformator.CreateFrom(transformFunction);
            typeTransformators[typeof(T)] = transformator;
        }

        public void SetPropertyTransformationRule<TProp>(string propertyName, Func<TProp, string> transformFunction)
        {
            var transformator = Transformator.CreateFrom(transformFunction);
            propertyTransformators[propertyName] = transformator;
        }

        public TypePrintingConfig<TOwner, T> Printing<T>() => new TypePrintingConfig<TOwner, T>(this);

        public PropertyPrintingConfig<TOwner, TProp> Printing<TProp>(
            Expression<Func<TOwner, TProp>> selector
        )
        {
            var propertyInfo = ((MemberExpression)selector.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TProp>(this, propertyInfo.Name);
        }

        public string PrintToString(TOwner obj) => Serialize(obj, obj.GetType().Name, 0, out bool _);

        private string Serialize(object obj, string name, int nestingLevel, out bool isCompositeObject)
        {
            isCompositeObject = false;

            if (obj == null)
                return "null";

            if (propertyTransformators.ContainsKey(name) && nestingLevel != 0)
                return propertyTransformators[name].Transform(obj);

            var objectType = obj.GetType();
            if (typeTransformators.ContainsKey(objectType) && nestingLevel != 0)
                return typeTransformators[objectType].Transform(obj);

            if (finalTypes.Contains(objectType))
                return obj.ToString();

            isCompositeObject = true;
            return SerializeCompositeObject(obj, name, nestingLevel);
        }

        private string SerializeCompositeObject(object obj, string name, int nestingLevel)
        {
            var sb = new StringBuilder();
            sb.AppendLine(name);
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                var serializedProperty = Serialize(
                    propertyInfo.GetValue(obj), propertyInfo.Name, nestingLevel + 1, out var isCompositeProperty);

                if (serializedProperty != null)
                {
                    var identation = new string('\t', nestingLevel + 1);
                    var prefix = isCompositeProperty ? "" : propertyInfo.Name + " = ";
                    sb.AppendLine($"{identation}{prefix}{serializedProperty}");
                }
            }
            return sb.ToString();
        }
    }
}