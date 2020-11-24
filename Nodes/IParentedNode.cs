namespace Nodes
{
    public interface IParentedNode<TPayload> : INode<TPayload>
    {
        IChildedNode<TPayload> Parent { get; internal set; }
    }
}