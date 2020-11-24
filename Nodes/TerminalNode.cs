namespace Nodes
{
    public class TerminalNode<TPayload> : IParentedNode<TPayload>
    {
        internal TerminalNode(string name, TPayload payload)
        {
            Name = name;
            Payload = payload;
        }

        public string Name { get; }
        public IChildedNode<TPayload> Parent { get; internal set; }

        IChildedNode<TPayload> IParentedNode<TPayload>.Parent
        {
            get => this.Parent;
            set => this.Parent = value;
        }

        public TPayload Payload { get; }
    }
}