using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public readonly HashSet<Type> excludeTypes = new HashSet<Type>();
        public readonly HashSet<string> excludeProperties = new HashSet<string>();

        public readonly Dictionary<string, Delegate> propertyOperations = new Dictionary<string, Delegate>();
        public readonly Dictionary<Type, Delegate> typeOperations = new Dictionary<Type, Delegate>();

        private readonly List<Type> finalTypes = new List<Type>
        {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(long)
        };

        public PrintingConfig<TOwner> Exclude<TPropertyType>()
        {
            excludeTypes.Add(typeof(TPropertyType));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TProperty>(Expression<Func<TOwner, TProperty>> propSelector)
        {
            var member = (propSelector.Body as MemberExpression)?.ToString();
            var property = member?.Substring(member.IndexOf('.'));
            excludeProperties.Add(property);
            return this;
        }

        public SerializingConfig<TOwner, TPropType> Serialize<TPropType>()
        {
            return new SerializingConfig<TOwner, TPropType>(this);
        }

        public SerializingConfig<TOwner, TProperty> Serialize<TProperty>(Func<TOwner, TProperty> propSelector)
        {
            return new SerializingConfig<TOwner, TProperty>(this);
        }

        public PrintingConfig<TOwner> DefaultSerialize()
        {
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var propType = propertyInfo.PropertyType;

                if (excludeTypes.Contains(propType) || excludeProperties.Contains($".{propertyInfo.Name}"))
                    continue;

                sb.Append(identation + propertyInfo.Name + " = ");

                if (typeOperations.ContainsKey(propType))
                    sb.Append(PrintToString(typeOperations[propType].DynamicInvoke(propertyInfo.GetValue(obj)),
                                  nestingLevel + 1));
                else
                    sb.Append(PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }

            return sb.ToString();
        }
    }
}