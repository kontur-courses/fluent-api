using System.Globalization;
using FluentAssertions;
using ObjectPrinting;

namespace ObjectPrintingTests;

[TestFixture]
public class ObjectPrinterTests
{
    [Test]
    public void PrintToString_ShouldSerializePerson_WhenUseProperties()
    {
        var person = new Person { Name = "John", Age = 25, Height = 175};
        
        var actualResult = ObjectPrinter.For<Person>().PrintToString(person);
        var expectedResult = "Person\r\n\tId = Guid\r\n\tName = John\r\n\tHeight = 175\r\n\tAge = 25\r\n";
        
        actualResult.Should().BeEquivalentTo(expectedResult);
    }
    
    [Test]
    public void PrintToString_ShouldSerializePerson_WhenUseFields()
    {
        var person = new PersonWithFields { Name = "John", Age = 25, Height = 175};
        
        var actualResult = ObjectPrinter.For<PersonWithFields>().PrintToString(person);
        var expectedResult = "PersonWithFields\r\n\tId = Guid\r\n\tName = John\r\n\tHeight = 175\r\n\tAge = 25\r\n";
        
        actualResult.Should().BeEquivalentTo(expectedResult);
    }
    
    [Test]
    public void PrintToString_ShouldSerializePerson_WhenExcludeType()
    {
        var personWithWeight = new PersonWithWeight { Name = "John", Age = 25, Height = 175, Weight = 70 };
        
        var actualResult = ObjectPrinter.For<PersonWithWeight>().Excluding<double>().PrintToString(personWithWeight);
        var expectedResult = "PersonWithWeight\r\n\tId = Guid\r\n\tName = John\r\n\tAge = 25\r\n";
        
        actualResult.Should().BeEquivalentTo(expectedResult);
    }
    
    [Test]
    public void PrintToString_ShouldSerializePerson_WhenExcludeProperty()
    {
        var person = new Person { Name = "John", Age = 25, Height = 175 };
        
        var actualResult = ObjectPrinter.For<Person>().Excluding(p => p.Id).PrintToString(person);
        var expectedResult = "Person\r\n\tName = John\r\n\tHeight = 175\r\n\tAge = 25\r\n";
        
        actualResult.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void PrintToString_ShouldSerializePerson_WhenExcludeFields()
    {
        var person = new PersonWithFields { Name = "John", Age = 25, Height = 175 };
        
        var actualResult = ObjectPrinter.For<PersonWithFields>().Excluding(p => p.Id).PrintToString(person);
        var expectedResult = "PersonWithFields\r\n\tName = John\r\n\tHeight = 175\r\n\tAge = 25\r\n";
        
        actualResult.Should().BeEquivalentTo(expectedResult);
    }
    
    [Test]
    public void PrintToString_ShouldSerializePerson_WhenUseCustomSerializationForType()
    {
        var personWithWeight = new PersonWithWeight { Name = "John", Age = 25, Height = 175, Weight = 70 };
        
        var actualResult = ObjectPrinter
            .For<PersonWithWeight>()
            .For<double>().Using(x => $"{x:F2}")
            .PrintToString(personWithWeight);
        var expectedResult = "PersonWithWeight\r\n" +
                             "\tId = Guid\r\n" +
                             "\tName = John\r\n" +
                             "\tHeight = 175,00\r\n" +
                             "\tWeight = 70,00\r\n" +
                             "\tAge = 25\r\n";
        
        actualResult.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void PrintToString_ShouldSerializePerson_WhenUseCustomCultureInfo()
    {
        var personWithWeight = new PersonWithWeight { Name = "John", Age = 25, Height = 175.5, Weight = 70.5 };
        
        var actualResult = ObjectPrinter
            .For<PersonWithWeight>()
            .For<double>().Using(CultureInfo.InvariantCulture)
            .PrintToString(personWithWeight);
        var expectedResult = "PersonWithWeight\r\n" +
                             "\tId = Guid\r\n" +
                             "\tName = John\r\n" +
                             "\tHeight = 175.5\r\n" +
                             "\tWeight = 70.5\r\n" +
                             "\tAge = 25\r\n";
        
        actualResult.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void PrintToString_ShouldSerializePerson_WhenUseCustomSerializationForMember()
    {
        var person = new Person { Name = "John", Age = 25, Height = 175.6 };
        
        var actualResult = ObjectPrinter
            .For<Person>()
            .For(p => p.Height).Using(h => ((int)Math.Round(h)).ToString())
            .PrintToString(person);
        var expectedResult = "Person\r\n\tId = Guid\r\n\tName = John\r\n\tHeight = 176\r\n\tAge = 25\r\n";
        
        actualResult.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void PrintToString_ShouldSerializePerson_WhenTrimString()
    {
        var person = new Person { Name = "John Jo", Age = 25, Height = 175 };

        var actualResult = ObjectPrinter
            .For<Person>()
            .For(p => p.Name).MaxLength(4)
            .PrintToString(person);
        var expectedResult = "Person\r\n\tId = Guid\r\n\tName = John\r\n\tHeight = 175\r\n\tAge = 25\r\n";
        
        actualResult.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void PrintToString_ShouldSerializePerson_WhenHaveCycleReference()
    {
        var personWithParent = new PersonWithParent { Name = "John", Age = 25, Height = 175 };
        personWithParent.Parent = personWithParent;
        
        var actualResult = ObjectPrinter
            .For<PersonWithParent>()
            .PrintToString(personWithParent);
        var expectedResult = "PersonWithParent\r\n" +
                             "\tId = Guid\r\n" +
                             "\tName = John\r\n" +
                             "\tHeight = 175\r\n" +
                             "\tAge = 25\r\n" +
                             "\tParent = cycled PersonWithParent in level 0\r\n";
        
        actualResult.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void PrintToString_ShouldSerializeList()
    {
        var numbers = new[] { 1, 2, 3, 4 };
        
        var actualResult = ObjectPrinter
            .For<int[]>()
            .PrintToString(numbers);
        var expectedResult = "Collection\r\n\t1\r\n\t2\r\n\t3\r\n\t4\r\n";
        
        actualResult.Should().BeEquivalentTo(expectedResult);
    }
    
    [Test]
    public void PrintToString_ShouldSerializeDictionary()
    {
        var dictionary = new Dictionary<int, string>
        {
            { 1, "test1" },
            { 2, "test2" },
            { 3, "test3" }
        };
        
        var actualResult = ObjectPrinter
            .For<Dictionary<int, string>>()
            .PrintToString(dictionary);
        var expectedResult = "Dictionary\r\n\t1 : test1\r\n\t2 : test2\r\n\t3 : test3\r\n";
        
        actualResult.Should().BeEquivalentTo(expectedResult);
    }
}