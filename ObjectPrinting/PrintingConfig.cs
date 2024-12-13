using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly SerrializeConfig serrializeConfig;

        public PrintingConfig()
        {
            serrializeConfig = new SerrializeConfig();
        }

        public PrintingConfig(SerrializeConfig serrializeConfig)
        {
            this.serrializeConfig = serrializeConfig;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var objType = obj.GetType();

            if (serrializeConfig.TypeSerrializers.TryGetValue(objType, out var serrialize))
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

                if (serrializeConfig.ExcludedTypes.Contains(propType) ||
                    serrializeConfig.ExcludedProperties.Contains(propertyInfo))
                    continue;

                var contains = serrializeConfig.PropertySerrializers.TryGetValue(propertyInfo, out serrialize);

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

        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        public PrintingConfig<TOwner> Exclude<T>()
        {
            var config = new PrintingConfig<TOwner>(serrializeConfig);

            config.serrializeConfig.ExcludedTypes.Add(typeof(T));

            return config;
        }

        public PrintingConfig<TOwner> Exclude<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
        {
            var config = new PrintingConfig<TOwner>(serrializeConfig);

            var memberExp = memberSelector.Body as MemberExpression;
            var propInfo = memberExp.Member as PropertyInfo;

            config.serrializeConfig.ExcludedProperties.Add(propInfo);

            return config;
        }

        public PropertyPrintingConfig<TOwner, T> Print<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(serrializeConfig);
        }

        public PropertyPrintingConfig<TOwner, TProp> Print<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
        {
            var memberExp = memberSelector.Body as MemberExpression;
            var propInfo = memberExp.Member as PropertyInfo;

            return new PropertyPrintingConfig<TOwner, TProp>(serrializeConfig, propInfo);
        }
    }
}