using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Policy;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        public PrintingConfig()
        {
            typeExclusions = new HashSet<Type>();
            propertyExclusions = new HashSet<MemberInfo>();
            typePrintingFunctions = new Dictionary<Type, Delegate>();
            typeCultures = new Dictionary<Type, CultureInfo>();
            propertyPrintingFunctions = new Dictionary<MemberInfo, Delegate>();
            propertyStringsLength = new Dictionary<MemberInfo, int>();
        }

        private readonly HashSet<Type> typeExclusions;
        private readonly HashSet<MemberInfo> propertyExclusions;
        private readonly Dictionary<Type, Delegate> typePrintingFunctions;
        private readonly Dictionary<Type, CultureInfo> typeCultures;
        private readonly Dictionary<MemberInfo, Delegate> propertyPrintingFunctions;
        private readonly Dictionary<MemberInfo, int> propertyStringsLength;
        
        Dictionary<Type, Delegate> IPrintingConfig.TypePrintingFunctions => typePrintingFunctions;
        Dictionary<Type, CultureInfo> IPrintingConfig.TypeCultures => typeCultures;
        Dictionary<MemberInfo, Delegate> IPrintingConfig.PropertyPrintingFunctions => propertyPrintingFunctions;
        Dictionary<MemberInfo, int> IPrintingConfig.PropertyStringsLength => propertyStringsLength;
        
        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string), typeof(byte), typeof(short),
            typeof(DateTime), typeof(TimeSpan)
        };


        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }
        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
                propertyExclusions.Add(memberExpression.Member);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typeExclusions.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string MemberToString(object value, MemberInfo memberInfo, int nestingLevel, List<object> serializeObjects)
        {
            var test =  memberInfo.Module;
            string valueString;
            if (propertyPrintingFunctions.TryGetValue(memberInfo, out var printingFunc)) 
                valueString = (string) printingFunc.DynamicInvoke(value);
            else
                valueString = PrintToString(value, nestingLevel + 1, serializeObjects);
            if (propertyStringsLength.TryGetValue(memberInfo, out var length))
                valueString = valueString.Substring(0, length);
            return  new string('\t', nestingLevel + 1) + memberInfo.Name + " = " + valueString;
        }

        private string PrintToString(object obj, int nestingLevel, List<object> serializeObjects = null)
        {
            if (obj == null)
                return "null";
            serializeObjects = serializeObjects ?? new List<object>();

            if (typePrintingFunctions.TryGetValue(obj.GetType(), out var printingFunc))
                return (string) printingFunc.DynamicInvoke(obj);

            if (typeCultures.TryGetValue(obj.GetType(), out  var cultureInfo) && obj is IFormattable formattable)
                return formattable.ToString((string) null, cultureInfo);
            
            if (finalTypes.Contains(obj.GetType()))
                return obj.ToString();

            var sb = new StringBuilder();
            var type = obj.GetType();
            
            if (serializeObjects.Any(obj1 => object.ReferenceEquals(obj, obj1)))
                return type.Name;
            serializeObjects.Add(obj);
            
            sb.AppendLine(type.Name);

            if (obj is IEnumerable iEnumerable)
            {
                foreach (var i in iEnumerable)
                    sb.AppendLine( new string('\t', nestingLevel + 1) + PrintToString(i, nestingLevel + 1, serializeObjects));
                return sb.ToString();
            }

            foreach (var propertyInfo in type.GetProperties())
               if (!typeExclusions.Contains(propertyInfo.PropertyType) && !propertyExclusions.Contains(propertyInfo))
                    sb.AppendLine(MemberToString(propertyInfo.GetValue(obj), propertyInfo, nestingLevel, serializeObjects));
            foreach (var fieldInfo in type.GetFields())
                if ((obj.GetType().IsClass || !fieldInfo.IsStatic) && !typeExclusions.Contains(fieldInfo.FieldType) &&
                    fieldInfo.IsPublic)
                    sb.AppendLine(MemberToString(fieldInfo.GetValue(obj), fieldInfo, nestingLevel, serializeObjects));
            return sb.ToString();
        }
    }
}