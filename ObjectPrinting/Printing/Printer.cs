using ObjectPrinting.Printing.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Printing
{
    internal class Printer<TOwner>
    {
        private readonly PrintingConfig<TOwner> _config;

        public Printer(PrintingConfig<TOwner> config)
        {
            _config = config;
        }

        public string PrintToString(object? obj, int nestingLevel, HashSet<object> path)
            => Print(_config.Rules.ForValue(obj), nestingLevel, path);

        private string Print(PrintDecision decision, int nestingLevel, HashSet<object> path)
        {
            var obj = decision.Value;

            if (obj == null)
                return "null" + Environment.NewLine;

            if (decision.Formatter != null)
                return decision.Formatter(obj) + Environment.NewLine;

            var type = obj.GetType();

            if (MemberAccessor.IsSimpleType(type))
                return obj + Environment.NewLine;

            if (!path.Add(obj))
                return $"{type.Name} (cycle){Environment.NewLine}";

            var result = obj is IEnumerable collection
                ? SerializeCollection(collection, nestingLevel, path)
                : SerializeMembers(obj, nestingLevel, path, type);

            path.Remove(obj);
            return result;
        }

        private string SerializeCollection(IEnumerable collection, int nestingLevel, HashSet<object> path)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(collection.GetType().Name);

            if (collection is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    var key = PrintToString(entry.Key, nestingLevel + 1, path);
                    key = key.Substring(0, key.Length - Environment.NewLine.Length);
                    sb.Append(identation + "[" + key + "] = " +
                              PrintToString(entry.Value, nestingLevel + 1, path));
                }
            }
            else
            {
                var index = 0;
                foreach (var item in collection)
                    sb.Append(identation + "[" + index++ + "] = " +
                              PrintToString(item, nestingLevel + 1, path));
            }

            return sb.ToString();
        }

        private string SerializeMembers(object obj, int nestingLevel, HashSet<object> path, Type type)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            sb.AppendLine(type.Name);

            foreach (var member in MemberAccessor.GetMembers(type))
            {
                var decision = _config.Rules.ForMember(member, obj);
                if (decision.IsExcluded)
                    continue;

                sb.Append(identation + member.Name + " = " + Print(decision, nestingLevel + 1, path));
            }
            return sb.ToString();
        }

    }
}
