using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<string> excludedProperties;
        private readonly HashSet<Type> excludedTypes;
        public Dictionary<Type, Func<object, string>> MethodsForPrintingTypes { get; }
        public Dictionary<string, Func<object, string>> MethodsForPrintingProperties { get; }

        readonly Type[] finalTypes = new[]
        {
            typeof(string), typeof(Guid),
        };

        public PrintingConfig()
        {
            excludedProperties = new HashSet<string>();
            excludedTypes = new HashSet<Type>();
            MethodsForPrintingTypes = new Dictionary<Type, Func<object, string>>();
            MethodsForPrintingProperties = new Dictionary<string, Func<object, string>>();
        }

        public PropertyPrintingConfig<TOwner, TField> Printing<TField>(
            Expression<Func<TOwner, TField>> property)
        {
            if (property.Body is MemberExpression expression)
                return new PropertyPrintingConfig<TOwner, TField>(this, GetPropertyName(expression.Member));
            throw new ArgumentException("Body should be MemberExpression");
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }
        public PrintingConfig<TOwner> ExcludingType<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> ExcludingTypes(params Type[] types)
        {
            excludedTypes.UnionWith(types);
            return this;
        }

        public PrintingConfig<TOwner> ExcludingProperty<TField>(Expression<Func<TOwner, TField>> property)
        {
            if (!(property.Body is MemberExpression expression))
                throw new ArgumentException("Body should be MemberExpression");
            
            excludedProperties.Add(GetPropertyName(expression.Member));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, new HashSet<object>());
        }

        private string PrintToString(object obj, int nestingLevel, HashSet<object> printedObjects, PropertyInfo propertyInfo = null)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (printedObjects.Contains(obj))
                return "Loopback!" + Environment.NewLine;

            var stringBuilder = new StringBuilder();
            var type = obj.GetType();
            var objectPrinting = GetPrintedObject(obj, type, propertyInfo);

            stringBuilder.Append(objectPrinting + Environment.NewLine);

            if (type.IsClass)
                printedObjects.Add(obj);

            if (type.IsPrimitive || finalTypes.Contains(type))
                return objectPrinting + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);

            if (obj is IEnumerable enumerable)
                stringBuilder.Append(GetPrintedEnumerableObj(enumerable, nestingLevel, printedObjects));

            else foreach (var propInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propInfo.PropertyType) ||
                    excludedProperties.Contains(GetPropertyName(propInfo)))
                    continue;

                var printedObject = PrintToString(
                    propInfo.GetValue(obj), nestingLevel + 1, printedObjects.ToHashSet(), propInfo);

                stringBuilder.Append($"{indentation}{propInfo.Name} = {printedObject}");
            }

            return stringBuilder.ToString();
        }

        private string GetPropertyName(MemberInfo memberInfo)
        {
            return $"{memberInfo?.DeclaringType?.FullName}.{memberInfo?.Name}";
        }

        private string GetPrintedObject(object obj, Type type, PropertyInfo propertyInfo)
        {
            if (MethodsForPrintingTypes.TryGetValue(type, out var method))
                return method(obj);

            var propertyName = GetPropertyName(propertyInfo);

            if (propertyInfo != null && MethodsForPrintingProperties.TryGetValue(propertyName, out method))
                return method(obj);

            if (type.IsPrimitive || finalTypes.Contains(type))
                return obj.ToString();

            return type.Name;
        }

        private string GetPrintedEnumerableObj(IEnumerable obj, int nestingLevel, HashSet<object> printedObjects)
        {
            var stringBuilder = new StringBuilder();

            if (obj is IDictionary dict)
            {
                AddPrintingDictPairs(dict, stringBuilder, nestingLevel, printedObjects);
            }
            else
            {
                AddPrintingSequenceElems(obj, stringBuilder, nestingLevel, printedObjects);
            }

            return stringBuilder.ToString();
        }

        private void AddPrintingSequenceElems(IEnumerable obj, StringBuilder builder,
            int nestingLevel, HashSet<object> printedObjects)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var count = 0;

            foreach (var element in obj)
            {
                builder.Append
                (
                    $"{indentation}{count++}: {PrintToString(element, nestingLevel + 1, printedObjects.ToHashSet())}"
                );
            }

        }

        private void AddPrintingDictPairs(IDictionary dict, StringBuilder sb,
            int nestingLevel, HashSet<object> printedObjects)
        {
            var indentation = new string('\t', nestingLevel + 1);

            foreach (DictionaryEntry pair in dict)
            {
                sb.Append
                (
                    $"{indentation}{pair.Key}: " +
                    $"{PrintToString(pair.Value, nestingLevel + 1, printedObjects.ToHashSet())}"
                );
            }
        }
    }
}