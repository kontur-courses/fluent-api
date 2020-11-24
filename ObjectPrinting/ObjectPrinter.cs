using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting
{
    public class ObjectPrinter<TOwner>
    {
        public const int MaxSerializationDepth = 10;
        private readonly PrintingConfig<TOwner> config;
        private Dictionary<Type, List<object>> objectsCache;

        public ObjectPrinter()
        {
            config = new PrintingConfig<TOwner>();
        }

        public ObjectPrinter(PrintingConfig<TOwner> config)
        {
            this.config = config;
        }

        public string PrintToString(TOwner obj)
        {
            objectsCache = new Dictionary<Type, List<object>>();
            return PrintToString(obj, 0);
        }

        private string PrintToStringFinalType(object obj)
        {
            return obj + Environment.NewLine;
        }

        private string PrintToStringCollection(ICollection collection, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel);
            var sb = new StringBuilder();
            sb.AppendLine($"{indentation}[");
            foreach (var element in collection)
                sb.Append($"{indentation}\t{PrintToString(element, nestingLevel + 1)}");
            sb.AppendLine($"{indentation}]");
            return sb.ToString();
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return $"null{Environment.NewLine}";

            var type = obj.GetType();

            if (config.IsFinal(type))
                return PrintToStringFinalType(obj);

            if (!objectsCache.ContainsKey(type))
                objectsCache[type] = new List<object>();
            int index;
            if ((index = objectsCache[type].IndexOf(obj)) != -1)
                return $"{type.Name} {index + 1}{Environment.NewLine}";
            objectsCache[type].Add(obj);

            var sb = new StringBuilder();
            sb.AppendLine($"{type.Name} {objectsCache[type].Count}");

            if (nestingLevel == MaxSerializationDepth - 1)
                return sb.ToString();

            if (obj is ICollection)
            {
                sb.Append(PrintToStringCollection((ICollection) obj, nestingLevel));
                return sb.ToString();
            }

            var indentation = new string('\t', nestingLevel + 1);
            foreach (var propInfo in type.GetProperties())
            {
                if (config.IsExcluded(propInfo))
                    continue;
                if (config.TryGetCustomPrinter(propInfo, out var printer))
                    sb.AppendLine($"{indentation}{propInfo.Name} = {printer(propInfo.GetValue(obj))}");
                else
                    sb.Append($"{indentation}{propInfo.Name}" +
                              " = " +
                              PrintToString(propInfo.GetValue(obj), nestingLevel + 1));
            }

            return sb.ToString();
        }
    }
}