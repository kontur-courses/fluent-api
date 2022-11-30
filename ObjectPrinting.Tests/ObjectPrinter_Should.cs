using System.Globalization;
using FluentAssertions;
using ObjectPrinting.Extensions;
using ObjectPrinting.Models;
using ObjectPrinting.PrintingConfiguration;

namespace ObjectPrinting.Tests;

public class ObjectPrinter_Should
{
    private PrintingConfig<Person> personConfig;
    private Person person1;
    private Person person2;
    private Person person3;

    [SetUp]
    public void Setup()
    {
        personConfig = ObjectPrinter.For<Person>();
        person1 = new Person { Age = 29, Height = 150.45, Id = Guid.NewGuid(), Name = "James Hetfield" };
        person2 = new Person { Age = 46, Height = 200.34, Id = Guid.NewGuid(), Name = "Lars Ulrich" };
        person3 = new Person { Age = 50, Height = 183.46, Id = Guid.NewGuid(), Name = "Kirk Hammett" };
    }

    [Test]
    public void PrintToString_ShouldReturnObject_WOConfiguration()
    {
        var str = personConfig.PrintToString(person1);
        str.Should().Contain($"Age = {person1.Age}");
        str.Should().Contain($"Name = {person1.Name}");
        str.Should().Contain("Id = Guid");
        str.Should().Contain($"Height = {person1.Height}");
        str.Should().Contain($"Birthday = {person1.Birthday}");
    }

    [Test]
    public void Exclude_ShouldExclude_Type()
    {
        personConfig.For<int>().Exclude();
        var str = personConfig.PrintToString(person1);
        str.Should().NotContain("Age");
    }

    [Test]
    public void Exclude_ShouldExclude_ManyTypes()
    {
        personConfig.For<int>().Exclude()
            .For<string>().Exclude()
            .For<Guid>().Exclude();
        var str = personConfig.PrintToString(person1);
        str.Should().NotContain("Id");
        str.Should().NotContain("Name");
        str.Should().NotContain("Age");
    }

    [Test]
    public void Exclude_ShouldExclude_Property()
    {
        personConfig.For(p => p.Id).Exclude();
        var str = personConfig.PrintToString(person1);
        str.Should().NotContain("Id");
    }

    [Test]
    public void Exclude_ShouldExclude_ManyProperties()
    {
        personConfig.For(p => p.Id).Exclude()
            .For(p => p.Name).Exclude();
        var str = personConfig.PrintToString(person1);
        str.Should().NotContain("Id").And.NotContain("Name");
    }

    [Test]
    public void ChangeSerialization_ShouldCustomizeSerialization_ForProperty()
    {
        personConfig.For(p => p.Height).ChangeSerialization(height => height.ToString("P"));
        var str = personConfig.PrintToString(person1);
        str.Should().Contain($"Height = {person1.Height:P}");
        Console.WriteLine(str);
    }

    [Test]
    public void ChangeSerialization_ShouldCustomizeSerialization_ForType()
    {
        personConfig.For<double>().ChangeSerialization(d => d.ToString("P"));
        var str = personConfig.PrintToString(person1);
        str.Should().Contain($"Height = {person1.Height:P}");
        Console.WriteLine(str);
    }

    [Test]
    public void UseCulture_ShouldCustomizeCulture_ForType()
    {
        personConfig.For<double>().UseCulture(CultureInfo.InvariantCulture);
        var str = personConfig.PrintToString(person1);
        str.Should().Contain($"Height = {person1.Height.ToString(CultureInfo.InvariantCulture)}");
        Console.WriteLine(str);
    }

    [Test]
    public void UseCulture_ShouldNotCustomizeCulture_ForDifferentType()
    {
        personConfig.For<double>().UseCulture(CultureInfo.InvariantCulture);
        var str = personConfig.PrintToString(person1);
        str.Should().Contain($"Height = {person1.Height.ToString(CultureInfo.InvariantCulture)}");
        str.Should().Contain($"Birthday = {person1.Birthday.ToString(CultureInfo.CurrentCulture)}");
        Console.WriteLine(str);
    }

