using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Tests;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<object> objects = new HashSet<object>();

        private readonly List<Type> excludedTypes = new List<Type>();

        protected MemberInfo FieldInfo;

        protected readonly List<MemberInfo> ExcludedFields = new List<MemberInfo>();

        protected readonly Dictionary<Type, CultureInfo> CultureInfos = new Dictionary<Type, CultureInfo>();

        private readonly Dictionary<Type, Func<object, string>> typeSerialize
            = new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<MemberInfo, Func<object, string>> fieldSerialize
            = new Dictionary<MemberInfo, Func<object, string>>();

        protected int MaxLength = -1;

        public PrintingConfig()
        {
        }

        protected PrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            excludedTypes = printingConfig.excludedTypes;
            FieldInfo = printingConfig.FieldInfo;
            ExcludedFields = printingConfig.ExcludedFields;
            CultureInfos = printingConfig.CultureInfos;
            typeSerialize = printingConfig.typeSerialize;
            fieldSerialize = printingConfig.fieldSerialize;
            MaxLength = printingConfig.MaxLength;
        }

        protected void SetSerializer(Type targetType, Func<object, string> serializer)
        {
            typeSerialize.Add(targetType, serializer);
        }

        protected void SetSerializer(MemberInfo targetInfo, Func<object, string> serializer)
        {
            fieldSerialize.Add(targetInfo, serializer);
        }

        public string PrintToString(TOwner obj)
        {
            objects.Clear();
            return MaxLength == -1
                ? PrintToString(obj, 0)
                : PrintToString(obj, 0)[..MaxLength];
        }

        private string PrintToString(object obj, int nestingLevel, bool newLine = true)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            if (objects.Contains(obj))
                return "cycled... No more this field" + Environment.NewLine;
            objects.Add(obj);

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return newLine ? obj + Environment.NewLine : obj.ToString();

            var sb = new StringBuilder(obj.GetType().Name);
            if (obj.GetType().IsGenericType)
                sb.Append("<" + obj.GetType().GetProperty("Item")?.PropertyType.Name + ">");
            sb.Append(Environment.NewLine);

            if (obj is IEnumerable enumerable)
                return enumerable is IDictionary dictionary
                    ? sb.Append(PrintDictionary(dictionary, nestingLevel)).ToString()
                    : sb.Append(PrintArray(enumerable, nestingLevel)).ToString();

            return sb.Append(PropertiesAppend(obj, nestingLevel)).ToString();
        }

        private StringBuilder PropertiesAppend(object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();

            foreach (var propertyInfo in type.GetProperties())
            {
                var prefix = identation + propertyInfo.Name + " = ";

                var propertyType = propertyInfo.PropertyType;

                if (excludedTypes.Contains(propertyType) || ExcludedFields.Contains(propertyInfo))
                    continue;

                if (fieldSerialize.TryGetValue(propertyInfo, out var fieldSerializer))
                {
                    sb.Append(prefix +
                              PrintToString(fieldSerializer(propertyInfo.GetValue(obj)),
                                  nestingLevel + 1));
                    continue;
                }

                if (typeSerialize.TryGetValue(propertyType, out var typeSerializer))
                {
                    sb.Append(prefix +
                              PrintToString(typeSerializer(propertyInfo.GetValue(obj)),
                                  nestingLevel + 1));
                    continue;
                }

                if (CultureInfos.TryGetValue(propertyType, out var cultureInfo))
                {
                    sb.Append(prefix +
                              PrintToString(Convert.ChangeType(
                                      propertyInfo.GetValue(obj),
                                      Type.GetTypeCode(propertyInfo.GetValue(obj)?.GetType()),
                                      cultureInfo
                                  )?.ToString(),
                                  nestingLevel + 1));
                    continue;
                }

                sb.Append(prefix +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }

            return sb;
        }

        private string PrintArray(IEnumerable enumerable, int nestingLevel)
        {
            return PrintEnumerable(enumerable, nestingLevel,
                el => PrintToString(el, nestingLevel + 1));
        }

        private string PrintDictionary(IDictionary dictionary, int nestingLevel)
        {
            return PrintEnumerable(dictionary.Keys, nestingLevel,
                kvp => PrintToString(kvp, nestingLevel + 2, false) +
                       " : " + PrintToString(dictionary[kvp], nestingLevel + 2));
        }

        private string PrintEnumerable(IEnumerable enumerable, int nestingLevel, Func<object, string> serializeElement)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            var index = 0;
            foreach (var el in enumerable)
                sb.Append(identation + "[" + index++ + "] = " + serializeElement(el));

            return sb.ToString();
        }

        public TypePrintingConfig<TOwner, T> WithType<T>()
        {
            return new TypePrintingConfig<TOwner, T>(this);
        }

        public FieldPrintingConfig<TOwner, T> WithField<T>(Expression<Func<Person, T>> fieldInfo)
        {
            var expression = (MemberExpression)fieldInfo.Body;
            FieldInfo = expression.Member;

            return new FieldPrintingConfig<TOwner, T>(this);
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }
    }
}