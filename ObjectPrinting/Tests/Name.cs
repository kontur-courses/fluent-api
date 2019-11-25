namespace ObjectPrinting.Tests
{
	public class Name
	{
		public string Firstname { get; }
		public string Surname { get; }

		public Name(string firstname, string surname="")
		{
			Firstname = firstname;
			Surname = surname;
		}
	}
}