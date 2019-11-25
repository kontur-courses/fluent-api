using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
	[TestFixture]
	public class ObjectPrinterAcceptanceTests
	{
		[Test]
		public void Demo()
		{
			var person = new Person {Name = new Name("Alex"), Age = 19};

			var s1 = ObjectPrinter.For<Person>()
				.Excluding<Guid>()
				.Excluding(p => p.Age)
				.Printing<int>().Using(i => i.ToString("X"))
				.Printing<double>().Using(CultureInfo.InvariantCulture)
				.Printing(p => p.Name.Firstname).TrimmedToLength(10)
				.PrintToString(person);

			//7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
			string s2 = person.PrintToString();

			//8. ...с конфигурированием
			string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
			Console.WriteLine(s1);
			Console.WriteLine(s2);
			Console.WriteLine(s3);
		}
	}
}