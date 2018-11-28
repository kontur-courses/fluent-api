using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ObjectPrinting.SerializingConfig;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludeTypes = new HashSet<Type>();
        private readonly HashSet<string> excludeProperties = new HashSet<string>();
        private readonly Dictionary<Type, Delegate> typeOperations = new Dictionary<Type, Delegate>();

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
            var property = member?.Substring(member.IndexOf('.') + 1);
            excludeProperties.Add(property);
            return this;
        }

        public SerializingConfig<TOwner, TPropType> Serialize<TPropType>()
        {
            return new SerializingConfig<TOwner, TPropType>(this, typeOperations);
        }

        public SerializingConfig<TOwner, TProperty> Serialize<TProperty>(Expression<Func<TOwner, TProperty>> propSelector)
        {
            return new SerializingConfig<TOwner, TProperty>(this, typeOperations);
        }

        public PrintingConfig<TOwner> DefaultSerialize()
        {
            excludeTypes.Clear();
            excludeProperties.Clear();
            typeOperations.Clear();
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public void PrintToString(List<TOwner> objects)
        {
            foreach (var obj in objects)
                PrintToString(obj);
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
            var propertiesInfo = type.GetProperties().Where(p =>
                !excludeTypes.Contains(p.PropertyType) && !excludeProperties.Contains(p.Name));

            foreach (var propertyInfo in propertiesInfo)
            {
                var propType = propertyInfo.PropertyType;
                sb.Append(identation + propertyInfo.Name + " = ");
                sb.Append(typeOperations.ContainsKey(propType)
                    ? PrintToString(typeOperations[propType].DynamicInvoke(propertyInfo.GetValue(obj)),
                        nestingLevel + 1)
                    : PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }

            return sb.ToString();
        }
    }
}