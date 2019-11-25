using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrintingHomeTask.ObjectPrinting;
using ObjectPrintingHomeTask.PropertyConfig;
using ObjectPrintingHomeTask.Tests.TestClass;

namespace ObjectPrintingHomeTask.Tests.ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19, Parent = new Person() {Name = "jewe"}};

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => "")
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Name).Using(x => x.Trim())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing<string>().TrimmedToLength(2)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();
            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Excluding(p => p.Age).Excluding(p => p.Parent.Name).Printing<double>().Using(x => "f32ew"));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }


        [Test]
        public void Should_ReturnSameObject_When_NoConfiguration()
        {
            var person = new Person() { Name = "Alex", Age = 19, Height = 1.8,
                                        Parent = new Person() { Name = "Jack" } };
            var expectedString = "Person 0" + Environment.NewLine +
                                 "\tId = Guid 1" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = 19" + Environment.NewLine +
                                 "\tParent = Person 2" + Environment.NewLine +
                                 "\t\tId = Guid 1" + Environment.NewLine +
                                 "\t\tName = Jack" + Environment.NewLine +
                                 "\t\tHeight = 0" + Environment.NewLine +
                                 "\t\tAge = 0" + Environment.NewLine +
                                 "\t\tParent = null" + Environment.NewLine;
            person
                .PrintToString()
                .Should()
                .BeEquivalentTo(expectedString);
        }

        [Test]
        public void Should_ReturnObject_Without_ExcludedType()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.8,
                                        Parent = new Person() { Name = "Jack" } };
            var expectedString = "Person 0" + Environment.NewLine +
                                 "\tId = Guid 1" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tParent = Person 2" + Environment.NewLine +
                                 "\t\tId = Guid 1" + Environment.NewLine +
                                 "\t\tName = Jack" + Environment.NewLine +
                                 "\t\tHeight = 0" + Environment.NewLine +
                                 "\t\tParent = null" + Environment.NewLine;
            person
                .PrintToString(p => p.Excluding<int>())
                .Should()
                .BeEquivalentTo(expectedString);
        }

        [Test]
        public void Should_ReturnObject_Without_ExcludedField()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.8,
                                        Parent = new Person() { Name = "Jack" } };
            var expectedString = "Person 0" + Environment.NewLine +
                                 "\tId = Guid 1" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = 19" + Environment.NewLine +
                                 "\tParent = Person 2" + Environment.NewLine +
                                 "\t\tId = Guid 1" + Environment.NewLine +
                                 "\t\tName = Jack" + Environment.NewLine +
                                 "\t\tHeight = 0" + Environment.NewLine +
                                 "\t\tAge = 0" + Environment.NewLine +
                                 "\t\tParent = null" + Environment.NewLine;
            person
                .PrintToString(p => p.Excluding(f => f.Name))
                .Should()
                .BeEquivalentTo(expectedString);
        }

        [Test]
        public void Should_Work_With_ExcludingInNestedTypes()
        {
            var person = new Person() { Name = "Alex", Age = 19, Height = 1.8,
                                        Parent = new Person() { Name = "Jack" } };
            var expectedString = "Person 0" + Environment.NewLine +
                                 "\tId = Guid 1" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tParent = Person 2" + Environment.NewLine +
                                 "\t\tId = Guid 1" + Environment.NewLine +
                                 "\t\tHeight = 0" + Environment.NewLine +
                                 "\t\tAge = 0" + Environment.NewLine +
                                 "\t\tParent = null" + Environment.NewLine;
            person
                .PrintToString(p => p.Excluding(f => f.Age).Excluding(f => f.Parent.Name))
                .Should()
                .BeEquivalentTo(expectedString);
        }

        [Test]
        public void Should_SerializedTypes()
        {
            var person = new Person() { Name = "Alex", Age = 19, Height = 1.8,
                                        Parent = new Person() { Name = "Jack" } };
            var expectedString = "Person 0" + Environment.NewLine +
                                 "\tId = Guid 1" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = someString" + Environment.NewLine +
                                 "\tParent = Person 2" + Environment.NewLine +
                                 "\t\tId = Guid 1" + Environment.NewLine +
                                 "\t\tName = Jack" + Environment.NewLine +
                                 "\t\tHeight = 0" + Environment.NewLine +
                                 "\t\tAge = someString" + Environment.NewLine +
                                 "\t\tParent = null" + Environment.NewLine;
            person
                .PrintToString(p => p.Printing<int>().Using(i => "someString"))
                .Should()
                .BeEquivalentTo(expectedString);
        }

        [Test]
        public void Should_SerializeProperties()
        {
            var person = new Person() { Name = "Alex", Age = 19, Height = 1.8,
                                        Parent = new Person() { Name = "Jack" } };
            var expectedString = "Person 0" + Environment.NewLine +
                                 "\tId = Guid 1" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = someString" + Environment.NewLine +
                                 "\tParent = Person 2" + Environment.NewLine +
                                 "\t\tId = Guid 1" + Environment.NewLine +
                                 "\t\tName = Jack" + Environment.NewLine +
                                 "\t\tHeight = 0" + Environment.NewLine +
                                 "\t\tAge = 0" + Environment.NewLine +
                                 "\t\tParent = null" + Environment.NewLine;
            person
                .PrintToString(p => p.Printing(per => per.Age).Using(i => "someString"))
                .Should()
                .BeEquivalentTo(expectedString);
        }

        [Test]
        public void Should_SerializeNestedProperties()
        {
            var person = new Person() { Name = "Alex", Age = 19, Height = 1.8,
                                        Parent = new Person() { Name = "Jack" } };
            var expectedString = "Person 0" + Environment.NewLine +
                                 "\tId = Guid 1" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = 19" + Environment.NewLine +
                                 "\tParent = Person 2" + Environment.NewLine +
                                 "\t\tId = Guid 1" + Environment.NewLine +
                                 "\t\tName = Jack" + Environment.NewLine +
                                 "\t\tHeight = someString" + Environment.NewLine +
                                 "\t\tAge = 0" + Environment.NewLine +
                                 "\t\tParent = null" + Environment.NewLine;
            person
                .PrintToString(p => p.Printing(per => per.Parent.Height).Using(i => "someString"))
                .Should()
                .BeEquivalentTo(expectedString);
        }

        [Test]
        public void Should_SetCulture_For_Double()
        {
            var person = new Person() { Name = "Alex", Age = 19, Height = 1.8,
                                        Parent = new Person() { Name = "Jack" } };
            var expectedString = "Person 0" + Environment.NewLine +
                                 "\tId = Guid 1" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1.8" + Environment.NewLine +
                                 "\tAge = 19" + Environment.NewLine +
                                 "\tParent = Person 2" + Environment.NewLine +
                                 "\t\tId = Guid 1" + Environment.NewLine +
                                 "\t\tName = Jack" + Environment.NewLine +
                                 "\t\tHeight = 0" + Environment.NewLine +
                                 "\t\tAge = 0" + Environment.NewLine +
                                 "\t\tParent = null" + Environment.NewLine;
            person
                .PrintToString(p => p.Printing<double>().Using(CultureInfo.GetCultureInfo("en-US")))
                .Should()
                .BeEquivalentTo(expectedString);
        }

        [Test]
        public void Should_TrimmedToLengthAllStringType()
        {
            var person = new Person() { Name = "Alex", Age = 19, Height = 1.8,
                                        Parent = new Person() { Name = "Jack" } };
            var expectedString = "Person 0" + Environment.NewLine +
                                 "\tId = Guid 1" + Environment.NewLine +
                                 "\tName = A" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = 19" + Environment.NewLine +
                                 "\tParent = Person 2" + Environment.NewLine +
                                 "\t\tId = Guid 1" + Environment.NewLine +
                                 "\t\tName = J" + Environment.NewLine +
                                 "\t\tHeight = 0" + Environment.NewLine +
                                 "\t\tAge = 0" + Environment.NewLine +
                                 "\t\tParent = null" + Environment.NewLine;
            person
                .PrintToString(p => p.Printing<string>().TrimmedToLength(1))
                .Should()
                .BeEquivalentTo(expectedString);
        }

        [Test]
        public void Should_TrimmedToLengthAllStringProperty()
        {
            var person = new Person() { Name = "Alex", Age = 19, Height = 1.8,
                                        Parent = new Person() { Name = "Jack" } };
            var expectedString = "Person 0" + Environment.NewLine +
                                 "\tId = Guid 1" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = 19" + Environment.NewLine +
                                 "\tParent = Person 2" + Environment.NewLine +
                                 "\t\tId = Guid 1" + Environment.NewLine +
                                 "\t\tName = J" + Environment.NewLine +
                                 "\t\tHeight = 0" + Environment.NewLine +
                                 "\t\tAge = 0" + Environment.NewLine +
                                 "\t\tParent = null" + Environment.NewLine;
            person
                .PrintToString(p => p.Printing(s => s.Parent.Name).TrimmedToLength(1))
                .Should()
                .BeEquivalentTo(expectedString);
        }

        [Test]
        public void Should_Work_WithCyclicReferences()
        {
            var person1 = new Person()
            {
                Name = "Alex",
                Age = 19,
                Height = 1.8,
            };
            var person2 = new Person()
            {
                Name = "Jack",
                Parent = person1
            };
            person1.Parent = person2;
            var expectedString = "Person 0" + Environment.NewLine +
                                 "\tId = Guid 1" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = 19" + Environment.NewLine +
                                 "\tParent = Person 2" + Environment.NewLine +
                                 "\t\tId = Guid 1" + Environment.NewLine +
                                 "\t\tName = Jack" + Environment.NewLine +
                                 "\t\tHeight = 0" + Environment.NewLine +
                                 "\t\tAge = 0" + Environment.NewLine +
                                 "\t\tParent = Person 0" + Environment.NewLine;
            person1.PrintToString().Should().BeEquivalentTo(expectedString);
        }

        [Test]
        public void Should_PrintDictionary_With_CircleReferences()
        {
            var person1 = new Person { Name = "Alex", Age = 19, Height = 1.8 };
            var person2 = new Person { Name = "Jack", Parent = person1};
            person1.Parent = person2;
            var persons = new Dictionary<int, Person> { {0, person1}, {1, person2} };
            var expectedString = "Dictionary`2" + Environment.NewLine +
                                 "KeyValuePair`2 0" + Environment.NewLine +
                                 "\tKey = 0" + Environment.NewLine +
                                 "\tValue = Person 1" + Environment.NewLine +
                                 "\t\tId = Guid 2" + Environment.NewLine +
                                 "\t\tName = Alex" + Environment.NewLine +
                                 "\t\tHeight = 1,8" + Environment.NewLine +
                                 "\t\tAge = 19" + Environment.NewLine +
                                 "\t\tParent = Person 3" + Environment.NewLine +
                                 "\t\t\tId = Guid 2" + Environment.NewLine +
                                 "\t\t\tName = Jack" + Environment.NewLine +
                                 "\t\t\tHeight = 0" + Environment.NewLine +
                                 "\t\t\tAge = 0" + Environment.NewLine +
                                 "\t\t\tParent = Person 1" + Environment.NewLine +
                                 "KeyValuePair`2 0" + Environment.NewLine +
                                 "\tKey = 1" + Environment.NewLine +
                                 "\tValue = Person 1" + Environment.NewLine +
                                 "\t\tId = Guid 2" + Environment.NewLine +
                                 "\t\tName = Jack" + Environment.NewLine +
                                 "\t\tHeight = 0" + Environment.NewLine +
                                 "\t\tAge = 0" + Environment.NewLine +
                                 "\t\tParent = Person 3" + Environment.NewLine +
                                 "\t\t\tId = Guid 2" + Environment.NewLine +
                                 "\t\t\tName = Alex" + Environment.NewLine +
                                 "\t\t\tHeight = 1,8" + Environment.NewLine +
                                 "\t\t\tAge = 19" + Environment.NewLine +
                                 "\t\t\tParent = Person 1" + Environment.NewLine;
            persons.PrintToString().Should().BeEquivalentTo(expectedString);
        }

        [Test]
        public void Should_PrintList()
        {
            var person1 = new Person { Name = "Alex", Age = 19, Height = 1.8 };
            var person2 = new Person { Name = "Jack" };
            var persons = new List<Person> { person1, person2 };
            var expectedString = "List`1" + Environment.NewLine +
                                 "Person 0" + Environment.NewLine +
                                 "\tId = Guid 1" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = 19" + Environment.NewLine +
                                 "\tParent = null" + Environment.NewLine +
                                 "Person 0" + Environment.NewLine +
                                 "\tId = Guid 1" + Environment.NewLine +
                                 "\tName = Jack" + Environment.NewLine +
                                 "\tHeight = 0" + Environment.NewLine +
                                 "\tAge = 0" + Environment.NewLine +
                                 "\tParent = null" + Environment.NewLine;
            persons.PrintToString().Should().BeEquivalentTo(expectedString);
        }

        [Test]
        public void Should_PrintArray()
        {
            var person1 = new Person { Name = "Alex", Age = 19, Height = 1.8 };
            var person2 = new Person { Name = "Jack" };
            Person[] persons = { person1, person2 };
            var expectedString = "Person[]" + Environment.NewLine +
                                 "Person 0" + Environment.NewLine +
                                 "\tId = Guid 1" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = 19" + Environment.NewLine +
                                 "\tParent = null" + Environment.NewLine +
                                 "Person 0" + Environment.NewLine +
                                 "\tId = Guid 1" + Environment.NewLine +
                                 "\tName = Jack" + Environment.NewLine +
                                 "\tHeight = 0" + Environment.NewLine +
                                 "\tAge = 0" + Environment.NewLine +
                                 "\tParent = null" + Environment.NewLine;
            persons.PrintToString().Should().BeEquivalentTo(expectedString);
        }
    }
}