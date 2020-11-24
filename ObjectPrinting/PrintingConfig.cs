using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nodes;
using ObjectPrinting.Configuration;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes;
        private readonly RootNode<IPropertyConfigurator> configurationRoot;
        private readonly IDictionary<Type, IPropertyConfigurator> groupAppliedConfigurators;

        public PrintingConfig(Type[] finalTypes, RootNode<IPropertyConfigurator> configurationRoot,
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
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (finalTypes.Contains(type))
                return obj.ToString() + Environment.NewLine;

            var sb = new StringBuilder();
            sb.AppendLine(type.Name);

            var serializedProperties = SerializationTarget.EnumerateFrom(type)
                .Select(t => SerializeProperty(t.MemberName, path, t.GetValue(obj), t.MemberType, nestingLevel + 1));

            sb.AppendJoin(string.Empty, serializedProperties);
            return sb.ToString();
        }

        private string SerializeProperty(string propertyName, string[] path, object value, Type type, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var serialized = GetConfiguratorOrDefault(propertyName, path, type)?.AppliedSerializer.Serialize(value)
                             ?? SerializeObject(value, path.Append(propertyName).ToArray(), nestingLevel + 1);

            return string.IsNullOrEmpty(serialized)
                ? string.Empty
                : $"{indentation}{propertyName} = " +
                  (serialized.EndsWith(Environment.NewLine)
                      ? serialized
                      : serialized + Environment.NewLine);
        }

        private IPropertyConfigurator? GetConfiguratorOrDefault(string propertyName, string[] path, Type type)
        {
            if (groupAppliedConfigurators.TryGetValue(type, out var configurator))
                return configurator;

            var matchedNode = configurationRoot.GetByPathOrDefault(path.Append(propertyName).ToArray());
            if (matchedNode is TerminalNode<IPropertyConfigurator> terminal)
                return terminal.Payload;
            return default;
        }
    }
}