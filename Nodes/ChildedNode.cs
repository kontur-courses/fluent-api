using System.Collections.Generic;

namespace Nodes
{
    public class ChildedNode<TPayload> : IChildedNode<TPayload>, IParentedNode<TPayload>
    {
        IDictionary<string, IParentedNode<TPayload>> IChildedNode<TPayload>.ChildNodes { get; } =
            new Dictionary<string, IParentedNode<TPayload>>();

        public ChildedNode(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public IChildedNode<TPayload> Parent { get; internal set; }

        IChildedNode<TPayload> IParentedNode<TPayload>.Parent
        {
            get => Parent;
            set => Parent = value;
        }
    }
}