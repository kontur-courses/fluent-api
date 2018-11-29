using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> :IPrintingConfig
    {
        private List<string> notSerializingPropertis;
        private List<Type> notSerializingTypes;
        private Dictionary<string, Delegate> Serializers;
        private Dictionary<string, CultureInfo> Cultures;
        private Dictionary<string, int> TrimLenghts;

        Dictionary<string, Delegate> IPrintingConfig.PropertySerializer => Serializers;
        Dictionary<string, CultureInfo> IPrintingConfig.Cultures => this.Cultures;
        Dictionary<string, int> IPrintingConfig.TrimLenghts => this.TrimLenghts;

        public PrintingConfig()
        {
            notSerializingPropertis = new List<string>();
            notSerializingTypes = new List<Type>();
            Serializers = new Dictionary<string, Delegate>();
            TrimLenghts = new Dictionary<string, int>();
            Cultures = new Dictionary<string, CultureInfo>();
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            notSerializingTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> func)
        {
            var propertyName = ((MemberExpression) func.Body).Member.Name;
            notSerializingPropertis.Add(propertyName);
            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> Serialise<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this, typeof(TPropType));
        }

        public TypePrintingConfig<TOwner, TPropType> Serialise<TPropType>(Expression<Func<TOwner, TPropType>> func)
        {
            return new TypePrintingConfig<TOwner, TPropType>(this, func);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 1);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (notSerializingPropertis.Contains(propertyInfo.Name)) continue;
                if (notSerializingTypes.Contains(propertyInfo.PropertyType)) continue;
                sb.Append(identation + propertyInfo.Name + " = ");
                var serializedProperty = TrySerializePropertyInfo(propertyInfo, obj);
                if (serializedProperty != null)
                    sb.Append(serializedProperty);
                else
                    sb.Append(PrintToString(propertyInfo.GetValue(obj),
                        nestingLevel + 1));
            }
            return sb.ToString();
        }

        private string TrySerializePropertyInfo(PropertyInfo propertyInfo, object obj)
        {
            
            var propertyName = propertyInfo.Name;
            var typeName = propertyInfo.PropertyType.FullName;

            if (Serializers.ContainsKey(propertyName))
                return Serializers[propertyName]
                    .DynamicInvoke(propertyInfo.GetValue(obj)).ToString();

            if (Serializers.ContainsKey(typeName))
                return Serializers[typeName]
                    .DynamicInvoke(propertyInfo.GetValue(obj)).ToString();

            if (Cultures.ContainsKey(typeName))
            {
                var culture = Cultures[typeName];
                var tmp = propertyInfo.GetValue(obj) as double?;
                return tmp?.ToString("F", culture);
            }

            if (TrimLenghts.ContainsKey(propertyName))
            {
                var trimLenght = TrimLenghts[propertyName];
                return propertyInfo.GetValue(obj)
                    .ToString()
                    .Substring(0, trimLenght);
            }

            return null;
        }
    }
}