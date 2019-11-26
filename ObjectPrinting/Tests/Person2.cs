namespace ObjectPrinting.Tests
{
	public class Person2
	{
		public string Name { get; set; }
		public Person2 Friend { get; set; }

		public override int GetHashCode() => Name?.GetHashCode() ?? 0;

		protected bool Equals(Person2 other) => Name == other.Name;

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj.GetType() == this.GetType() && Equals((Person2) obj);
		}
	}
}