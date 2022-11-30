using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private HashSet<Type> removedTypes = new HashSet<Type>();
        private Dictionary<Type, CultureInfo> cultures = new Dictionary<Type, CultureInfo>();
        private Dictionary<Type, Func<object, string>> alterSerialize = new Dictionary<Type, Func<object, string>>();
        private HashSet<PropertyInfo> excludes = new HashSet<PropertyInfo>();
        private Dictionary<PropertyInfo, int> stringsToCrop = new Dictionary<PropertyInfo, int>();
        private Dictionary<object, int> printedObjects = new Dictionary<object, int>();
        private Dictionary<PropertyInfo, Func<object, string>> alterSerializeProp =
            new Dictionary<PropertyInfo, Func<object, string>>();

        private HashSet<Type> additionalFinalTypes = new HashSet<Type>
        {
            typeof(string), typeof(DateTime), typeof(TimeSpan)
        };

        Dictionary<PropertyInfo, Func<object, string>> IPrintingConfig<TOwner>.PropertyRules => alterSerializeProp;
        Dictionary<PropertyInfo, int> IPrintingConfig<TOwner>.StringsToCrop => stringsToCrop;

        public PrintingConfig<TOwner> SerializeType<T>(Func<T, string> func)
        {
            alterSerialize.Add(typeof(T), x => func((T)x));
            return this;
        }

        public PropertyConfig<TOwner, TOwnerType> ConfigureProperty<TOwnerType>(
            Expression<Func<TOwner, TOwnerType>> expression)
        {
            var tType = typeof(TOwnerType);
            var property = ((MemberExpression)expression.Body).Member as PropertyInfo;
            return new PropertyConfig<TOwner, TOwnerType>(this, property);
        }

        public PrintingConfig<TOwner> ExcludeProperty<TOwnerType>(Expression<Func<TOwner, TOwnerType>> func)
        {
            var property = ((MemberExpression)func.Body).Member as PropertyInfo;
            excludes.Add(property);
            return this;
        }

        public PrintingConfig<TOwner> ExcludeType<T>()
        {
            var tType = typeof(T);
            removedTypes.Add(tType);
            return this;
        }

        public PrintingConfig<TOwner> SetCulture<T>(CultureInfo culture) where T : struct, IComparable<T>, IFormattable
        {
            cultures.Add(typeof(T), culture);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            printedObjects.Clear();
            return PrintToString(obj, 0);
        }

        public string PrintListToString(IList<TOwner> objList)
        {
            var sb = new StringBuilder();
            foreach (var obj in objList)
                sb.Append(PrintToString(obj));

            return sb.ToString();
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return nestingLevel == 0 ? "null" + Environment.NewLine : "null";

            var type = obj.GetType();
            if (printedObjects.ContainsKey(obj))
                return type.Name + $" (On nesting level: {printedObjects[obj]})";

            if (!type.IsValueType)
                printedObjects.Add(obj, nestingLevel);

            if (removedTypes.Contains(type))
                return nestingLevel == 0 ? "" + Environment.NewLine : "";

            if (alterSerialize.ContainsKey(type))
                return nestingLevel == 0 ? alterSerialize[type](obj) + Environment.NewLine : alterSerialize[type](obj);

            if (type.IsPrimitive || additionalFinalTypes.Contains(type))
                return PrintFinalType(obj, type, nestingLevel);


            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (removedTypes.Contains(propertyInfo.PropertyType) || excludes.Contains(propertyInfo))
                    continue;

                var result = PrintProperty(obj, propertyInfo, nestingLevel);

                sb.Append(identation + propertyInfo.Name + " = " + result + Environment.NewLine);
            }

            return nestingLevel == 0 ? sb.ToString() : sb.ToString().TrimEnd();
        }

        private string PrintProperty(object obj, PropertyInfo propertyInfo, int nestingLevel)
        {
            var result = alterSerializeProp.ContainsKey(propertyInfo)
                ? alterSerializeProp[propertyInfo](propertyInfo.GetValue(obj))
                : PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);

            if (stringsToCrop.ContainsKey(propertyInfo))
                result = result.Length > stringsToCrop[propertyInfo]
                    ? result[..(stringsToCrop[propertyInfo])]
                    : result;
            return result;
        }

        private string PrintFinalType(object obj, Type type, int nestingLevel)
        {
            var textResult = cultures.ContainsKey(type)
                ? ((IFormattable)obj).ToString(null, cultures[type])
                : obj.ToString();

            if (nestingLevel == 0)
                textResult += Environment.NewLine;
            return textResult;
        }
    }

    public interface IPrintingConfig<TOwner>
    {
        Dictionary<PropertyInfo, Func<object, string>> PropertyRules { get; }
        Dictionary<PropertyInfo, int> StringsToCrop { get; }
    }
}