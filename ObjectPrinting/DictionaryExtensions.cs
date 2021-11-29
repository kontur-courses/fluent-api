using System;
using System.Collections;
using System.Text;

namespace ObjectPrinting
{
    public static class DictionaryExtensions
    {
        public static string GetStringValueOrDefault(this IDictionary dictionary, string indent)
        {
            var sb = new StringBuilder(Environment.NewLine);
            foreach (var key in dictionary.Keys) sb.AppendLine($"{indent}{key} : {dictionary[key]}");

            return sb.ToString();
        }
    }
}