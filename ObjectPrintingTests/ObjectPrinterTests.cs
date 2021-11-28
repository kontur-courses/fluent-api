using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    public class ObjectPrinterTests
    {
        private readonly string newLine = Environment.NewLine; 

        [Test]
        public void ForWithoutSettings_ShouldPrintAllPropertiesAndFields()
        {
            var pet = new Pet { Name = "Bob", Age = 5, Owner = null };
            var printer = ObjectPrinter.For<Pet>();

            printer.PrintToString(pet).Should()
                .Be($"{nameof(Pet)}{newLine}" +
                    $"\tAge = {pet.Age}{newLine}" +
                    $"\tOwner = null{newLine}" +
                    $"\tName = {pet.Name}{newLine}");
        }

        [Test]
        public void ExcludingType_ShouldExcludeAllPropertiesAndFieldsWithType()
        {
            var pet = new Pet { Name = "Bob", Age = 5, Owner = null };
            var printer = ObjectPrinter.For<Pet>()
                .Excluding<Person>();

            printer.PrintToString(pet).Should()
                .Be($"{nameof(Pet)}{newLine}" +
                    $"\tAge = {pet.Age}{newLine}" +
                    $"\tName = {pet.Name}{newLine}");
        }

        [Test]
        public void ExcludingProperty_ShouldExcludeOneProperty()
        {
            var pet = new Pet { Name = "Bob", Age = 5, Owner = null };
            var printer = ObjectPrinter.For<Pet>()
                .Excluding(p => p.Owner);

            printer.PrintToString(pet).Should()
                .Be($"{nameof(Pet)}{newLine}" +
                    $"\tAge = {pet.Age}{newLine}" +
                    $"\tName = {pet.Name}{newLine}");
        }

        [Test]
        public void ExcludingField_ShouldExcludeThisField()
        {
            var pet = new Pet { Name = "Bob", Age = 5, Owner = null };
            var printer = ObjectPrinter.For<Pet>()
                .Excluding(p => p.Name);

            printer.PrintToString(pet).Should()
                .Be($"{nameof(Pet)}{newLine}" +
                    $"\tAge = {pet.Age}{newLine}" +
                    $"\tOwner = null{newLine}");
        }

        [Test]
        public void PrintingTypeWithUsing_ShouldAddSerializerForPropertiesAndFieldsWithType()
        {
            var pet = new Pet { Name = "Bob", Age = 5, Owner = null };
            var printer = ObjectPrinter.For<Pet>()
                .Printing<Person>().Using(p => p == null ? "No owner" : p.Name);

            printer.PrintToString(pet).Should()
                .Be($"{nameof(Pet)}{newLine}" +
                    $"\tAge = {pet.Age}{newLine}" +
                    $"\tOwner = No owner{newLine}" +
                    $"\tName = {pet.Name}{newLine}");
        }

        [Test]
        public void PrintingPropertyWithUsing_ShouldAddSerializerForThisProperty()
        {
            var pet = new Pet { Name = "Bob", Age = 5, Owner = null };

            var printer = ObjectPrinter.For<Pet>()
                .Printing(p => p.Age).Using(age => (age * 7) + "(cat years)");

            printer.PrintToString(pet).Should()
                .Be($"{nameof(Pet)}{newLine}" +
                    $"\tAge = {pet.Age * 7}(cat years){newLine}" +
                    $"\tOwner = null{newLine}" +
                    $"\tName = {pet.Name}{newLine}");
        }

        [Test]
        public void PrintingFiledWithUsing_ShouldAddSerializerForThisField()
        {
            var pet = new Pet { Name = "Bob", Age = 5, Owner = null };

            var printer = ObjectPrinter.For<Pet>()
                .Printing(p => p.Name).Using(name => name.ToUpper());

            printer.PrintToString(pet).Should()
                .Be($"{nameof(Pet)}{newLine}" +
                    $"\tAge = {pet.Age}{newLine}" +
                    $"\tOwner = null{newLine}" +
                    $"\tName = {pet.Name.ToUpper()}{newLine}");
        }

        [Test]
        public void ExcludingAndPrinting_ShouldBeFluent()
        {
            var pet = new Pet { Name = "Bob", Age = 5, Owner = null };

            var printer = ObjectPrinter.For<Pet>()
                .Printing(p => p.Name).Using(name => name.ToUpper())
                .Excluding<Person>()
                .Printing(p => p.Age).Using(age => age * 7 + "(cat years)");

            printer.PrintToString(pet).Should()
                .Be($"{nameof(Pet)}{newLine}" +
                    $"\tAge = {pet.Age * 7}(cat years){newLine}" +
                    $"\tName = {pet.Name.ToUpper()}{newLine}");
        }

        [Test]
        public void PrintingFromattableType_ShouldBeAbleToChooseCulture()
        {
            var person = new Person() {Height = 177.5, Weight = 64.1};

            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>()
                .Excluding<string>()
                .Excluding<Person>()
                .Excluding<Guid>()
                .Printing<double>().Using(CultureInfo.InvariantCulture);

            printer.PrintToString(person).Should()
                .Be($"{nameof(Person)}{newLine}" +
                    $"\tHeight = 177.5{newLine}" +
                    $"\tWeight = 64.1{newLine}");
        }

        [Test]
        public void PrintingString_ShouldBeAbleToTrimToLength()
        {
            var person = new Person() { Name = "Alexey Surname Should Be Trimmed"};

            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>()
                .Excluding<Person>()
                .Excluding<Guid>()
                .Excluding<double>()
                .Printing<string>().TrimToLength(6);

            printer.PrintToString(person).Should()
                .Be($"{nameof(Person)}{newLine}" +
                    $"\tName = {person.Name[..6]}{newLine}");
        }

        [Test]
        public void PrintToString_ShouldSkipCyclicReferences()
        {
            var person = new Person { Name = "Alexey" };
            var brother = new Person {Name = "Ivan", Brother = person};
            person.Brother = brother;

            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>()
                .Excluding<Guid>()
                .Excluding<double>();

            printer.PrintToString(person).Should()
                .Be($"{nameof(Person)}{newLine}" +
                    $"\tName = {person.Name}{newLine}" +
                    $"\tBrother = {nameof(Person)}{newLine}" +
                    $"\t\tName = {brother.Name}{newLine}");
        }

        [Test]
        public void PrintToString_ShouldSerializeDictionary()
        {
            var dict = new Dictionary<string, int>
            {
                {"1", 1},
                {"2", 2},
                {"3", 3}
            };

            dict.PrintToString().Should()
                .ContainAll("Key = 1", "Value = 1", 
                    "Key = 2", "Value = 2", 
                    "Key = 3", "Value = 3");
        }

        [Test]
        public void PrintToString_ShouldSerializeEnumerable()
        {
            var list = new List<Pet>
            {
                new Pet {Age = 1, Name = "Bob"},
                new Pet {Age = 2, Name = "Bars"},
                new Pet {Age = 3, Name = "Mars"}
            };

            list.PrintToString().Should()
                .ContainAll("Pet", $"Age = {list[0].Age}", $"Name = {list[0].Name}",
                    $"Age = {list[1].Age}", $"Name = {list[1].Name}",
                    $"Age = {list[2].Age}", $"Name = {list[2].Name}");
        }
    }
}
