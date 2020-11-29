using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<Type> excludedTypes = new List<Type>();
        private readonly Dictionary<string, List<Type>> excludedFields = new Dictionary<string, List<Type>>();
        private readonly Dictionary<Type, CultureInfo> cultures = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<Type, Delegate> serializationsForType = new Dictionary<Type, Delegate>();
        private readonly Dictionary<string, Dictionary<Type, Delegate>> serializationForProperty =
            new Dictionary<string, Dictionary<Type, Delegate>>();
        private List<object> serializedObjects = new List<object>();

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var name = ((MemberExpression) memberSelector.Body).Member.Name;
            var declaringType = ((MemberExpression) memberSelector.Body).Member.DeclaringType;
            if (excludedFields.ContainsKey(name))
            {
                excludedFields[name].Add(declaringType);
            }
            else
            {
                excludedFields[name] = new List<Type>(){declaringType};
            }
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var name = ((MemberExpression) memberSelector.Body).Member.Name;
            var declaringType = ((MemberExpression) memberSelector.Body).Member.DeclaringType;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, name, declaringType);
        }

        public string PrintToString(TOwner obj)
        {
            return obj is ICollection collection ? PrintToStringICollectable(collection) : PrintToString(obj, 0);
        }
        
        public void AddSerializationForType<TPropType>(Type type, Func<TPropType, string> serializaton)
        {
            serializationsForType[type] = serializaton;
        }

        public void AddSerializationForProperty<TPropType>(string propertyName, Func<TPropType, string> serialization,
            Type declaringType)
        {
            if (!serializationForProperty.ContainsKey(propertyName))
            {
                serializationForProperty[propertyName] = new Dictionary<Type, Delegate>();
            }
            serializationForProperty[propertyName][declaringType] = serialization;
        }

        public void AddCultureForMember(Type type, CultureInfo culture)
        {
            cultures[type] = culture;
        }

        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (MemberIsAlreadySerilized(obj))
            {
                return "circle ref" + Environment.NewLine;
            }

            if (finalTypes.Contains(obj.GetType()))
            {
                return GetMemberWithCultureOrNull(obj) ?? obj + Environment.NewLine;
            }
            
            serializedObjects.Add(obj);
            return PrintObject(obj, nestingLevel);
        }

        private string PrintObject(object obj, int nestingLevel)
        {
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                PrintMember(propertyInfo.Name, propertyInfo.GetValue(obj),propertyInfo.PropertyType,
                    obj, sb, nestingLevel);
            }

            foreach (var fieldInfo in type.GetFields())
            {
                PrintMember(fieldInfo.Name, fieldInfo.GetValue(obj), fieldInfo.FieldType,
                    obj, sb, nestingLevel);
            }

            return sb.ToString();
        }

        private void PrintMember(string name, object value, Type type, object parent, StringBuilder sb, int nestingLevel)
        {
            if (excludedTypes.Contains(type) || 
                (excludedFields.ContainsKey(name) && excludedFields[name].Contains(parent.GetType())))
            {
                return;
            }
            
            var identation = new string('\t', nestingLevel + 1);
            sb.Append(identation + name + " = ");
            
            if (serializationForProperty.ContainsKey(name) && 
                serializationForProperty[name].ContainsKey(parent.GetType()))
            {
                var serializator = serializationForProperty[name][parent.GetType()];
                sb.Append(serializator.DynamicInvoke(value) + Environment.NewLine);
                return;
            }
            
            if (serializationsForType.TryGetValue(type, out var func))
            {
                sb.Append(func.DynamicInvoke(value) + Environment.NewLine);
                return;
            }

            sb.Append(PrintToString(value, nestingLevel + 1));
        }
        
        private string PrintToStringICollectable(IEnumerable collection)
        {
            var sb = new StringBuilder();
            var index = 0;
            foreach (var obj in collection)
            {
                sb.Append(index + ": ");
                sb.Append(PrintToString(obj, 1));
                index++;
            }

            return sb.ToString();
        }

        private string GetMemberWithCultureOrNull(object obj)
        {
            if (cultures.TryGetValue(obj.GetType(), out var culture))
            {
                return String.Format(culture, "{0}", obj);
            }

            return null;
        }

        private bool MemberIsAlreadySerilized(object obj)
        {
            foreach (var serilizedObj in serializedObjects)
            {
                if (ReferenceEquals(obj, serilizedObj))
                {
                    return true;
                }
            }

            return false;
        }
    }
}