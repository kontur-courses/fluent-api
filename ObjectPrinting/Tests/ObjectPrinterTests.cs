using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    class ObjectPrinterTests
    {
        [Test]
        public void PrintToString_WorksCorrectly_OnComplicatedObject()
        {
            var father = new Person { Name = "Alexander", Age = 220, Id = Guid.Empty, Height = 160};
            var person = new Person { Name = "Alex", Age = 19, Id = Guid.Empty, Height = 182, Father = father };
            var printer = ObjectPrinter.For<Person>();
            var result = printer.PrintToString(person);
            result.Should().Be(String.Format("Person{0}" +
                                                 "\tId = 00000000-0000-0000-0000-000000000000{0}" +
                                                 "\tName = Alex{0}" +
                                                 "\tHeight = 182{0}" +
                                                 "\tAge = 19{0}" +
                                                 "\tFather = Person{0}" +
                                                     "\t\tId = 00000000-0000-0000-0000-000000000000{0}" +
                                                     "\t\tName = Alexander{0}" +
                                                     "\t\tHeight = 160{0}" +
                                                     "\t\tAge = 220{0}" +
                                                     "\t\tFather = null{0}", Environment.NewLine));
        }

        [Test]
        public void PrintToString_WorksCorrectly_OnList()
        {
            var list = new List<int> {1, 5, 6, 7, -5};
            var printer = ObjectPrinter.For<List<int>>();
            var result = printer.PrintToString(list);
            result.Should().Be(String.Format(
                "List`1{0}" +
                    "\t1{0}" +
                    "\t5{0}" +
                    "\t6{0}" +
                    "\t7{0}" +
                    "\t-5{0}", Environment.NewLine));
        }

        [Test]
        public void PrintToString_WorksCorrectly_OnDictionary()
        {
            var dict = new Dictionary<int, string> {{1, "one"}, {-3, "minus three"}};
            var printer = ObjectPrinter.For<Dictionary<int,string>>();
            var result = printer.PrintToString(dict);
            result.Should().Be(String.Format(
                "Dictionary`2{0}" +
                  "\tKeyValuePair`2{0}" +
                      "\t\tKey = 1{0}" +
                      "\t\tValue = one{0}" +
                  "\tKeyValuePair`2{0}" +
                      "\t\tKey = -3{0}" +
                      "\t\tValue = minus three{0}", Environment.NewLine));
        }

        [Test]
        public void PrintToString_WorksCorrectly_OnObjectWithListProperty()
        {
            var marks = new List<int> {2, 3, 2, 4};
            var student = new Student{FirstName = "Dima", SecondName = "Ivanovsky", Marks = marks};
            var printer = ObjectPrinter.For<Student>();
            var result = printer.PrintToString(student);
            result.Should().Be(String.Format(
                "Student{0}" +
                   "\tFirstName = Dima{0}" +
                   "\tSecondName = Ivanovsky{0}" +
                   "\tMarks = List`1{0}" +
                       "\t\t2{0}" +
                       "\t\t3{0}" +
                       "\t\t2{0}" +
                       "\t\t4{0}", Environment.NewLine));
    }

        [Test]
        public void PrintToString_DoesNotThrowException_OnObjectWithCycleReference()
        {
            var father = new Person();
            var person = new Person {Father = father };
            father.Father = person;

            var printer = ObjectPrinter.For<Person>();
            Action act = () => printer.PrintToString(person);
            act.ShouldNotThrow();
        }

        [Test]
        public void Excluding_WorksCorrectly_OnComplicatedObject()
        {
            var person = new Person { Name = "Son", Age = 19, Id = Guid.Empty, Height = 182};
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Person>();
            var result = printer.PrintToString(person);
            result.Should().Be(String.Format(
                "Person{0}" +
                   "\tId = 00000000-0000-0000-0000-000000000000{0}" +
                   "\tName = Son{0}" +
                   "\tHeight = 182{0}" +
                   "\tAge = 19{0}", Environment.NewLine));
        }

        [Test]
        public void Excluding_ExcludesType_InNestedObjects()
        {
            var father = new Person { Name = "Father", Age = 65, Id = Guid.Empty, Height = 160 };
            var person = new Person { Name = "Son", Age = 19, Id = Guid.Empty, Height = 182, Father = father };
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>();
            var result = printer.PrintToString(person);
            result.Should().Be(String.Format(
                "Person{0}" +
                    "\tName = Son{0}" +
                    "\tHeight = 182{0}" +
                    "\tAge = 19{0}" +
                    "\tFather = Person{0}" +
                        "\t\tName = Father{0}" +
                        "\t\tHeight = 160{0}" +
                        "\t\tAge = 65{0}" +
                        "\t\tFather = null{0}", Environment.NewLine));
        }

        [Test]
        public void Excluding_ExcludesCorrectly_OnObjectWithListProperty()
        {
            var marks = new List<int> { 2, 3, 2, 4 };
            var student = new Student { FirstName = "Dima", SecondName = "Ivanovsky", Marks = marks };
            var printer = ObjectPrinter.For<Student>()
                .Excluding<int>();
            var result = printer.PrintToString(student);
            result.Should().Be(String.Format(
                "Student{0}" +
                    "\tFirstName = Dima{0}" +
                    "\tSecondName = Ivanovsky{0}" +
                    "\tMarks = List`1{0}", Environment.NewLine));
        }

        [Test]
        public void AlternativeTypePrinting_WorksCorrectly_OnComplicatedObject()
        {
            var person = new Person { Name = "Alex", Age = 19, Id = Guid.Empty, Height = 182 };
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(i => "X");
            var result = printer.PrintToString(person);
            result.Should().Be(String.Format(
                "Person{0}" +
                    "\tId = 00000000-0000-0000-0000-000000000000{0}" +
                    "\tName = Alex{0}" +
                    "\tHeight = 182{0}" +
                    "\tAge = X{0}" +
                    "\tFather = null{0}", Environment.NewLine));
        }

        [Test]
        public void AlternativeTypePrinting_UsesAlternativeWay_InNestedProperties()
        {
            var father = new Person { Name = "Alexander", Age = 220, Id = Guid.Empty, Height = 160 };
            var person = new Person { Name = "Alex", Age = 19, Id = Guid.Empty, Height = 182, Father = father };
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(i => "X");
            var result = printer.PrintToString(person);
            result.Should().Be(String.Format(
                "Person{0}" +
                   "\tId = 00000000-0000-0000-0000-000000000000{0}" +
                   "\tName = Alex{0}" +
                   "\tHeight = 182{0}" +
                   "\tAge = X{0}" +
                   "\tFather = Person{0}" +
                        "\t\tId = 00000000-0000-0000-0000-000000000000{0}" +
                        "\t\tName = Alexander{0}" +
                        "\t\tHeight = 160{0}" +
                        "\t\tAge = X{0}" +
                        "\t\tFather = null{0}", Environment.NewLine));
        }

        [Test]
        public void AlternativeTypePrinting_WorksCorrectly_OnList()
        {
            var list = new List<Person> { new Person{ Name = "First" }, new Person{ Name = "Second"} };
            var printer = ObjectPrinter.For<List<Person>>()
                .Printing<Person>().Using(i => "Some Person");
            var result = printer.PrintToString(list);
            result.Should().Be(String.Format(
                "List`1{0}" +
                   "\tSome Person{0}" +
                   "\tSome Person{0}", Environment.NewLine));
        }

        [Test]
        public void AlternativeCulture_WorksCorrectly_OnComplicatedObject()
        {
            var person = new Person { Name = "Alex", Age = 19, Id = Guid.Empty, Height = 182.5 };
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(new CultureInfo("en-US"));
            var result = printer.PrintToString(person);
            result.Should().Be(String.Format(
                "Person{0}" +
                   "\tId = 00000000-0000-0000-0000-000000000000{0}" +
                   "\tName = Alex{0}" +
                   "\tHeight = 182.5{0}" +
                   "\tAge = 19{0}" +
                   "\tFather = null{0}", Environment.NewLine));
        }

        [Test]
        public void AlternativeCulture_UsesThisCulture_InNestedProperties()
        {
            var father = new Person { Name = "Alexander", Age = 220, Id = Guid.Empty, Height = 160.2 };
            var person = new Person { Name = "Alex", Age = 19, Id = Guid.Empty, Height = 182.5, Father = father };
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(new CultureInfo("en-US"));
            var result = printer.PrintToString(person);
            result.Should().Be(String.Format(
                "Person{0}" +
                   "\tId = 00000000-0000-0000-0000-000000000000{0}" +
                   "\tName = Alex{0}" +
                   "\tHeight = 182.5{0}" +
                   "\tAge = 19{0}" +
                   "\tFather = Person{0}" +
                       "\t\tId = 00000000-0000-0000-0000-000000000000{0}" +
                       "\t\tName = Alexander{0}" +
                       "\t\tHeight = 160.2{0}" +
                       "\t\tAge = 220{0}" +
                       "\t\tFather = null{0}", Environment.NewLine));
        }

        [Test]
        public void AlternativeCulture_WorksCorrectly_OnList()
        {
            var list = new List<double> {1.1, 2.2, 3.3};
            var printer = ObjectPrinter.For<List<double>>()
                .Printing<double>().Using(new CultureInfo("en-US"));
            var result = printer.PrintToString(list);
            result.Should().Be(String.Format(
                "List`1{0}" +
                   "\t1.1{0}" +
                   "\t2.2{0}" +
                   "\t3.3{0}", Environment.NewLine));
        }


        [Test]
        public void AlternativePropertyPrinting_WorksCorrectly_OnComplicatedObjects()
        {
            var person = new Person { Name = "Alex", Age = 19, Id = Guid.Empty, Height = 182 };
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(i => "Almost " + i);
            var result = printer.PrintToString(person);
            result.Should().Be(String.Format(
                "Person{0}" +
                   "\tId = 00000000-0000-0000-0000-000000000000{0}" +
                   "\tName = Alex{0}" +
                   "\tHeight = 182{0}" +
                   "\tAge = Almost 19{0}" +
                   "\tFather = null{0}", Environment.NewLine));
        }

        [Test]
        public void AlternativePropertyPrinting_IsUsedOnlyOnFirstNestingLevel_InNestedProperties()
        {
            var father = new Person { Name = "Alexander", Age = 220, Id = Guid.Empty, Height = 160 };
            var person = new Person { Name = "Alex", Age = 19, Id = Guid.Empty, Height = 182, Father = father};
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).Using(i => "not " +  i );
            var result = printer.PrintToString(person);
            result.Should().Be(String.Format(
                "Person{0}" +
                   "\tId = 00000000-0000-0000-0000-000000000000{0}" +
                   "\tName = not Alex{0}" +
                   "\tHeight = 182{0}" +
                   "\tAge = 19{0}" +
                   "\tFather = Person{0}" +
                       "\t\tId = 00000000-0000-0000-0000-000000000000{0}" +
                       "\t\tName = Alexander{0}" +
                       "\t\tHeight = 160{0}" +
                       "\t\tAge = 220{0}" +
                       "\t\tFather = null{0}", Environment.NewLine));
        }

        [Test]
        public void TrimMethod_WorksCorrectly_OnComplicatedObjects()
        {
            var person = new Person { Name = "Alexander", Age = 19, Id = Guid.Empty, Height = 182 };
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(4);
            var result = printer.PrintToString(person);
            result.Should().Be(String.Format(
                "Person{0}" +
                   "\tId = 00000000-0000-0000-0000-000000000000{0}" +
                   "\tName = Alex{0}" +
                   "\tHeight = 182{0}" +
                   "\tAge = 19{0}" +
                   "\tFather = null{0}", Environment.NewLine));
        }

        [Test]
        public void TrimMethod_IsUsedOnlyOnFirstNestingLevel_InNestedProperties()
        {
            var father = new Person { Name = "Alexander", Age = 220, Id = Guid.Empty, Height = 160 };
            var person = new Person { Name = "Alexander", Age = 19, Id = Guid.Empty, Height = 182, Father = father };
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(4);
            var result = printer.PrintToString(person);
            result.Should().Be(String.Format(
                "Person{0}" +
                   "\tId = 00000000-0000-0000-0000-000000000000{0}" +
                   "\tName = Alex{0}" +
                   "\tHeight = 182{0}" +
                   "\tAge = 19{0}" +
                   "\tFather = Person{0}" +
                       "\t\tId = 00000000-0000-0000-0000-000000000000{0}" +
                       "\t\tName = Alexander{0}" +
                       "\t\tHeight = 160{0}" +
                       "\t\tAge = 220{0}" +
                       "\t\tFather = null{0}", Environment.NewLine));
        }

        [Test]
        public void ExcludingProperty_IsUsedOnlyOnFirstNestingLevel_InNestedProperties()
        {
            var person = new Person { Name = "Alex", Age = 19, Id = Guid.Empty, Height = 182 };
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age);
            var result = printer.PrintToString(person);
            result.Should().Be(String.Format(
                "Person{0}" +
                   "\tId = 00000000-0000-0000-0000-000000000000{0}" +
                   "\tName = Alex{0}" +
                   "\tHeight = 182{0}" +
                   "\tFather = null{0}", Environment.NewLine));
        }

        [Test]
        public void ExcludingProperty_WorksCorrectly_OnComplicatedObject()
        {
            var father = new Person { Name = "Alexander", Age = 220, Id = Guid.Empty, Height = 160 };
            var person = new Person { Name = "Alex", Age = 19, Id = Guid.Empty, Height = 182, Father = father };
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age);
            var result = printer.PrintToString(person);
            result.Should().Be(String.Format(
                "Person{0}" +
                   "\tId = 00000000-0000-0000-0000-000000000000{0}" +
                   "\tName = Alex{0}" +
                   "\tHeight = 182{0}" +
                   "\tFather = Person{0}" +
                       "\t\tId = 00000000-0000-0000-0000-000000000000{0}" +
                       "\t\tName = Alexander{0}" +
                       "\t\tHeight = 160{0}" +
                       "\t\tAge = 220{0}" +
                       "\t\tFather = null{0}", Environment.NewLine));
        }

        [Test]
        public void ObjectExtensionPrintToString_WorksCorrectly_OnComplicatedObject()
        {
            var person = new Person { Name = "Alex", Age = 19, Id = Guid.Empty, Height = 182 };
            var result = person.PrintToString();
            result.Should().Be(String.Format(
                "Person{0}" +
                   "\tId = 00000000-0000-0000-0000-000000000000{0}" +
                   "\tName = Alex{0}" +
                   "\tHeight = 182{0}" +
                   "\tAge = 19{0}" +
                   "\tFather = null{0}", Environment.NewLine));
        }

        [Test]
        public void PrintToStringWithConfigurations_WorksCorrectly()
        {
            var person = new Person { Name = "Alex", Age = 19, Id = Guid.Empty, Height = 182 };
            var result = person.PrintToString(s => s.Excluding(p => p.Age).Printing<double>().Using(i => "XXX"));
            result.Should().Be(String.Format(
                "Person{0}" +
                   "\tId = 00000000-0000-0000-0000-000000000000{0}" +
                   "\tName = Alex{0}" +
                   "\tHeight = XXX{0}" +
                   "\tFather = null{0}", Environment.NewLine));
        }

        [Test]
        public void PrintToString_WorksCorrectly_WithCombinationOfMethods1()
        {
            var father = new Person { Name = "Alexander", Age = 43, Id = Guid.Empty, Height = 168.5 };
            var person = new Person { Name = "Dmitry", Age = 19, Id = Guid.Empty, Height = 173.5, Father = father };
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<double>().Using(new CultureInfo("en-US"))
                .Printing<int>().Using(i => i + " years")
                .Printing<string>().Using(i => i + " Woodman");
            var result = printer.PrintToString(person);
            result.Should().Be(String.Format(
                "Person{0}" +
                "\tName = Dmitry Woodman{0}" +
                "\tHeight = 173.5{0}" +
                "\tAge = 19 years{0}" +
                "\tFather = Person{0}" +
                    "\t\tName = Alexander Woodman{0}" +
                    "\t\tHeight = 168.5{0}" +
                    "\t\tAge = 43 years{0}" +
                    "\t\tFather = null{0}", Environment.NewLine));
        }

        [Test]
        public void PrintToString_WorksCorrectly_WithCombinationOfMethods2()
        {
            var marks = new List<int> { 2, 3, 2, 4 };
            var student = new Student { FirstName = "Dima", SecondName = "Ivanovsky is not so long second name", Marks = marks };
            var printer = ObjectPrinter.For<Student>()
                .Printing(p => p.SecondName).TrimmedToLength(9)
                .Excluding(p => p.FirstName);
            var result = printer.PrintToString(student);
            result.Should().Be(String.Format(
                "Student{0}" +
                    "\tSecondName = Ivanovsky{0}" +
                    "\tMarks = List`1{0}" +
                        "\t\t2{0}" +
                        "\t\t3{0}" +
                        "\t\t2{0}" +
                        "\t\t4{0}", Environment.NewLine));
        }

        [Test]
        public void PrintToString_WorksCorrectly_WithOverwrittenMethods()
        {
            var marks = new List<int> { 2, 3, 2, 4 };
            var student = new Student { FirstName = "Dima", SecondName = "Ivanovsky", Marks = marks };
            var printer = ObjectPrinter.For<Student>()
                .Printing(s => s.FirstName).Using(n => n + "1")
                .Printing(s => s.FirstName).Using(n => n + "2")
                .Printing(s => s.FirstName).Using(n => n + "3");
            var result = printer.PrintToString(student);
            result.Should().Be(String.Format(
                "Student{0}" +
                "\tFirstName = Dima3{0}" +
                "\tSecondName = Ivanovsky{0}" +
                "\tMarks = List`1{0}" +
                "\t\t2{0}" +
                "\t\t3{0}" +
                "\t\t2{0}" +
                "\t\t4{0}", Environment.NewLine));
        }
    }
}
