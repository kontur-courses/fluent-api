using System.Collections.Generic;

namespace Nodes
{
    public class ChildedNode<TPayload> : IChildedNode<TPayload>, IParentedNode<TPayload>
    {
        IDictionary<string, IParentedNode<TPayload>> IChildedNode<TPayload>.ChildNodes { get; } =
            new Dictionary<string, IParentedNode<TPayload>>();

        public ChildedNode(string name, IChildedNode<TPayload> parent)
        {
            Name = name;
            Parent = parent;
        }

        public string Name { get; }
        public IChildedNode<TPayload> Parent { get; set; }
    }
}