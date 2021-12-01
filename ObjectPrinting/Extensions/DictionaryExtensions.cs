using System;
using System.Collections;
using System.Text;

namespace ObjectPrinting.Extensions
{
    public static class DictionaryExtensions
    {
        public static string GetStringValueOrDefault(this IDictionary dictionary, int indentLevel)
        {
            var sb = new StringBuilder(Environment.NewLine);

            if (dictionary == null)
                sb.AppendLine("null");
            else
                foreach (var key in dictionary.Keys)
                {
                    var indent = new string('\t', indentLevel);

                    if (dictionary[key] is IDictionary dict)
                    {
                        sb.Append($"{indent}{key} : IDictionary");
                        sb.AppendLine(dict.GetStringValueOrDefault(indentLevel + 1).TrimEnd());
                    }
                    else
                    {
                        sb.AppendLine($"{indent}{key} : {dictionary[key]}");
                    }
                }

            return sb.ToString();
        }
    }
}