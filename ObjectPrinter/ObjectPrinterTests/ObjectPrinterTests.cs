using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinter;
using ObjectPrinter.Extensions;

namespace ObjectPrinterTests;

[TestFixture]
public class ObjectPrinterTests
{
	[Test]
	public void Excluding_Types_ShouldNotPrinting()
	{
		var person = new Person { Id = new Guid(), Name = "Alex", Height = 179, Age = 19 };
		var customPrinter = ObjectPrinter<Person>.Configuration()
			.Excluding<int>()
			.Build();

		var result = customPrinter.PrintToString(person);

		result.Should().NotContain("Age");
	}

	[Test]
	public void Excluding_Properties_ShouldNotPrinting()
	{
		var person = new Person { Id = new Guid(), Name = "Alex", Height = 179, Age = 19 };
		var customPrinter = ObjectPrinter<Person>.Configuration()
			.Excluding(p => p.Id)
			.Build();

		var result = customPrinter.PrintToString(person);

		result.Should().NotContain("Id");
	}

	[Test]
	public void Using_CanApplyCustomRulesToType()
	{
		var person = new Person { Name = "Alex", Age = 19 };
		var customPrinter = ObjectPrinter<Person>.Configuration()
			.Printing<int>().Using(i => $"+-{i}")
			.Build();

		var result = customPrinter.PrintToString(person);

		result.Should().Contain($"+-{person.Age}");
	}

	[Test]
	public void Using_CanApplyCustomRulesToProperty()
	{
		var person = new Person { Name = "Alex", Age = 19 };
		var customPrinter = ObjectPrinter<Person>.Configuration()
			.Printing(p => p.Name).Using(name => "****")
			.Build();

		var result = customPrinter.PrintToString(person);

		result.Should()
			.Contain("****")
			.And.NotContain($"{person.Name}");
	}

	[Test]
	public void Using_RuleForProperty_ShouldOverrideRuleForType()
	{
		var person = new Person { Name = "Alex", LastName = "Smith" };

		var customPrinter = ObjectPrinter<Person>.Configuration()
			.Printing<string>().Using(str => "It's string")
			.Printing(p => p.Name).Using(name => "It's name")
			.Build();

		var result = customPrinter.PrintToString(person);

		result.Should()
			.Contain("It's name")
			.And.Contain("It's string")
			.And.NotContain(person.Name)
			.And.NotContain(person.LastName);
	}

	[Test]
	public void PrintToString_NestedTypesWithoutCyclicReferences_ShouldPrint()
	{
		var person = new Person
		{
			Name = "Alex",
			Age = 19,
			Parent = new Person { Name = "God", Age = int.MaxValue }
		};

		var result = person.PrintToString();

		result.Should()
			.Contain("Parent = Person")
			.And.Contain("God")
			.And.Contain($"{int.MaxValue}");
	}

	[Test]
	public void PrintToString_NestedTypesWithCyclicReferences_ShouldNotThrow()
	{
		var person = new Person
		{
			Name = "Alex",
			Age = 19
		};
		person.Parent = person;

		var action = () => person.PrintToString();

		action.Should().NotThrow();
	}

	[Test]
	public void PrintToString_StaticMember_ShouldNotPrint()
	{
		var person = new Person
		{
			Name = "Alex",
			Age = 19
		};

		var result = person.PrintToString();

		result.Should()
			.NotContain("MaxAge")
			.And.NotContain("MinAge");
	}

	[Test]
	public void Using_CanAddCulture()
	{
		var ruCulture = new CultureInfo("ru-RU");
		var enCulture = new CultureInfo("en-Us");
		var person = new Person
		{
			Height = 49.5
		};

		var withRuCulture = person
			.PrintToString(c => c.Printing(p => p.Height).Using(ruCulture));

		var withEnCulture = person
			.PrintToString(c => c.Printing<double>().Using(enCulture));

		withRuCulture.Should().Contain(person.Height.ToString(ruCulture));
		withEnCulture.Should().Contain(person.Height.ToString(enCulture));
	}

	[Test]
	public void TrimmedToLength_ShouldTrim()
	{
		var person = new Person
		{
			Name = "1234567"
		};

		var customPrinter = ObjectPrinter<Person>.Configuration()
			.Printing(p => p.Name).TrimmedToLength(2)
			.Build();

		var result = customPrinter.PrintToString(person);

		result.Should()
			.Contain("12")
			.And.NotContain("3");
	}

	[Test]
	public void AcceptanceTests()
	{
		var person = new Person { Name = "Alex", Age = 19, Height = 179.2, Id = new Guid() };

		var customPrinter = ObjectPrinter<Person>.Configuration()
			//1. Исключение из сериализации свойства/ поля определенного типа
			.Excluding<Guid>()
			//2. Альтернативный способ сериализации для определенного типа
			.Printing<int>().Using(i => i.ToString("X"))
			//3. Для всех типов, имеющих культуру, есть возможность ее указать
			.Printing<double>().Using(CultureInfo.InvariantCulture)
			//4. Настройка сериализации конкретного свойства / поля
			//5. Возможность обрезания строк
			.Printing(p => p.Name).TrimmedToLength(10)
			//6. Исключение из сериализации конкретного свойства/ поля
			.Excluding(p => p.Parent)
			.Build();


		var s1 = customPrinter.PrintToString(person);

		//7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
		var s2 = person.PrintToString();

		//8. ...с конфигурированием
		var s3 = person.PrintToString(s => s.Excluding(p => p.Age));
		Console.WriteLine(s1);
		Console.WriteLine(s2);
		Console.WriteLine(s3);
	}

	private class Person
	{
		public static int MaxAge = int.MaxValue;
		public string LastName;
		public static int MinAge => 0;

		public Guid Id { get; set; }
		public string Name { get; set; }
		public double Height { get; set; }
		public int Age { get; set; }

		public Person Parent { get; set; }
	}
}