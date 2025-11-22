using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.ObjectPrinter;

internal class SerializationContext<TOwner>(PrintingConfig<TOwner> config, StringBuilder builder)
{
    public readonly PrintingConfig<TOwner> Config = config;
    public readonly HashSet<object> Visited = new(ReferenceEqualityComparer.Instance);
    public readonly StringBuilder Builder = builder;

    public void Indent(int level) => Builder.Append(new string('\t', level));
}


