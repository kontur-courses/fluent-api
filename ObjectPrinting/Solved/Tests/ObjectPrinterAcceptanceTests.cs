using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Solved.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;
        private PrintingConfig<Person> printer;
        [Test]
        public void Demo()
        {
            person = new Person { Name = "Alex", Age = 19 };

            printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Name).Using(s => s.Substring(0,2))
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);

            string s1 = printer.PrintToString(person);
            
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();
            
            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Excluding(p => p.Height));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        [SetUp]
        public void SetUp()
        {
            person = new Person()
            {
                Name = "Alex", 
                Age = 19, 
                Height = 185.5, 
                Id = new Guid()
            };
            printer = ObjectPrinter.For<Person>();
        }

        [Test]
        public void NotConfiguratedPrinterTest()
        {
            var result = printer.PrintToString(person);
            Console.WriteLine(printer.PrintToString(person));
            result
                .Should()
                .ContainAll(
                    $"Name = {person.Name}",
                    $"Age = {person.Age}",
                    $"Height = {person.Height}",
                    "Id = Guid");
        }
        
        [Test]
        public void ExcludePropertyTest()
        {
            printer
                .Excluding(p => p.Name);
            var result = printer.PrintToString(person);
            Console.WriteLine(printer.PrintToString(person));
            result
                .Should()
                .ContainAll(
                    $"Age = {person.Age}",
                    $"Height = {person.Height}",
                    "Id = Guid")
                .And
                .NotContain($"Name = {person.Name}");

        }
        
        [Test]
        public void ExcludeTypeTest()
        {
            printer
                .Excluding<Guid>();
            var result = printer.PrintToString(person);
            Console.WriteLine(printer.PrintToString(person));
            result
                .Should()
                .ContainAll(
                    $"Age = {person.Age}",
                    $"Height = {person.Height}",
                    $"Name = {person.Name}")
                .And
                .NotContain("Id = Guid");

        }
        
        [Test]
        public void AlternativeSerializationOfTypeTest()
        {
            printer
                .Printing<int>().Using(i => i.ToString("X"));
            var result = printer.PrintToString(person);
            Console.WriteLine(printer.PrintToString(person));
            result
                .Should()
                .ContainAll(
                    $"Age = {person.Age.ToString("X")}",
                    $"Height = {person.Height}",
                    $"Name = {person.Name}",
                    "Id = Guid");
        }
        
        [TestCase(0)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(10)]
        [TestCase(11)]
        public void StringTrimmingTest(int lenght)
        {
            printer
                .Printing(p => p.Name).TrimmedToLength(lenght);
            var result = printer.PrintToString(person);
            Console.WriteLine(printer.PrintToString(person));
            result
                .Should()
                .ContainAll(
                    $"Age = {person.Age}",
                    $"Height = {person.Height}",
                    $"Name = {person.Name.Substring(0, lenght > person.Name.Length ? person.Name.Length : lenght)}",
                    "Id = Guid");
        }
        [Test]
        public void CultureTest()
        {
            printer
                .Printing<double>().Using(CultureInfo.InvariantCulture);
            var result = printer.PrintToString(person);
            Console.WriteLine(printer.PrintToString(person));
            result
                .Should()
                .ContainAll(
                    $"Age = {person.Age}",
                    $"Height = {person.Height.ToString("G", CultureInfo.InvariantCulture)}",
                    $"Name = {person.Name}",
                    "Id = Guid");
        }
        
        [Test]
        public void AlternativeSerializationOfPropertyTest()
        {
            printer
                .Printing(p => p.Name).Using(name => name.Substring(0,3));
            var result = printer.PrintToString(person);
            Console.WriteLine(printer.PrintToString(person));
            result
                .Should()
                .ContainAll(
                    $"Age = {person.Age}",
                    $"Height = {person.Height}",
                    $"Name = {person.Name.Substring(0,3)}",
                    "Id = Guid");
            
        }

        [Test]
        public void DictionarySerializationTest()
        {
            var pFamily = new PersonWithFamily(person);
            var father = new Person()
            {
                Name = "Bob", 
                Age = 37, 
                Height = 195.1, 
                Id = new Guid()
            };
            var mother = new Person()
            {
                Name = "Bethany", 
                Age = 35, 
                Height = 183.7, 
                Id = new Guid()
            };
            pFamily.AddFamilyMember("father", father);
            pFamily.AddFamilyMember("mother", mother);
            var result = pFamily.PrintToString();
            Console.WriteLine(result);
            result
                .Should()
                .ContainAll(
                    "Id = Guid",
                    $"Age = {father.Age}",
                    $"Height = {father.Height}",
                    $"Name = {father.Name}",
                    $"Age = {mother.Age}",
                    $"Height = {mother.Height}",
                    $"Name = {mother.Name}");
        }
        
        [Test]
        public void RecursionTest()
        {
            var node1 = new Node("a");
            var node2 = new Node("b");
            node1.nextNode = node2;
            node2.nextNode = node1;
            Console.WriteLine(node1.PrintToString());
        }
        
        [Test]
        public void EnumerationSerializationTest()
        {
            var question = new Question();
            question.QuestionText = "text";
            question.PossibleAnswers = new List<string>(new[] { "a", "b" });
            var result = question.PrintToString();
            result
                .Should()
                .ContainAll("text", "a", "b");
            Console.WriteLine(result);
        }
    }
}