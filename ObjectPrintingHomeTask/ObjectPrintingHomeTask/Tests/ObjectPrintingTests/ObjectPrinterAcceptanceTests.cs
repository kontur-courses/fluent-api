using System;
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
            var person = new Person { Name = "Alex", Age = 19 };

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
            string s3 = person.PrintToString(s => s.Excluding(p => p.Age).Excluding(p => p.Name));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }


        [Test]
        public void Should_ReturnSameObject_When_NoConfiguration()
        {
            var person = new Person() { Name = "Alex", Age = 19, Height = 1.8,
                                        Parent = new Person() { Name = "Jack" } };
            var expectedString = "Person" + Environment.NewLine +
                                 "\tId = Guid" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = 19" + Environment.NewLine +
                                 "\tParent = Person" + Environment.NewLine +
                                 "\t\tId = Guid" + Environment.NewLine +
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
            var person = new Person() { Name = "Alex", Age = 19, Height = 1.8,
                                        Parent = new Person() { Name = "Jack" } };
            var expectedString = "Person" + Environment.NewLine +
                                 "\tId = Guid" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tParent = Person" + Environment.NewLine +
                                 "\t\tId = Guid" + Environment.NewLine +
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
            var person = new Person() { Name = "Alex", Age = 19, Height = 1.8,
                                        Parent = new Person() { Name = "Jack" } };
            var expectedString = "Person" + Environment.NewLine +
                                 "\tId = Guid" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = 19" + Environment.NewLine +
                                 "\tParent = Person" + Environment.NewLine +
                                 "\t\tId = Guid" + Environment.NewLine +
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
            var expectedString = "Person" + Environment.NewLine +
                                 "\tId = Guid" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tParent = Person" + Environment.NewLine +
                                 "\t\tId = Guid" + Environment.NewLine +
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
            var expectedString = "Person" + Environment.NewLine +
                                 "\tId = Guid" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = someString" + Environment.NewLine +
                                 "\tParent = Person" + Environment.NewLine +
                                 "\t\tId = Guid" + Environment.NewLine +
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
            var expectedString = "Person" + Environment.NewLine +
                                 "\tId = Guid" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = someString" + Environment.NewLine +
                                 "\tParent = Person" + Environment.NewLine +
                                 "\t\tId = Guid" + Environment.NewLine +
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
            var expectedString = "Person" + Environment.NewLine +
                                 "\tId = Guid" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = 19" + Environment.NewLine +
                                 "\tParent = Person" + Environment.NewLine +
                                 "\t\tId = Guid" + Environment.NewLine +
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
            var expectedString = "Person" + Environment.NewLine +
                                 "\tId = Guid" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1.8" + Environment.NewLine +
                                 "\tAge = 19" + Environment.NewLine +
                                 "\tParent = Person" + Environment.NewLine +
                                 "\t\tId = Guid" + Environment.NewLine +
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
            var expectedString = "Person" + Environment.NewLine +
                                 "\tId = Guid" + Environment.NewLine +
                                 "\tName = A" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = 19" + Environment.NewLine +
                                 "\tParent = Person" + Environment.NewLine +
                                 "\t\tId = Guid" + Environment.NewLine +
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
            var expectedString = "Person" + Environment.NewLine +
                                 "\tId = Guid" + Environment.NewLine +
                                 "\tName = Alex" + Environment.NewLine +
                                 "\tHeight = 1,8" + Environment.NewLine +
                                 "\tAge = 19" + Environment.NewLine +
                                 "\tParent = Person" + Environment.NewLine +
                                 "\t\tId = Guid" + Environment.NewLine +
                                 "\t\tName = J" + Environment.NewLine +
                                 "\t\tHeight = 0" + Environment.NewLine +
                                 "\t\tAge = 0" + Environment.NewLine +
                                 "\t\tParent = null" + Environment.NewLine;
            person
                .PrintToString(p => p.Printing(s => s.Parent.Name).TrimmedToLength(1))
                .Should()
                .BeEquivalentTo(expectedString);
        }
    }
}