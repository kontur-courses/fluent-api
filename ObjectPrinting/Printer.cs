using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting
{
    internal class Printer<TOwner>
    {
        private readonly PrintingConfig<TOwner> _config;

        public Printer(PrintingConfig<TOwner> config)
        {
            _config = config;
        }

        public string PrintToString(object obj, int nestingLevel, HashSet<object> path)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (_config._formaterType.TryGetValue(type, out var typeFormater))
                return typeFormater(obj) + Environment.NewLine;

            if (_config.finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            if (!path.Add(obj))
                return $"{type.Name} (cycle){Environment.NewLine}";

            var result = obj is IEnumerable collectino
                ? SerializeCollection(collectino, nestingLevel, path)
                : SerializeProperty(obj, nestingLevel, path);
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
                    var key = PrintToString(entry.Key, nestingLevel + 1, path).TrimEnd();
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

        private string SerializeProperty(object obj, int nestingLevel, HashSet<object> path)
        {
            var type = obj.GetType();
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            sb.AppendLine(type.Name);

            foreach (var member in PrintingConfig<TOwner>.GetMembers(type))
            {
                if (_config.IsMemberExcluded(member))
                    continue;

                var value = PrintingConfig<TOwner>.GetValue(member, obj);
                var printed = value != null && _config._formaterMember.TryGetValue(MemberKey.From(member), out var propertyFormater)
                    ? propertyFormater(value) + Environment.NewLine
                    : PrintToString(value, nestingLevel + 1, path);

                sb.Append(identation + member.Name + " = " + printed);

            }
            return sb.ToString();
        }

    }
}
