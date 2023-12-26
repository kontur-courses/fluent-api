using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public delegate bool Predicate(PropertyInfo info);
    public partial class PrintingConfig<TOwner>
    {
        private Configuration configuration;

        private readonly Type[] finalTypes;

        public PrintingConfig()
        {
            configuration = new Configuration();

            finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
        }

        public PrintingConfig<TOwner> Configure(Action<Options> configure)
        {
            var opts = new Options();

            configure(opts);

            configuration.Options = opts;

            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null";

            if (obj.GetType() == typeof(string) && configuration.Options.MaxStringLength != -1)
            {
                var str = (string)obj;
                var length = Math.Min(configuration.Options.MaxStringLength, str.Length);
                return str.Substring(0, length);
            }

            if (finalTypes.Contains(obj.GetType()))
                return Convert.ToString(obj, configuration.Options.CultureInfo);

            if (nestingLevel > configuration.Options.MaxRecursionDepth)
                return "...";

            if (obj is ICollection)
                return PrintCollection(obj);

            var identation = new string('\t', nestingLevel + 1);
            var type = obj.GetType();
            var lines = new List<string>() { type.Name};

            var properties = type.GetProperties().Where(p => !ToExclude(p)).ToList();

            foreach (var propertyInfo in properties)
            {
                var serialized = "";
                if (TryGetConfig(propertyInfo, out var serializer))
                    serialized = serializer(propertyInfo.GetValue(obj));
                else
                    serialized = PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1);

                var line = string.Format("{0}{1} = {2}", 
                    identation, propertyInfo.Name, serialized);

                lines.Add(line);
            }

            return string.Join("\n", lines);
        }

        private string PrintCollection(object obj)
        {
            if (obj is IDictionary)
            {
                var list = new List<string>();
                foreach (DictionaryEntry entry in obj as IDictionary)
                {
                    list.Add($"{PrintToString(entry.Key, 0)} : {PrintToString(entry.Value, 0)}");
                }
                return $"{{{string.Join(",", list)}}}";
            }
            else
            {
                var collection = (obj as ICollection).Cast<object>();
                var str = string.Join(",", collection.Select(x => PrintToString(x, 0)));
                return $"[{str}]";
            }
        }

        private bool TryGetConfig(PropertyInfo info, out Func<object, string> serializer)
        {
            serializer = configuration.Serializers
                .FirstOrDefault(x => x.predicate(info))
                .serializer;
            return !(serializer is null);
        }

        private bool ToExclude(PropertyInfo info)
        {
            return configuration.ToExclude.Any(predicate => predicate(info));
        }
    }
}