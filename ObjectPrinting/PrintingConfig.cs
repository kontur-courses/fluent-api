using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly SerrializeConfig serrializeConfig;
        private readonly HashSet<object> serializedObjects;

        public PrintingConfig()
        {
            serrializeConfig = new SerrializeConfig();
            serializedObjects = new HashSet<object>();
        }

        public PrintingConfig(SerrializeConfig serrializeConfig)
        {
            this.serrializeConfig = serrializeConfig;
            serializedObjects = new HashSet<object>();
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (serrializeConfig.TryGetSerializer(obj.GetType(), out var serrialize))
                return serrialize.DynamicInvoke(obj) + Environment.NewLine;

            if (obj is ICollection collection)
                return SerializeCollection(collection, nestingLevel);

            return PrintToStringObject(obj, nestingLevel);
        }

        private string PrintToStringObject(object obj, int nestingLevel)
        {
            var sb = new StringBuilder();

            sb.AppendLine(obj.GetType().Name);

            PrintToStringFields(sb, obj, nestingLevel);
            PrintToStringProperties(sb, obj, nestingLevel);

            return sb.ToString();
        }

        private void PrintToStringProperties(StringBuilder sb, object obj, int nestingLevel)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                var (objStr, ok) = PrintToStringMember(
                    property,
                    property.PropertyType,
                    property.GetValue(obj),
                    nestingLevel
                    );

                if (!ok)
                    continue;

                sb.Append(objStr);
            }
        }

        private void PrintToStringFields(StringBuilder sb, object obj, int nestingLevel)
        {
            foreach (var field in obj.GetType().GetFields())
            {
                var (objStr, ok) = PrintToStringMember(
                    field,
                    field.FieldType,
                    field.GetValue(obj),
                    nestingLevel
                    );

                if (!ok)
                    continue;

                sb.Append(objStr);
            }
        }

        private (string?, bool) PrintToStringMember(MemberInfo member, Type? type, object? value, int nestingLevel)
        {
            if (value == null)
                return (ToPrintingFormat(member.Name, "null" + Environment.NewLine, nestingLevel), true);

            if (type is null ||
                serrializeConfig.IsExcludedType(type) ||
                serrializeConfig.IsExcludedMember(member) ||
                serializedObjects.Contains(value)
                )
                return (null, false);

            if (!value.GetType().IsValueType)
                serializedObjects.Add(value);

            var contains = serrializeConfig.TryGetSerializer(member, out var serrialize);

            var objStr = contains ?
                serrialize.DynamicInvoke(value) + Environment.NewLine :
                PrintToString(value, nestingLevel + 1);

            return (ToPrintingFormat(member.Name, objStr, nestingLevel), true);
        }

        private string ToPrintingFormat(string memberName, string objStr, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);

            return identation + memberName + " = " + objStr;
        }

        private string SerializeCollection(ICollection collection, int nestingLevel)
        {
            if (collection is IDictionary dictionary)
                return SerializeDictionary(dictionary, nestingLevel);

            var sb = new StringBuilder();

            foreach (var item in collection)
                sb.Append(PrintToString(item, nestingLevel + 1).Trim() + " ");

            return $"[ {sb}]" + Environment.NewLine;
        }

        private string SerializeDictionary(IDictionary dictionary, int nestingLevel)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel);

            sb.AppendLine(identation + "{");

            foreach (var key in dictionary.Keys)
            {
                identation = new string('\t', nestingLevel + 1);

                sb.Append(identation + "[" + PrintToString(key, nestingLevel + 1).Trim() + " - ")
                    .Append(PrintToString(dictionary[key], 0).Trim())
                    .AppendLine("],");
            }

            return sb + "}" + Environment.NewLine;
        }

        public string PrintToString(TOwner obj)
        {
            serializedObjects.Add(obj);

            var result = PrintToString(obj, 0);

            serializedObjects.Clear();

            return result;
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            var config = new PrintingConfig<TOwner>(serrializeConfig);

            config.serrializeConfig.ExcludeType(typeof(T));

            return config;
        }

        public PrintingConfig<TOwner> Exclude<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
        {
            var config = new PrintingConfig<TOwner>(serrializeConfig);

            var memberExp = memberSelector.Body as MemberExpression;

            config.serrializeConfig.ExcludeMember(memberExp.Member);

            return config;
        }

        public PropertyPrintingConfig<TOwner, T> Print<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(serrializeConfig);
        }

        public PropertyPrintingConfig<TOwner, TProp> Print<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
        {
            var memberExp = memberSelector.Body as MemberExpression;

            return new PropertyPrintingConfig<TOwner, TProp>(serrializeConfig, memberExp.Member);
        }
    }
}