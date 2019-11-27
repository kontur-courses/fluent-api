namespace ObjectPrinting.Tests
{
    public class PersonWithSimpleHashCode
    {
        public string Name { get; set; }
        public PersonWithSimpleHashCode Friend { get; set; }
        public PersonWithSimpleHashCode BestFriend { get; set; }

        public override int GetHashCode() => Name?.GetHashCode() ?? 0;

        protected bool Equals(PersonWithSimpleHashCode other) => Name == other.Name;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((PersonWithSimpleHashCode) obj);
        }
    }
}