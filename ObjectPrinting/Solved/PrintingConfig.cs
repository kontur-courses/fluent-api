using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        private static readonly Type[] finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
        private readonly HashSet<Type> excludingTypes = new HashSet<Type>();
        private readonly HashSet<string> excludingFields = new HashSet<string>();
        private readonly Dictionary<Type, CultureInfo> cultures = new Dictionary<Type, CultureInfo>();
        private int? trimedString;
        private readonly Dictionary<string, int> fieldsTrim = new Dictionary<string, int>();
        private readonly HashSet<object> objects = new HashSet<object>();

        internal void AddFieldsTrim(string fullName, int length) => fieldsTrim[fullName] = length;

        internal void AddCulture(Type type, CultureInfo culture) => cultures[type] = culture;

        internal void TrimedString(int trimed) => trimedString = trimed;

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
            var s = memberSelector.GetFullNameProperty();
            excludingFields.Add(s);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            var result = PrintToString(obj, 0);
            return result;
        }

        private string PrintToString(object obj, int nestingLevel, string fullName = "")
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;
            var type = obj.GetType();

            if (finalTypes.Contains(type))
            {
                return GetStringFinalType();
            }

            objects.Add(obj);

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (!excludingTypes.Contains(propertyInfo.PropertyType) &&
                    !excludingFields.Contains(fullName + '.' + propertyInfo.Name))
                {
                    if(finalTypes.Contains(propertyInfo.DeclaringType) || 
                       !objects.Contains<object>(propertyInfo.GetValue(obj)))
                    sb.Append(identation + propertyInfo.Name + " = " +
                        PrintToString(propertyInfo.GetValue(obj),
                        nestingLevel + 1, fullName + '.' + propertyInfo.Name));
                }
            }
            return sb.ToString();

            string GetStringFinalType()
            {
                if (type == typeof(string))
                {
                    var result = obj as string;
                    int trimed = fieldsTrim.ContainsKey(fullName) ? fieldsTrim[fullName] : trimedString ?? result.Length;
                    var trimedResult = Math.Min(result.Length, trimed);
                    if (cultures.ContainsKey(type))
                        return result.ToString(cultures[type])[0..trimedResult] + Environment.NewLine;
                    return result[0..trimedResult] + Environment.NewLine;
                }
                if (cultures.ContainsKey(type))
                    return GetCulturesResult();
                return obj + Environment.NewLine;
            }

            string GetCulturesResult()
            {
                if(type == typeof(double))
                    return ((double)obj).ToString(cultures[type]);
                if (type == typeof(int))
                    return ((double)obj).ToString(cultures[type]);
                if (type == typeof(float))
                    return ((float)obj).ToString(cultures[type]);
                if (type == typeof(DateTime))
                    return ((DateTime)obj).ToString(cultures[type]);
                return obj.ToString();
            }
        }
    }
}