using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectPrinting.Configuration;
using ObjectPrinting.Nodes;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes;
        private readonly ChildedNode<IPropertyConfigurator> configurationRoot;
        private readonly IDictionary<Type, IPropertyConfigurator> groupAppliedConfigurators;
        private readonly HashSet<object> alreadySerialized = new HashSet<object>();

        public PrintingConfig(Type[] finalTypes, ChildedNode<IPropertyConfigurator> configurationRoot,
            IDictionary<Type, IPropertyConfigurator> groupAppliedConfigurators)
        {
            this.finalTypes = finalTypes;
            this.configurationRoot = configurationRoot;
            this.groupAppliedConfigurators = groupAppliedConfigurators;
        }

        public string PrintToString(TOwner obj)
        {
            return SerializeObject(obj, new string[0], 0);
        }

        private string SerializeObject(object? obj, string[] path, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (finalTypes.Contains(type))
                return obj.ToString() + Environment.NewLine;

            if (alreadySerialized.Contains(obj))
                return string.Empty;
            alreadySerialized.Add(obj);

            if (obj is IDictionary dictionary)
            {
                return $"[{dictionary.Count}]" + Environment.NewLine +
                       SerializeDictionary(dictionary, path, nestingLevel + 1);
            }

            if (obj is ICollection collection)
            {
                return $"[{collection.Count}]" + Environment.NewLine +
                       SerializeCollection(collection, path, nestingLevel + 1);
            }

            var sb = new StringBuilder();
            sb.AppendLine(type.Name);

            var serializedProperties = SerializationTarget.EnumerateFrom(type)
                .Select(t => SerializeProperty(t.MemberName, path, t.GetValue(obj), t.MemberType, nestingLevel + 1));

            sb.AppendJoin(string.Empty, serializedProperties);
            return sb.ToString();
        }

        private string SerializeProperty(string propertyName, string[] path, object? value, Type type, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var serialized = value == null
                ? "null"
                : GetConfiguratorOrDefault(propertyName, path, type)?.AppliedSerializer.Serialize(value) ??
                  SerializeObject(value, path.Append(propertyName).ToArray(), nestingLevel + 1);

            return string.IsNullOrEmpty(serialized)
                ? string.Empty
                : $"{indentation}{propertyName} = " +
                  (serialized.EndsWith(Environment.NewLine)
                      ? serialized
                      : serialized + Environment.NewLine);
        }

        private string SerializeCollection(ICollection collection, string[] path, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var currentIndex = 0;
            foreach (var item in collection)
            {
                var prefix = indentation + $"[{currentIndex}]:{item?.GetType().Name ?? "null"} = ";
                sb.Append(prefix + SerializeObject(
                    item,
                    path.Append(currentIndex.ToString()).ToArray(),
                    nestingLevel + 1));
                currentIndex++;
            }

            return sb.ToString();
        }

        private string SerializeDictionary(IDictionary dictionary, string[] path, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            foreach (var key in dictionary.Keys)
            {
                var keyString = key.ToString(); // TODO serialize keys and values separately
                var item = dictionary[key];
                var prefix = indentation + $"[{keyString}:{key.GetType().Name}]:{item?.GetType().Name ?? "null"} = ";
                sb.Append(prefix + SerializeObject(
                    item,
                    path.Append(keyString).ToArray(),
                    nestingLevel + 1));
            }

            return sb.ToString();
        }

        private IPropertyConfigurator? GetConfiguratorOrDefault(string propertyName, string[] path, Type type)
        {
            var matchedNode = configurationRoot.GetByPathOrDefault(path.Append(propertyName).ToArray());
            if (matchedNode is TerminalNode<IPropertyConfigurator> terminal)
                return terminal.Payload;

            if (groupAppliedConfigurators.TryGetValue(type, out var configurator))
                return configurator;
            return default;
        }
    }
}