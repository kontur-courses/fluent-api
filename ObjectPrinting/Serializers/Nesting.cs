namespace ObjectPrinting.Serializers
{
    public record Nesting
    {
        public int Level { get; init; }
        public int Offset { get; init; }
        public string Indentation => new string(indentationSymbol, Level) + new string(' ', Offset);

        private readonly char indentationSymbol;

        public Nesting(char indentationSymbol = '\t')
        {
            this.indentationSymbol = indentationSymbol;
        }
    }
}