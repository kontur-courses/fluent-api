using System;
using System.Linq;
using System.Text;
using Nodes;
using ObjectPrinting.Configuration;
using ObjectPrinting.Serializers;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes;
        private RootNode<IPropertySerializer> configurationRoot;

        public PrintingConfig(Type[] finalTypes, RootNode<IPropertySerializer> configurationRoot)
        {
            this.finalTypes = finalTypes;
            this.configurationRoot = configurationRoot;
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
                sb.Append(SerializeProperty(target.MemberName, target.GetValue(obj), nestingLevel + 1));

            return sb.ToString();
        }

        private string SerializeProperty(string propertyName, object? value, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            return $"{indentation}{propertyName} = " + PrintToString(value, nestingLevel + 1);
        }
    }
}