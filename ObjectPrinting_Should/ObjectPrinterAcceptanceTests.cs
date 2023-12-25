using System.Globalization;
using FluentAssertions;
using ObjectPrinting;

namespace ObjectPrinting_Should;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    private Person person = null!;

    [SetUp]
    public void Setup()
    {
        person = new Person { Name = "Alex", Age = 19, Height = 179.5, Id = new Guid() };
    }

    [Test]
    public void PrintToString_SkipsExcludedTypes()
    {
        var printer = ObjectPrinter.For<Person>().Excluding<Guid>();
        var expectedString = string.Join(Environment.NewLine, "Person", "\tSibling = null",
            "\tName = Alex", "\tHeight = 179,5", "\tAge = 19", "\tFavouriteNumbers = null", "");
        var outputString = printer.PrintToString(person);
        outputString.Should().Be(expectedString);
    }

    [Test]
    public void PrintToString_SkipsExcludedProperty()
    {
        var printer = ObjectPrinter.For<Person>().Excluding(p => p.Id);

        var expectedString = string.Join(Environment.NewLine, "Person", "\tSibling = null",
            "\tName = Alex", "\tHeight = 179,5", "\tAge = 19", "\tFavouriteNumbers = null", "");
        var outputString = printer.PrintToString(person);
        outputString.Should().Be(expectedString);
    }

    [Test]
    public void PrintToString_UsesCustomSerializator_WhenGivenToType()
    {
        var printer = ObjectPrinter.For<Person>().Printing<int>().Using(i => i.ToString("X"));

        var expectedString = string.Join(Environment.NewLine, "Person", "\tId = 00000000-0000-0000-0000-000000000000",
            "\tSibling = null", "\tName = Alex", "\tHeight = 179,5", "\tAge = 13", "\tFavouriteNumbers = null", "");
        var outputString = printer.PrintToString(person);
        outputString.Should().Be(expectedString);
    }

    [Test]
    public void PrintToString_UsesCustomSerialization_WhenGivenToProperty()
    {
        var printer = ObjectPrinter.For<Person>().Printing(p => p.Age).Using(i => i.ToString("X"));

        var expectedString = string.Join(Environment.NewLine, "Person", "\tId = 00000000-0000-0000-0000-000000000000",
            "\tSibling = null", "\tName = Alex", "\tHeight = 179,5", "\tAge = 13", "\tFavouriteNumbers = null", "");
        var outputString = printer.PrintToString(person);
        outputString.Should().Be(expectedString);
    }

    [Test]
    public void PrintToString_TrimsStringProperties_WhenTrimmingIsSet()
    {
        var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).TrimmedToLength(1);

        var expectedString = string.Join(Environment.NewLine, "Person", "\tId = 00000000-0000-0000-0000-000000000000",
            "\tSibling = null", "\tName = A", "\tHeight = 179,5", "\tAge = 19", "\tFavouriteNumbers = null", "");
        var outputString = printer.PrintToString(person);
        outputString.Should().Be(expectedString);
    }

    [Test]
    public void PrintToString_UsesCustomCulture_WhenGivenToNumericType()
    {
        var printer = ObjectPrinter.For<Person>().Printing<double>().Using<double>(CultureInfo.InvariantCulture);

        var expectedString = string.Join(Environment.NewLine, "Person", "\tId = 00000000-0000-0000-0000-000000000000",
            "\tSibling = null", "\tName = Alex", "\tHeight = 179.5", "\tAge = 19", "\tFavouriteNumbers = null", "");
        var outputString = printer.PrintToString(person);
        outputString.Should().Be(expectedString);
    }

    [Test]
    public void PrintToString_SerializesClass_WhenCalledFromItInstance()
    {
        var outputString = person.PrintToString();
        var expectedString = string.Join(Environment.NewLine, "Person", "\tId = 00000000-0000-0000-0000-000000000000",
            "\tSibling = null", "\tName = Alex", "\tHeight = 179,5", "\tAge = 19", "\tFavouriteNumbers = null", "");
        outputString.Should().Be(expectedString);
    }

    [Test]
    public void PrintToString_SerializesClass_WhenCalledFromItInstanceWithConfig()
    {
        var outputString = person.PrintToString(s => s.Excluding(p => p.Age));
        var expectedString = string.Join(Environment.NewLine, "Person", "\tId = 00000000-0000-0000-0000-000000000000",
            "\tSibling = null", "\tName = Alex", "\tHeight = 179,5", "\tFavouriteNumbers = null", "");
        outputString.Should().Be(expectedString);
    }

    [Test]
    public void PrintToString_SerializesCyclicReferences()
    {
        var firstPerson = new Person() { Age = 20, Name = "Ben" };
        var secondPerson = new Person() { Age = 20, Name = "John", Sibling = firstPerson };
        firstPerson.Sibling = secondPerson;
        //var printer = ObjectPrinter.For<Person>().Excluding<double>().Excluding<Guid>();

        var outputString = firstPerson.PrintToString();
        var expectedString = string.Join(Environment.NewLine, "Person", 
            "\tId = 00000000-0000-0000-0000-000000000000",
            "\tSibling = Person", "\t\tId = 00000000-0000-0000-0000-000000000000", 
            "\t\tName = John", "\t\tHeight = 0", "\t\tAge = 20", "\t\tFavouriteNumbers = null", 
            "\tName = Ben", "\tHeight = 0", "\tAge = 20", "\tFavouriteNumbers = null", "");
        outputString.Should().Be(expectedString);
    }

    [Test]
    public void PrintToString_SerializesArray_WhenInClass()
    {
        person.FavouriteNumbers = new[] { 1.1, 2.2, 3.3, 4.4, 5.5 };
        var outputString = person.PrintToString();
        var expectedString = string.Join(Environment.NewLine, "Person",
            "\tId = 00000000-0000-0000-0000-000000000000", 
            "\tSibling = null", "\tName = Alex", "\tHeight = 179,5", "\tAge = 19",
            "\tFavouriteNumbers = [ 1,1 2,2 3,3 4,4 5,5 ]", "");
        outputString.Should().Be(expectedString);
    }

    [Test]
    public void PrintToString_SerializesArray()
    {
        var numbers = new[] { 1.1, 2.2, 3.3, 4.4, 5.5 };
        var outputString = numbers.PrintToString();
        var expectedString = string.Join(Environment.NewLine, "[ 1,1 2,2 3,3 4,4 5,5 ]", "");
        outputString.Should().Be(expectedString);
    }
        
    [Test]
    public void PrintToString_SerializesEmptyArray()
    {
        var numbers = Array.Empty<int>();
        var outputString = numbers.PrintToString();
        var expectedString = string.Join(Environment.NewLine, "[]", "");
        outputString.Should().Be(expectedString);
    }

    [Test]
    public void PrintToString_SerializesList()
    {
        var numbers = new List<double> { 1.1, 2.2, 3.3, 4.4, 5.5 };
        var outputString = numbers.PrintToString();
        var expectedString = string.Join(Environment.NewLine, "[ 1,1 2,2 3,3 4,4 5,5 ]", "");
        outputString.Should().Be(expectedString);
    }

    [Test]
    public void PrintToString_SerializesDictionary()
    {
        var numbers = new Dictionary<Person, double> { { new Person(), 1 }, { new Person(), 2 } };
        var arrayOfDictionaries = new[] { numbers , new Dictionary<Person, double>()};
        var arrayOfDictionariesHashSet = new HashSet<Dictionary<Person,double>[]> {arrayOfDictionaries,};
        var printer = ObjectPrinter.For<HashSet<Dictionary<Person,double>[]>>().Excluding<Guid>();

        var outputString = printer.PrintToString(arrayOfDictionariesHashSet);
        var expectedString = @"[
	[
		{
			[
			Person
				Sibling = null
				Name = null
				Height = 0
				Age = 0
				FavouriteNumbers = null
			:
			1
			]
			[
			Person
				Sibling = null
				Name = null
				Height = 0
				Age = 0
				FavouriteNumbers = null
			:
			2
			]
		}
		{
		}
	]
]
";
        outputString.Should().Be(expectedString);
    }
}