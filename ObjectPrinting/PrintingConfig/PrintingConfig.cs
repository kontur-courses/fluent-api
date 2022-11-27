using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.PrintingConfig
{
    public class PrintingConfig<TOwner>
    {
        protected readonly Dictionary<Type, Delegate> TypeSerializers;
        protected readonly Dictionary<PropertyInfo, Delegate> PropertySerializers;
        private readonly Dictionary<Type, CultureInfo> typeCultures;
        private readonly Dictionary<PropertyInfo, CultureInfo> propertyCultures;
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<PropertyInfo> excludedProperties;
        private readonly int? maxStringLength;
        private readonly Dictionary<PropertyInfo, int> maxPropertyLength;
        
        private readonly Type[] finalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        internal PrintingConfig(int maxStringLength, PrintingConfig<TOwner> parent) : this(parent)
        {
            this.maxStringLength = maxStringLength;
        }

        internal PrintingConfig(PropertyInfo property, int maxLength, PrintingConfig<TOwner> parent) : this(parent)
        {
            maxPropertyLength[property] = maxLength;
        }
        
        internal PrintingConfig(Type type, CultureInfo cultureInfo, PrintingConfig<TOwner> parent) : this(parent)
        {
            typeCultures[type] = cultureInfo;
        }

        internal PrintingConfig(PropertyInfo property, CultureInfo cultureInfo, PrintingConfig<TOwner> parent) : this(parent)
        {
            propertyCultures[property] = cultureInfo;
        }

        protected PrintingConfig(PrintingConfig<TOwner> parent)
        {
            excludedTypes = parent.excludedTypes;
            excludedProperties = parent.excludedProperties;
            TypeSerializers = parent.TypeSerializers;
            PropertySerializers = parent.PropertySerializers;
            maxPropertyLength = parent.maxPropertyLength;
            maxStringLength = parent.maxStringLength;
            propertyCultures = parent.propertyCultures;
            typeCultures = parent.typeCultures;
        }
        
        public PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<PropertyInfo>();
            TypeSerializers = new Dictionary<Type, Delegate>();
            PropertySerializers = new Dictionary<PropertyInfo, Delegate>();
            maxPropertyLength = new Dictionary<PropertyInfo, int>();
            propertyCultures = new Dictionary<PropertyInfo, CultureInfo>();
            typeCultures = new Dictionary<Type, CultureInfo>();
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel, Stack<List<object>> stack = null)
        {
            stack ??= new Stack<List<object>>();
            var list = new List<object> { obj };

            if (obj == null)
                return "null" + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
            {
                if (typeCultures.ContainsKey(obj.GetType()))
                    return ((IFormattable)obj).ToString("", typeCultures[obj.GetType()]) + Environment.NewLine;
                
                return obj + Environment.NewLine;
            }
            
            var sb = new StringBuilder();
            sb.AppendLine(obj.GetType().Name);
            sb.Append(SerializeProperties(obj, nestingLevel, stack, list));
            if (obj is ICollection collection)
            {
                sb.Append('\t', nestingLevel + 1).AppendLine("Elements");
                sb.Append(SerializeElements(collection, nestingLevel + 1, stack, list));
            }
            
            return sb.ToString();
        }

        private string SerializeElements(ICollection collection, int nestingLevel, Stack<List<object>> stack, List<object> list)
        {
            var sb = new StringBuilder();
            var i = 0;
            var indentation = new string('\t', nestingLevel + 1);
            list.Add(collection);
            stack.Push(list);

            if (collection is IDictionary dictionary)
                foreach (DictionaryEntry entry in dictionary)
                    sb.Append(GetLine($"[{entry.Key}]", indentation, 
                        PrintToString(entry.Value, nestingLevel + 1, stack)));
            else
                foreach (var obj in collection)
                    sb.Append(GetLine($"[{i++}]", indentation, 
                        PrintToString(obj, nestingLevel + 1, stack)));

            stack.Pop();
            return sb.ToString();
        }

        private string SerializeProperties(object obj, int nestingLevel, Stack<List<object>> stack, List<object> list)
        {
            var type = obj.GetType();
            var sb = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.GetIndexParameters().Length > 0)
                    continue;
                
                string serialized;
                string line;
                var value = propertyInfo.GetValue(obj);
                if (stack.Any(l => l.Any(o => ReferenceEquals(o, value))))
                {
                    line = GetLine(propertyInfo.Name, indentation, "<Cyclical reference>");
                    sb.AppendLine(line);
                    continue;
                }

                list.Add(value);

                if (excludedProperties.Contains(propertyInfo))
                    continue;

                if (PropertySerializers.ContainsKey(propertyInfo))
                {
                    serialized = Serialize(value, propertyInfo);
                    line = GetLine(propertyInfo.Name, indentation, serialized);
                    sb.AppendLine(line);
                    continue;
                }

                if (excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;

                if (TypeSerializers.ContainsKey(propertyInfo.PropertyType))
                {
                    serialized = Serialize(value, propertyInfo.PropertyType);
                    line = GetLine(propertyInfo.Name, indentation, serialized);
                    sb.AppendLine(line);
                    continue;
                }

                stack.Push(list);
                serialized = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1, stack);
                if (maxPropertyLength.ContainsKey(propertyInfo))
                    serialized = CutString(serialized, maxPropertyLength[propertyInfo]);
                else if (propertyInfo.PropertyType == typeof(string) && maxStringLength != null)
                    serialized = CutString(serialized, maxStringLength.Value);
                line = GetLine(propertyInfo.Name, indentation, serialized);
                sb.AppendLine(line);
                stack.Pop();
            }

            return sb.ToString();
        }

        public PrintingConfig<TOwner> ExcludeProperty<TField>(Expression<Func<TOwner, TField>> field)
        {
            if (!(field.Body is MemberExpression member))
                throw new ArgumentException($"Expected property, got {field.Body} instead");
            
            var propertyInfo = member.Member as PropertyInfo;
            excludedProperties.Add(propertyInfo);
            return this;
        }
        
        public PrintingConfig<TOwner> ExcludeType<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PropertyPrintingConfig<TOwner, TField> SerializeProperty<TField>(Expression<Func<TOwner, TField>> field)
        {
            if (!(field.Body is MemberExpression member))
                throw new ArgumentException($"Expected property, got {field.Body} instead.");
            
            var propertyInfo = member.Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TField>(this, propertyInfo);
        }

        public TypePrintingConfig<TOwner, T> SerializeType<T>()
        {
            return new TypePrintingConfig<TOwner, T>(this);
        }

        private string Serialize(object value, PropertyInfo propertyInfo)
        {
            var str = (string)PropertySerializers[propertyInfo].DynamicInvoke(value);
            if (str == null)
                return "null";
            
            if (maxPropertyLength.ContainsKey(propertyInfo))
                return CutString(str, maxPropertyLength[propertyInfo]);

            return str;
        }

        private string Serialize(object value, Type type)
        {
            var str = (string)TypeSerializers[type].DynamicInvoke(value);
            if (str == null)
                return "null";
            
            if (type == typeof(string) && maxStringLength != null)
                return CutString(str, maxStringLength.Value);

            return str;
        }
        
        private string GetLine(string name, string indentation, string value)
        {
            return indentation + name + " = " + value;
        }
        
        private string CutString(string value, int length)
        {
            if (value.Length <= length) return value;
            
            var charArray = new char[length + 3];
            var i = 0;
            for (; i < length; i++)
                charArray[i] = value[i];
            for (; i < length + 3; i++)
                charArray[i] = '.';
            
            return new string(charArray);
        }
    }
}