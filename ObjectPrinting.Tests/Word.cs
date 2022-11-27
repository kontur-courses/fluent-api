namespace ObjectPrinting.Tests
{
    public class Word
    {
        public string Prefix { get; set; }
        public string Root { get; set; }
        public string Suffix { get; set; }

        public int Length
        {
            get
            {
                var result = 0;
                if (Prefix != null) result += Prefix.Length;
                if (Root != null) result += Root.Length;
                if (Suffix != null) result += Suffix.Length;
                return result;
            }
        }
    }
}