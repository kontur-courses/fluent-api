namespace ObjectPrinterTests
{
    public class Kid
    {
        public string Name { set; get; }
        public Kid Parent;

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj?.GetType() != GetType()) return false;
            var kid = (Kid)obj;
            return Name == kid.Name;
        }
    }
    
}