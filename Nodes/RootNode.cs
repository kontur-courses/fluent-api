using System.Collections.Generic;

namespace Nodes
{
    public class RootNode<TPayload> : IChildedNode<TPayload>
    {
        IDictionary<string, IParentedNode<TPayload>> IChildedNode<TPayload>.ChildNodes { get; } =
            new Dictionary<string, IParentedNode<TPayload>>();

        public RootNode(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}