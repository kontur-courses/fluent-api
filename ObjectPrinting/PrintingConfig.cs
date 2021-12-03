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
        private readonly Type[] finalTypes;
        private readonly HashSet<Type> exceptTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> exceptProperties = new HashSet<PropertyInfo>();
        private readonly Dictionary<Type, Delegate> alternativeSerializers = new Dictionary<Type, Delegate>();
        private readonly Dictionary<MemberInfo, Delegate> propertySerializers = new Dictionary<MemberInfo, Delegate>();

        public PrintingConfig()
        {
            finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string), typeof(DateTime), typeof(TimeSpan)
            };
        }

        public void AddAlternativeTypeSerializer(Type type, Delegate serializer)
        {
            if (alternativeSerializers.ContainsKey(type))
            {
                alternativeSerializers[type] = serializer;
            }
            else
            {
                alternativeSerializers.Add(type, serializer);
            }
        }

        public void AddAlternativePropertySerializer(PropertyInfo property, Delegate serializer)
        {
            if (propertySerializers.ContainsKey(property))
            {
                propertySerializers[property] = serializer;
            }
            else
            {
                propertySerializers.Add(property, serializer);
            }
        }

        public TypeConfig<TOwner, TProperty> Serialize<TProperty>()
        {
            return new TypeConfig<TOwner, TProperty>(this);
        }

        public PropertyConfig<TOwner> Serialize<TProperty>(Expression<Func<TOwner, TProperty>> selector)
        {
            return new PropertyConfig<TOwner>(this, (PropertyInfo)(selector.Body as MemberExpression)?.Member);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> ExceptType<TExcept>()
        {
            exceptTypes.Add(typeof(TExcept));
            return this;
        }

        public PrintingConfig<TOwner> ExceptProperty<TProperty>(Expression<Func<TOwner, TProperty>> selector)
        {
            exceptProperties.Add((PropertyInfo)(selector.Body as MemberExpression)?.Member);
            return this;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
            {
                return "null" + Environment.NewLine;
            }

            if (finalTypes.Contains(obj.GetType()))
            {
                return obj + Environment.NewLine;
            }

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties()
                .Where(p => !exceptTypes.Contains(p.PropertyType) && !exceptProperties.Contains(p)))
            {
                sb.Append(indentation + propertyInfo.Name + " = " + GetSerialization(obj, propertyInfo, nestingLevel));
            }

            return sb.ToString();
        }

        private string GetSerialization(object obj, PropertyInfo propertyInfo, int nestingLevel)
        {
            if (propertySerializers.ContainsKey(propertyInfo))
            {
                return propertySerializers[propertyInfo].DynamicInvoke(propertyInfo) + Environment.NewLine;
            }

            if (alternativeSerializers.ContainsKey(propertyInfo.PropertyType))
            {
                return alternativeSerializers[propertyInfo.PropertyType].DynamicInvoke(propertyInfo.GetValue(obj)) +
                       Environment.NewLine;
            }

            return PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
        }
    }
}