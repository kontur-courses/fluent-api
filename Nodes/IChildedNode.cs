using System.Collections.Generic;

namespace Nodes
{
    public interface IChildedNode<TPayload> : INode<TPayload>
    {
        internal IDictionary<string, IParentedNode<TPayload>> ChildNodes { get; }
    }
}