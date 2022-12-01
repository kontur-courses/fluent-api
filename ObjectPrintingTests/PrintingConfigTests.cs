using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class PrintingConfigTests
    {
        private readonly Person person = new Person { Name = "Alex", Age = 19 };
        private PrintingConfig<Person> printingConfig;

        [SetUp]
        public void CreateObjectPrinter()
        {
            printingConfig = ObjectPrinter.For<Person>();
        }

        [Test]
        public void CreateNullObject()
        {
            Action act = () => new PrintingConfig<Person>();

            act.Should().NotThrow("valid constructor");
        }

        [Test]
        public void PrintPrivateFields()
        {
            printingConfig.PrintToString(person).Should().
                NotContain("secretNumber").
                And.NotContain("SecretNumber");
        }

        [Test]
        public void Excluding_PropertyByName_Success()
        {
            CheckThatNotContainIn(printingConfig.Excluding(p => p.Name), nameof(Person.Name));
        }
        
        [Test]
        public void Excluding_PropertyByType_Success()
        {
            CheckThatNotContainIn(printingConfig.Excluding<string>(), nameof(Person.Name));
            CheckThatNotContainIn(printingConfig.Excluding<Guid>(), nameof(Person.Id));
            CheckThatNotContainIn(printingConfig.Excluding<decimal>(), nameof(Person.Height));
            CheckThatNotContainIn(printingConfig.Excluding<int>(), nameof(Person.Age));
        }

        [Test]
        public void Excluding_Inherited_Success()
        {
            CheckThatNotContainIn(printingConfig.Excluding<string>(), nameof(Person.Name));
            var printedObject = printingConfig.PrintToString(person);
            printedObject.Should().Contain(nameof(Person.Name));
        }

        private void CheckThatNotContainIn(PrintingConfig<Person> printer, string propertyNames)
        {
            var printedObject = printer.PrintToString(person);
            printedObject.Should().NotContain(propertyNames);
        }

        [Test]
        public void PrintToString_LoopedReferencedMembers_PrintedOnce()
        {
            person.BestFriend = new Person() { Name = "Igor", BestFriend = person };

            var printedObject = printingConfig.PrintToString(person);

            printedObject.Should().Contain(person.Name, Exactly.Once()).And.Contain("recursive reference", Exactly.Once());
        }

        [Test]
        public void PrintToString_ListOfPerson_Success()
        {
            var persons = new List<Person>
            {
                person,
                new Person() { Name = "Igor", Age = 17 }
            };
            var printedObject = ObjectPrinter.For<List<Person>>().PrintToString(persons);
            printedObject.Should().ContainAll(persons[0].Name, persons[1].Name);
        }

        [Test]
        public void PrintToString_ArrayOfPerson_Success()
        {
            var persons = new [] {person, new Person() { Name = "Igor", Age = 17} };
            var printedObject = ObjectPrinter.For<Person[]>().PrintToString(persons);
            printedObject.Should().ContainAll(persons[0].Name, persons[1].Name);
        }

        [Test]
        public void PrintToString_DictionaryOfPerson_Success()
        {
            var persons = new Dictionary<int, Person>
            {
                { 1, person },
                { 2, new Person() { Name = "Igor", Age = 17 } }
            };
            var printedObject = ObjectPrinter.For<Dictionary<int, Person>>().PrintToString(persons);
            printedObject.Should().ContainAll(persons[1].Name, persons[2].Name);
        }

        /*
        //TODO: удалить или доделать
        [TestCase(typeof(string), nameof(Person.Name))]
        [TestCase(typeof(Guid), nameof(Person.Id))]
        [TestCase(typeof(double), nameof(Person.Height))]
        [TestCase(typeof(int), nameof(Person.Age))]
        public void Excluding_PropertyByType_Success(Type propertyType, string propertyNames)
        {
            var methodName = nameof(PrintingConfig<Person>.Excluding);
            var method = propertyType.GetMethod(nameof(PrintingConfig<Person>.Excluding));
            var method2 = typeof(PrintingConfig<Person>).getg(methodName, new[] { propertyType });
            var printedObject = (method.Invoke(printingConfig, null) as PrintingConfig<Person>).PrintToString();
            //propertyType.GetMethod(nameof(PrintingConfig<Person>.Excluding)).Invoke(printingConfig, null);
            //var printedObject = printingConfig.Excluding<propertyType>().PrintToString(person);

            printedObject.Should().NotContainAll(propertyNames.Split());
        }*/
    }
}
