using System.Globalization;
using FluentAssertions;

namespace ObjectPrinting.Tests;

public class PrintingConfigTests
{
    private readonly Person defaultPerson = new()
    {
        Name = "Alex",
        Age = 19,
        Email = "alex@gmail.com",
        Height = 185.5
    };

    [Test]
    public void Excluding_MustExcludeFieldsAndTypePropertiesFromSerialization()
    {
        var expected = string.Join(
            Environment.NewLine, 
            [
                "Person",
                "\tId = 00000000-0000-0000-0000-000000000000",
                "\tHeight = 185,5",
                "\tAge = 19",
                "\tParent = null",
                "\tChild = null",
                string.Empty
            ]);
        var printer = ObjectPrinter.For<Person>()
            .Excluding<string>();
        
        var actual = printer.PrintToString(defaultPerson);
        
        actual.Should().Be(expected);
    }
    
    [Test]
    public void PrintToString_FullObjectSerialization()
    {
        var expected = string.Join(
            Environment.NewLine, 
            [
                "Person",
                "\tId = 00000000-0000-0000-0000-000000000000",
                "\tName = Alex",
                "\tHeight = 185,5",
                "\tAge = 19",
                "\tParent = null",
                "\tChild = null",
                "\tEmail = alex@gmail.com",
                string.Empty
            ]);
        var printer = ObjectPrinter.For<Person>();
        
        var actual = printer.PrintToString(defaultPerson);
        
        actual.Should().Be(expected);
    }

    [Test]
    public void Using_AlternativeWayToSerializeNumbers()
    {
        var expected = string.Join(
            Environment.NewLine, 
            [
                "Person",
                "\tId = 00000000-0000-0000-0000-000000000000",
                "\tName = Alex",
                "\tHeight = 185,5",
                "\tAge = 10011",
                "\tParent = null",
                "\tChild = null",
                "\tEmail = alex@gmail.com",
                string.Empty
            ]);
        var printer = ObjectPrinter.For<Person>()
            .Printing<int>()
            .Using(number => Convert.ToString(number, 2));
        
        var actual = printer.PrintToString(defaultPerson);
        
        actual.Should().Be(expected);
    }
    
    [Test]
    public void Excluding_NamePropertyIsExcluded()
    {
        var expected = string.Join(
            Environment.NewLine, 
            [
                "Person",
                "\tId = 00000000-0000-0000-0000-000000000000",
                "\tHeight = 185,5",
                "\tAge = 19",
                "\tParent = null",
                "\tChild = null",
                "\tEmail = alex@gmail.com",
                string.Empty
            ]);
        var printer = ObjectPrinter.For<Person>()
            .Excluding(p => p.Name);
        
        var actual = printer.PrintToString(defaultPerson);
        
        actual.Should().Be(expected);
    }
    
    [Test]
    public void Using_OtherLocalizationOfEmailField()
    {
        var expected = string.Join(
            Environment.NewLine, 
            [
                "Person",
                "\tId = 00000000-0000-0000-0000-000000000000",
                "\tName = Alex",
                "\tHeight = 185,5",
                "\tAge = 19",
                "\tParent = null",
                "\tChild = null",
                "\tEmail = ALEX@GMAIL.COM",
                string.Empty
            ]);
        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.Email)
            .Using(email => email.ToUpper() + Environment.NewLine);
        
        var actual = printer.PrintToString(defaultPerson);
        
        actual.Should().Be(expected);
    }
    
    [Test]
    public void Using_NamePropertyIsTruncated()
    {
        var person = new Person
        {
            Name = "Хьюберт Блейн Вольфешлегельштейнхаузенбергердорф-старший",
            Height = defaultPerson.Height,
            Age = defaultPerson.Age,
            Email = defaultPerson.Email
        };
        var expected = string.Join(
            Environment.NewLine, 
            [
                "Person",
                "\tId = 00000000-0000-0000-0000-000000000000",
                "\tName = Хьюберт Блейн Вольфе...",
                "\tHeight = 185,5",
                "\tAge = 19",
                "\tParent = null",
                "\tChild = null",
                "\tEmail = alex@gmail.com",
                string.Empty
            ]);
        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.Name)
            .TrimmedToLength(20);
        
        var actual = printer.PrintToString(person);
        
        actual.Should().Be(expected);
    }
    
    [Test]
    public void Using_ChangedTypeSerializationCulture()
    {
        var expected = string.Join(
            Environment.NewLine, 
            [
                "Person",
                "\tId = 00000000-0000-0000-0000-000000000000",
                "\tName = Alex",
                "\tHeight = 185.5",
                "\tAge = 19",
                "\tParent = null",
                "\tChild = null",
                "\tEmail = alex@gmail.com",
                string.Empty
            ]);
        var printer = ObjectPrinter.For<Person>()
            .Printing<double>()
            .Using(new CultureInfo("en-US"));
        
        var actual = printer.PrintToString(defaultPerson);
        
        actual.Should().Be(expected);
    }
    
    [Test]
    public void PrintToString_CyclicLinks_NoStackOverflowException()
    {
        var parent = new Person();
        var child = new Person();
        parent.Child = child;
        child.Parent = parent;
        var printer = ObjectPrinter.For<Person>();
        Action act = () => printer.PrintToString(parent);
        
        act.Should().NotThrow<StackOverflowException>();
    }

    [Test]
    public void PrintToString_CyclicLinks()
    {
        var expected = File.ReadAllText("ExpectedResponses/PrintToString_CyclicLinks.txt");
        var parent = new Person();
        var child = new Person();
        parent.Child = child;
        child.Parent = parent;
        var printer = ObjectPrinter.For<Person>();
        
        var actual = printer.PrintToString(parent);
        
        actual.Should().Be(expected);
    }
    
    [Test]
    public void PrintToString_List_SerializedList()
    {
        var expected = string.Join(
            Environment.NewLine, 
            [
                "List<Int32>",
                "\tCapacity = 4",
                "\tCount = 3",
                "\tcollection items:",
                "\t\t1",
                "\t\t2",
                "\t\t3",
                string.Empty
            ]);
        var list = new List<int> { 1, 2, 3 };
        
        CheckSerializationOfCollection(expected, list);
    }
    
    [Test]
    public void PrintToString_Array_SerializedArray()
    {
        var expected = File.ReadAllText("ExpectedResponses/PrintToString_Array_SerializedArray.txt");
        var array = new[] { 1, 2, 3 };
        
        CheckSerializationOfCollection(expected, array);
    }
    
    [Test]
    public void PrintToString_Dictionary_SerializedDictionary()
    {
        var expected = File.ReadAllText("ExpectedResponses/PrintToString_Dictionary_SerializedDictionary.txt");
        var dict = new Dictionary<List<string>, int>
        {
            { ["1"], 1 },
            { ["2"], 2 },
            { ["3"], 3 }
        };
        
        CheckSerializationOfCollection(expected, dict);
    }

    private void CheckSerializationOfCollection<TCollection>(string expected, TCollection collection)
    {
        var printer = ObjectPrinter.For<TCollection>();
        
        var actual = printer.PrintToString(collection);
        
        actual.Should().Be(expected);
    }
}