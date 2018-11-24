using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    class ObjectPrinterTests
    {
        private readonly String newLine = Environment.NewLine;

        [Test]
        public void Excluding_ShouldExcludeType()
        {
            var doubleWrapper = new Wrapper<double>(10);
            var expected = $"{typeof(Wrapper<double>).Name}{newLine}";
            var actual = ObjectPrinter.For<Wrapper<double>>()
                .Excluding<double>()
                .PrintToString(doubleWrapper);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Excluding_ShouldExcludeProperty()
        {
            var doubleWrapper = new Wrapper<int>(10);
            var expected = $"{typeof(Wrapper<int>).Name}{newLine}";
            var actual = ObjectPrinter.For<Wrapper<int>>()
                .Excluding(wrapper => wrapper.Value)
                .PrintToString(doubleWrapper);

            Assert.AreEqual(expected, actual);
        }

        [TestCase("ru-RU", "10,1")]
        [TestCase("en-US", "10.1")]
        public void UsingCultureInfo_ShouldSetCulture(string cultureName, string expectedDoubleRepr)
        {
            var doubleWrapper = new Wrapper<float>(10.1f);
            var expected = $"{typeof(Wrapper<float>).Name}{newLine}\tValue = {expectedDoubleRepr}{newLine}";
            var actual = ObjectPrinter.For<Wrapper<float>>()
                .Printing<float>().Using(new CultureInfo(cultureName))
                .PrintToString(doubleWrapper);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(2, "he", TestName = "trim when trimLength is less than property length")]
        [TestCase(10, "hello", TestName = "leave the same when trimLength is greater than property length")]
        public void TrimmedToLength_Should(int trimLength, string expectedTrimmedString)
        {
            var stringWrapper = new Wrapper<string>("hello");
            var expected = $"{typeof(Wrapper<string>).Name}{newLine}\tValue = {expectedTrimmedString}{newLine}";
            var actual = ObjectPrinter.For<Wrapper<string>>()
                .Printing<string>().TrimmedToLength(trimLength)
                .PrintToString(stringWrapper);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Using_ShouldSetCustomPropertyPrinter()
        {
            var stringWrapper = new Wrapper<string>("hello");
            var expected = $"{typeof(Wrapper<string>).Name}{newLine}\tValue = custom{newLine}";
            var actual = ObjectPrinter.For<Wrapper<string>>()
                .Printing(wrapper => wrapper.Value).Using(prop => "custom")
                .PrintToString(stringWrapper);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Using_ShouldSetCustomTypePrinter()
        {
            var stringWrapper = new Wrapper<string>("hello");
            var expected = $"{typeof(Wrapper<string>).Name}{newLine}\tValue = custom{newLine}";
            var actual = ObjectPrinter.For<Wrapper<string>>()
                .Printing<string>().Using(prop => "custom")
                .PrintToString(stringWrapper);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestPrintingWithManyConfigs()
        {
            var person = new Person
                { Age = 10, Height = 1.3, Name = "Default"};
            var expected = $"Person{newLine}\tName = Def{newLine}\tHeight = 1,3{newLine}\tAge = age{newLine}";
            var actual = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<double>().Using(new CultureInfo("ru-RU"))
                .Printing(p => p.Age).Using(a => "age")
                .Printing(p => p.Name).TrimmedToLength(3)
                .PrintToString(person);
            
            Assert.AreEqual(expected, actual);
        }
    }
}
