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
        private RootNode<IPropertyConfigurator> configurationRoot;
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
            return PrintToString(obj, 0);
        }

        private string PrintToString(object? obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (finalTypes.Contains(type))
                return obj.ToString() + Environment.NewLine;

            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var target in SerializationTarget.EnumerateFrom(type))
                sb.Append(
                    SerializeProperty(target.MemberName, target.GetValue(obj), target.MemberType, nestingLevel + 1));

            return sb.ToString();
        }

        private string SerializeProperty(string propertyName, object value, Type type, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var serialized = groupAppliedConfigurators.TryGetValue(type, out var configurator) && configurator != null
                ? configurator.AppliedSerializer.Serialize(value)
                : PrintToString(value, nestingLevel + 1);
            if (string.IsNullOrEmpty(serialized))
                return string.Empty;
            return $"{indentation}{propertyName} = " + serialized;
        }
    }
}