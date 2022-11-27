using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
        public void Excluding_PropertyByName_Success()
        {
            CheckThatNotContainIn(printingConfig.Excluding(p => p.Name), nameof(Person.Name));
        }
        
        [Test]
        public void Excluding_PropertyByType_Success()
        {
            CheckThatNotContainIn(printingConfig.Excluding<string>(), nameof(Person.Name));
            CheckThatNotContainIn(printingConfig.Excluding<Guid>(), nameof(Person.Id));
            CheckThatNotContainIn(printingConfig.Excluding<double>(), nameof(Person.Height));
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
