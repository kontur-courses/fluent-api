namespace Nodes
{
    public class TerminalNode<TPayload> : IParentedNode<TPayload>
    {
        internal TerminalNode(string name, IChildedNode<TPayload> parent, TPayload payload)
        {
            Name = name;
            Parent = parent;
            Payload = payload;
        }

        public string Name { get; }
        public IChildedNode<TPayload> Parent { get; set; }

        public TPayload Payload { get; }
    }
}