using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private int maxLength = -1;

        private readonly HashSet<object> objects = new HashSet<object>();

        private readonly List<Type> excludedTypes = new List<Type>();

        private readonly Dictionary<Type, CultureInfo> cultureInfos = new Dictionary<Type, CultureInfo>();

        private readonly TypePrintingConfig<TOwner> typePrintingConfig = new TypePrintingConfig<TOwner>();
        private readonly FieldPrintingConfig<TOwner> fieldPrintingConfig = new FieldPrintingConfig<TOwner>();

        private readonly Type[] finalTypes =
        {
            typeof(string), typeof(DateTime), typeof(TimeSpan)
        };

        public string PrintToString(TOwner obj)
        {
            objects.Clear();
            return maxLength == -1
                ? PrintToString(obj, 0)
                : PrintToString(obj, 0)[..maxLength];
        }

        private string PrintToString(object obj, int nestingLevel, bool newLine = true)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            if (objects.Contains(obj))
                return "cycled... No more this field" + Environment.NewLine;
            objects.Add(obj);

            var type = obj.GetType();

            if (type.IsPrimitive || finalTypes.Contains(type))
                return newLine ? obj + Environment.NewLine : obj.ToString();

            var sb = new StringBuilder(type.Name);
            if (type.IsGenericType)
                sb.Append("<" + type.GetProperty("Item")?.PropertyType.Name + ">");
            sb.Append(Environment.NewLine);

            if (obj is IEnumerable enumerable)
                return enumerable is IDictionary dictionary
                    ? sb.Append(SpecificTypesSerializer.PrintDictionary(dictionary, nestingLevel, PrintToString))
                        .ToString()
                    : sb.Append(SpecificTypesSerializer.PrintArray(enumerable, nestingLevel, PrintToString)).ToString();
            
            sb.Append(MembersAppend(obj, nestingLevel, type.GetFields()));
            sb.Append(MembersAppend(obj, nestingLevel, type.GetProperties()));
            return sb.ToString();
        }

        private StringBuilder MembersAppend(object obj, int nestingLevel, IEnumerable enumerable)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            foreach (var info in enumerable)
            {
                string prefix;
                Type type;
                object val;
                switch (info)
                {
                    case PropertyInfo propInfo:
                        prefix = identation + propInfo.Name + " = ";
                        type = propInfo.PropertyType;
                        val = propInfo.GetValue(obj);
                        break;
                    case FieldInfo fieldInfo:
                        prefix = identation + fieldInfo.Name + " = ";
                        type = fieldInfo.FieldType;
                        val = fieldInfo.GetValue(obj);
                        break;
                    default:
                        return new StringBuilder();
                }

                if (excludedTypes.Contains(type) || fieldPrintingConfig.ExcludedFields.Contains(info))
                    continue;

                if (info is MemberInfo memberInfo
                    && fieldPrintingConfig.FieldSerialize.TryGetValue(memberInfo, out var fieldSerializer))
                {
                    sb.Append(prefix +
                              PrintToString(fieldSerializer(val),
                                  nestingLevel + 1));
                    continue;
                }

                if (typePrintingConfig.TypeSerialize.TryGetValue(type, out var typeSerializer))
                {
                    sb.Append(prefix +
                              PrintToString(typeSerializer(val),
                                  nestingLevel + 1));
                    continue;
                }

                if (cultureInfos.TryGetValue(type, out var cultureInfo))
                {
                    sb.Append(prefix +
                              PrintToString(Convert.ChangeType(
                                      val,
                                      Type.GetTypeCode(val?.GetType()),
                                      cultureInfo
                                  )?.ToString(),
                                  nestingLevel + 1));
                    continue;
                }

                sb.Append(prefix +
                          PrintToString(val,
                              nestingLevel + 1));
            }

            return sb;
        }

        public TypePrintingConfig<TOwner> WithType<T>()
        {
            return typePrintingConfig.SwapContext<T>(this);
        }

        public PrintingConfig<TOwner> TrimString(int length)
        {
            maxLength = length;
            return this;
        }

        public PrintingConfig<TOwner> NumberCulture<T>(CultureInfo cultureInfo)
        {
            cultureInfos.Add(typeof(T), cultureInfo);
            return this;
        }

        public FieldPrintingConfig<TOwner> WithField<T>(Expression<Func<TOwner, T>> fieldInfo)
        {
            var expression = (MemberExpression)fieldInfo.Body;
            return fieldPrintingConfig.SwapContext(this, expression.Member);
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }
    }
}