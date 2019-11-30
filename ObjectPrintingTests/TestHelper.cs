using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectPrintingTests
{
    public class TestHelper
    {
        public static string GetExpectedResult(Type type, Dictionary<string, string> members)
        {
            var builder = new StringBuilder();
            builder.AppendLine(type.Name);
            AppendLinesToBuilder(members.Select(member => $"\t{member.Key} = {member.Value}"), builder);

            return builder.ToString();
        }

        public static string GetExpectedResultForCollection(ICollection collection, string indentation = "\t")
        {
            var builder = new StringBuilder();
            builder.AppendLine(collection.GetType().Name);
            AppendLinesToBuilder(collection.Cast<object>().Select(element => $"{indentation}{element}"), builder);

            return builder.ToString();
        }

        public static string GetExpectedResultForCollection<T>(ICollection<T> collection, Func<T, string> print)
        {
            var builder = new StringBuilder();
            builder.AppendLine(collection.GetType().Name);
            AppendLinesToBuilder(collection.Select(element => $"\t{print(element)}"), builder);

            return builder.ToString();
        }

        private static void AppendLinesToBuilder(IEnumerable<string> lines, StringBuilder builder)
        {
            foreach (var line in lines)
            {
                builder.AppendLine(line);
            }
        }
    }
}