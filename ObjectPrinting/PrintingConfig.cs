using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly SerrializeConfig serrializeConfig;
        private HashSet<object> serializedObjects;

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
            if (obj == null)
                return "null" + Environment.NewLine;

            serializedObjects.Add(obj);

            var objType = obj.GetType();

            if (serrializeConfig.TryGetSerializer(objType, out var serrialize))
                return serrialize.DynamicInvoke(obj) + Environment.NewLine;

            if (obj is ICollection collection)
                return SerializeCollection(collection, nestingLevel);

            var identation = new string('\t', nestingLevel + 1);

            var sb = new StringBuilder();

            sb.AppendLine(objType.Name);

            foreach (var propertyInfo in objType.GetProperties())
            {
                var propType = propertyInfo.PropertyType;
                var propValue = propertyInfo.GetValue(obj);

                if (serrializeConfig.IsExcludedType(propType) ||
                    serrializeConfig.IsExcludedMember(propertyInfo) ||
                    serializedObjects.Contains(propValue)
                    )
                    continue;

                var contains = serrializeConfig.TryGetSerializer(propertyInfo, out serrialize);

                var objStr = contains ?
                    serrialize.DynamicInvoke(propValue) + Environment.NewLine :
                    PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);

                sb.Append(identation + propertyInfo.Name + " = " + objStr);
            }

            foreach (var propertyInfo in objType.GetFields())
            {
                var propType = propertyInfo.DeclaringType;
                var propValue = propertyInfo.GetValue(obj);

                if (serrializeConfig.IsExcludedType(propType) ||
                    serrializeConfig.IsExcludedMember(propertyInfo) ||
                    serializedObjects.Contains(propValue)
                    )
                    continue;

                var contains = serrializeConfig.TryGetSerializer(propertyInfo, out serrialize);

                var objStr = contains ?
                    serrialize.DynamicInvoke(propValue) + Environment.NewLine :
                    PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);

                sb.Append(identation + propertyInfo.Name + " = " + objStr);
            }

            return sb.ToString();
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