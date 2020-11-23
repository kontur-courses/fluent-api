namespace ObjectPrinting.Serializers
{
    public class TrimmedPropertySerializer : IPropertySerializer<string>
    {
        private readonly int maxLength;

        public TrimmedPropertySerializer(int maxLength)
        {
            this.maxLength = maxLength;
        }

        public string Serialize(string value)
        {
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}