    [Test]
    public void UseCulture_ShouldCustomizeCulture_ForManyTypes()
    {
        personConfig.For<double>().UseCulture(CultureInfo.InvariantCulture)
            .For<DateTime>().UseCulture(CultureInfo.InvariantCulture);
        var str = personConfig.PrintToString(person1);
        str.Should().Contain($"Height = {person1.Height.ToString(CultureInfo.InvariantCulture)}");
        str.Should().Contain($"Birthday = {person1.Birthday.ToString(CultureInfo.InvariantCulture)}");
        Console.WriteLine(str);
    }

    [TestCase(0)]
    [TestCase(5)]
    public void SetLength_ShouldTrim_SmallStrings(int count)
    {
        personConfig.For(s => s.Name).SetLength(count);
        var str = personConfig.PrintToString(person1);
        str.Should().Contain($"Name = {person1.Name[..count] + Environment.NewLine}");
        Console.WriteLine(str);
    }

    [Test]
    public void SetLength_ShouldNotChangeString_IfLengthMoreThanSizeOfAString()
    {
        personConfig.For(s => s.Name).SetLength(person1.Name.Length + 1);
        var str = personConfig.PrintToString(person1);
        str.Should().Contain($"Name = {person1.Name + Environment.NewLine}");
        Console.WriteLine(str);
    }

    [Test]
    public void PrintToString_ShouldWork_WithArrays()
    {
        var collection = new[] { person1, person2, person3 };
        var str = personConfig.PrintEnumerable(collection, 0);
        Console.WriteLine(str);
    }

    [Test]
    public void PrintToString_ShouldWork_WithLists()
    {
        var collection = new List<Person> { person1, person2, person3 };
        personConfig.For(p => p.Id).Exclude();
        var str = personConfig.PrintEnumerable(collection, 0);
        Console.WriteLine(str);
    }

    [Test]
    public void PrintToString_ShouldWorkCorrect_WithCyclicReference()
    {
        var person = person1;
        person.Parent = person;
        Action action = () => personConfig.PrintToString(person);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void IgnoreCyclicReference_ShouldIgnore_WithCyclicReference()
    {
        var person = person1;
        person.Parent = person;
        personConfig.IgnoreCyclicReference();
        var str = personConfig.PrintToString(person);
        str.Should().Contain("New cyclic reference detected");
        Console.WriteLine(str);
    }

    [Test]
    public void PrintToString_ShouldPrintParent_WithConfiguration()
    {
        var person = person1;
        person.Parent = person2;
        personConfig.For<int>().Exclude();
        personConfig.For(p => p.Id).Exclude();
        var str = personConfig.PrintToString(person);
        str.Should().NotContain("Age");
        str.Should().NotContain("Id");
        Console.WriteLine(str);
    }

    [Test]
    public void PrintToString_ShouldPrintArrays_InsideClass()
    {
        var person = person1;
        person.Aliases = new[] { "Alex", "Memes", "Tupac" };
        var str = personConfig.PrintToString(person);
        str.Should().Contain("Age");
        str.Should().Contain("Memes");
        str.Should().Contain("Tupac");
        Console.WriteLine(str);
    }

    [Test]
    public void PrintToString_ShouldPrintLists_InsideClass()
    {
        var person = person1;
        person.FavoriteNumbers = new List<int> { 420, 1337, 69 };
        var str = personConfig.PrintToString(person);
        str.Should().Contain("420");
        str.Should().Contain("1337");
        str.Should().Contain("69");
        Console.WriteLine(str);
    }

    [Test]
    public void PrintToString_ShouldExclude_ExcludedInsideCollections()
    {
        var person = person1;
        person.FavoriteNumbers = new List<int> { 420, 1337, 69 };
        var str = personConfig.For<int>().Exclude().PrintToString(person);
        str.Should().NotContain("420");
        str.Should().NotContain("1337");
        str.Should().NotContain("69");
        Console.WriteLine(str);
    }
    
    [Test]
    public void PrintToString_ShouldPrintDictionaries_InsideClass()
    {
        var person = person1;
        person.Tasks = new Dictionary<string, DateTime> { {"do homework", DateTime.Today}, {"bake a cake", DateTime.Now}};
        var str = personConfig.For<int>().Exclude().PrintToString(person);
        str.Should().Contain("Key = do homework");
        str.Should().Contain($"Value = {DateTime.Today}");
        str.Should().Contain("Key = bake a cake");
        str.Should().Contain($"Value = {DateTime.Now}");
        Console.WriteLine(str);
    }
}