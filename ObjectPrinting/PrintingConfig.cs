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
        private readonly HashSet<object> objects = new HashSet<object>();

        private readonly List<Type> excludedTypes = new List<Type>();

        private readonly List<MemberInfo> excludedFields = new List<MemberInfo>();

        private readonly Dictionary<Type, CultureInfo> cultureInfos = new Dictionary<Type, CultureInfo>();

        private readonly Dictionary<Type, Func<object, string>> typesSerialize
            = new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<MemberInfo, Func<object, string>> fieldsSerialize
            = new Dictionary<MemberInfo, Func<object, string>>();

        private readonly Dictionary<MemberInfo, int> fieldsLength = new Dictionary<MemberInfo, int>();

        private readonly Type[] finalTypes =
        {
            typeof(string), typeof(DateTime), typeof(TimeSpan)
        };

        public string PrintToString(TOwner obj)
        {
            objects.Clear();
            return PrintToString(obj, 0);
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

            return sb.Append(MembersAppend(obj, nestingLevel)).ToString();
        }

        private StringBuilder MembersAppend(object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            foreach (var info in obj.GetType().GetMembers())
            {
                var prefix = identation + info.Name + " = ";
                Type type;
                object val;
                switch (info)
                {
                    case PropertyInfo propInfo:
                        type = propInfo.PropertyType;
                        val = propInfo.GetValue(obj);
                        break;
                    case FieldInfo fieldInfo:
                        type = fieldInfo.FieldType;
                        val = fieldInfo.GetValue(obj);
                        break;
                    default:
                        continue;
                }

                if (excludedTypes.Contains(type) || excludedFields.Contains(info))
                    continue;

                var len = fieldsLength.TryGetValue(info, out var value) ? value : -1;

                string tmpStr;
                if (fieldsSerialize.TryGetValue(info, out var fieldSerializer))
                {
                    tmpStr = prefix + PrintToString(fieldSerializer(val), nestingLevel + 1);
                    sb.Append(len != -1 ? tmpStr[..len] : tmpStr);
                    continue;
                }

                if (typesSerialize.TryGetValue(type, out var typeSerializer))
                {
                    tmpStr = prefix + PrintToString(typeSerializer(val), nestingLevel + 1);
                    sb.Append(len != -1 ? tmpStr[..len] : tmpStr);
                    continue;
                }

                if (cultureInfos.TryGetValue(type, out var cultureInfo))
                {
                    tmpStr = prefix +
                             PrintToString(Convert.ChangeType(
                                 val,
                                 Type.GetTypeCode(val?.GetType()),
                                 cultureInfo
                             )?.ToString(), nestingLevel + 1);
                    sb.Append(len != -1 ? tmpStr[..len] : tmpStr);
                    continue;
                }

                tmpStr = prefix + PrintToString(val, nestingLevel + 1);
                sb.Append(len != -1 ? tmpStr[..len] : tmpStr);
            }

            return sb;
        }

        public TypePrintingConfig<TOwner, T> WithType<T>()
        {
            return new TypePrintingConfig<TOwner, T>(typesSerialize, this);
        }

        public PrintingConfig<TOwner> NumberCulture<T>(CultureInfo cultureInfo)
            where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
        {
            cultureInfos.Add(typeof(T), cultureInfo);
            return this;
        }

        public PrintingConfig<TOwner> TrimString<T>(Expression<Func<TOwner, T>> fieldInfo, int length)
            where T : IEquatable<string?>
        {
            var expression = (MemberExpression)fieldInfo.Body;
            fieldsLength.Add(expression.Member, length);
            return this;
        }

        public FieldPrintingConfig<TOwner> WithField<T>(Expression<Func<TOwner, T>> fieldInfo)
        {
            var expression = (MemberExpression)fieldInfo.Body;
            return new FieldPrintingConfig<TOwner>(excludedFields, fieldsSerialize, this, expression.Member);
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }
    }
}