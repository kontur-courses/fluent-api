using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
	[TestFixture]
	public class ObjectPrinter_Tests
	{
		private PrintingConfig<Person> _objPrinter;
		private Person _person;

		[SetUp]
		public void SetUp()
		{
			_objPrinter = ObjectPrinter.For<Person>();
			_person = new Person
			{
				Age = 19,
				Height = 600,
				Name = new Name("John", "Cool"),
			};
		}

		[Test]
		public void Excluding_ExcludesFieldByType()
		{
			var expectedString = @"Person
	Id = Guid
	Name = Name
		Firstname = John
		Surname = Cool
	Height = 600
";

			var actualString = _objPrinter.Excluding<int>().PrintToString(_person);
			actualString.Should().Be(expectedString);
		}

		[Test]
		public void Excluding_ExcludesNestedPropertyByName()
		{
			var expectedString = @"Person
	Id = Guid
	Name = Name
		Firstname = John
	Height = 600
	Age = 19
";

			var actualString = _objPrinter
				.Excluding(p => p.Name.Surname)
				.PrintToString(_person);
			actualString.Should().Be(expectedString);
		}

		[Test]
		public void Printing_AllowsChangingTypePrintingMethodWithUsing()
		{
			var expectedString = @"Person
	Id = Guid
	Name = John Cool
		Firstname = John
		Surname = Cool
	Height = 600
	Age = 19
";
			var actualString = _objPrinter
				.Printing<Name>().Using(n => $"{n.Firstname} {n.Surname}")
				.PrintToString(_person);
			actualString.Should().Be(expectedString);
		}

		[Test]
		public void Printing_AllowsChangingTypePrintingCultureWithUsing()
		{
			_person.Height = 6.1;
			var expectedString = @"Person
	Id = Guid
	Name = Name
		Firstname = John
		Surname = Cool
	Height = 6,1
	Age = 19
";

			var actualString = _objPrinter
				.Printing<double>().Using(CultureInfo.CurrentCulture)
				.PrintToString(_person);
			actualString.Should().Be(expectedString);
		}

		[Test]
		public void TrimmedToLength_TrimsPropertyValue()
		{
			var expectedString = @"Person
	Id = Guid
	Name = Name
		Firstname = Jo
		Surname = Cool
	Height = 600
	Age = 19
";
			var actualString = _objPrinter
				.Printing(p => p.Name.Firstname).TrimmedToLength(2)
				.PrintToString(_person);
			actualString.Should().Be(expectedString);
		}

		[Test]
		public void PrintsArrayCorrectly()
		{
			var array = new[] {1, 2};
			var expectedString = @"Int32[]
	[0] 1
	[1] 2
";

			var actualString = ObjectPrinter.For<int[]>()
				.PrintToString(array);
			actualString.Should().Be(expectedString);
		}

		[Test]
		public void PrintsDictionaryCorrectly()
		{
			var dictionary = new Dictionary<string, int>
			{
				{"One", 1},
				{"Two", 2},
			};
			var expectedString = @"System.Collections.Generic.Dictionary`2[System.String,System.Int32]
	One: 1
	Two: 2
";

			var actualString = ObjectPrinter.For<Dictionary<string, int>>()
				.PrintToString(dictionary);
			actualString.Should().Be(expectedString);
		}

		[Test]
		public void PreventsCircularReferences()
		{
			var father1 = new Father();
			var father2 = new Father{San = father1};
			father1.San = father2;
			var expectedString = @"Father
	San = Father
		San = Circular reference
";
			
			var actualString = ObjectPrinter.For<Father>()
				.PrintToString(father1);
			actualString.Should().Be(expectedString);
		}
	}
}