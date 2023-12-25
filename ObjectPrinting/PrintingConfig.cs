using System;
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
            return MaxLength == -1
                ? PrintToString(obj, 0)
                : PrintToString(obj, 0)[..MaxLength];
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
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
                var propertyType = propertyInfo.PropertyType;
                if (excludedTypes.Contains(propertyType))
                    continue;

                if (ExcludedFields.Contains(propertyInfo))
                    continue;

                if (typeSerialize.TryGetValue(propertyType, out var typeSerializer))
                {
                    var specificSerialized = typeSerializer(propertyInfo.GetValue(obj));

                    sb.Append(identation + propertyInfo.Name + " = " +
                              PrintToString(specificSerialized, nestingLevel + 1));
                }
                else if (fieldSerialize.TryGetValue(propertyInfo, out var fieldSerializer))
                {
                    var specificSerialized = fieldSerializer(propertyInfo.GetValue(obj));

                    sb.Append(identation + propertyInfo.Name + " = " +
                              PrintToString(specificSerialized, nestingLevel + 1));
                }
                else if (CultureInfos.TryGetValue(propertyType, out var cultureInfo))
                {
                    var culturedNum = Convert.ChangeType(
                        propertyInfo.GetValue(obj),
                        Type.GetTypeCode(propertyInfo.GetValue(obj)?.GetType()),
                        cultureInfo
                    )?.ToString();

                    sb.Append(identation + propertyInfo.Name + " = " +
                              PrintToString(culturedNum, nestingLevel + 1));
                }
                else
                    sb.Append(identation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj),
                                  nestingLevel + 1));
            }

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