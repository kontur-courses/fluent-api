using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        [Explicit]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);

            string s1 = printer.PrintToString(person);
            
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();
            
            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }
        
        [Test]
        public void Excluding_Guid_OmitsPublicProperty()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>();
            var serialisedBeforeExcluding = printer.PrintToString(person);
            
            var serialisedAfterExcluding = printer
                .Excluding<Guid>()
                .PrintToString(person);
            
            serialisedBeforeExcluding.Should().Contain("\tId = ");
            
            serialisedAfterExcluding.Should().NotContain("\tId = ");
            serialisedAfterExcluding.Should().Contain("\tName = Alex");
            serialisedAfterExcluding.Should().Contain("\tAge = 19");
        }
        
        [Test]
        public void Excluding_Guid_OmitsPublicField()
        {
            var person = new PersonWithField { Name = "Alex", Age = 19, Gender = "Male" };
            var printer = ObjectPrinter.For<PersonWithField>();
            var serialisedBeforeExcluding = printer.PrintToString(person);

            
            var serialisedAfterExcluding = printer
                .Excluding<Guid>()
                .PrintToString(person);
            
            serialisedBeforeExcluding.Should().Contain("\tId = ");
            
            serialisedAfterExcluding.Should().NotContain("\tId = ");
            serialisedAfterExcluding.Should().Contain("\tName = Alex");
            serialisedAfterExcluding.Should().Contain("\tAge = 19");
            serialisedAfterExcluding.Should().Contain("\tGender = Male");
        }

        [Test]
        public void Excluding_Age_OmitsPublicProperty()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>();
            var serialisedBeforeExcluding = printer.PrintToString(person);
            
            var serialisedAfterExcluding = printer
                .Excluding(p => p.Age)
                .PrintToString(person);
            
            serialisedBeforeExcluding.Should().Contain("\tAge = ");
            
            serialisedAfterExcluding.Should().NotContain("\tAge = ");
            serialisedAfterExcluding.Should().Contain("\tName = Alex");
        }
        
        [Test]
        public void Excluding_Gender_OmitsPublicField()
        {
            var person = new PersonWithField { Name = "Alex", Age = 19, Gender = "Male" };
            var printer = ObjectPrinter.For<PersonWithField>();
            var serialisedBeforeExcluding = printer.PrintToString(person);

            var serialisedAfterExcluding = printer
                .Excluding(p => p.Gender)
                .PrintToString(person);
            
            serialisedBeforeExcluding.Should().Contain("\tGender = ");
            
            serialisedAfterExcluding.Should().NotContain("\tGender = ");
            serialisedAfterExcluding.Should().Contain("\tName = Alex");
            serialisedAfterExcluding.Should().Contain("\tAge = 19");
        }

        [Test]
        public void Printing_ByType_FormatsPublicFieldsAndPropertiesOfThatType()
        {
            var person = new SampleWithIntMembers();

            var result = ObjectPrinter.For<SampleWithIntMembers>()
                .Printing<int>().Using(value => value.ToString("X"))
                .PrintToString(person);

            var lines = result.Split(Environment.NewLine);

            lines.Should().Contain("\tAge = 13");
            lines.Should().Contain("\tScore = 1A");
            lines.Should().Contain("\tName = Alex");
        }
        
        [Test]
        public void Printing_BySelector_FormatsOnlySelectedMember()
        {
            var person = new SampleWithIntMembers(); // Age = 19, Score = 26

            var result = ObjectPrinter.For<SampleWithIntMembers>()
                .Printing(p => p.Age).Using(value => value.ToString("X"))
                .PrintToString(person);

            var lines = result.Split(Environment.NewLine);

            lines.Should().Contain("\tAge = 13");
            lines.Should().Contain("\tScore = 26");
        }
        
        private sealed class SampleWithIntMembers
        {
            public int Age { get; set; } = 19;
            public int Score = 26;
            public string Name { get; set; } = "Alex";
        }
    }
}