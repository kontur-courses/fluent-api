using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrintingTests.Auxiliary;

namespace ObjectPrintingTests
{
    class ObjectPrinterTests
    {
        private static readonly String NewLine = Environment.NewLine;
        
        [Test]
        public void Excluding_ShouldExcludeType()
        {
            var doubleWrapper = new Wrapper<double>(10);
            var expected = $"Wrapper<Double>{NewLine}";

            var actual = ObjectPrinter.For<Wrapper<double>>()
                .Excluding<double>()
                .PrintToString(doubleWrapper);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Excluding_ShouldExcludeProperty()
        {
            var doubleWrapper = new Wrapper<int>(10);
            var expected = $"Wrapper<Int32>{NewLine}";

            var actual = ObjectPrinter.For<Wrapper<int>>()
                .Excluding(wrapper => wrapper.Value)
                .PrintToString(doubleWrapper);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UsingCultureInfo_ShouldPrintFloatWithPoint_WhenENCulture()
        {
            var doubleWrapper = new Wrapper<float>(10.1f);
            var expected = $"Wrapper<Single>{NewLine}" + 
                           $"\tValue = 10.1{NewLine}";

            var actual = ObjectPrinter.For<Wrapper<float>>()
                .Printing<float>().WithCultureInfo(new CultureInfo("en-US"))
                .PrintToString(doubleWrapper);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UsingCultureInfo_ShouldPrintFloatWithComma_WhenRUCulture()
        {
            var doubleWrapper = new Wrapper<float>(10.1f);
            var expected = $"Wrapper<Single>{NewLine}" +
                           $"\tValue = 10,1{NewLine}";

            var actual = ObjectPrinter.For<Wrapper<float>>()
                .Printing<float>().WithCultureInfo(new CultureInfo("ru-RU"))
                .PrintToString(doubleWrapper);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TrimmedToLength_ShouldShorten()
        {
            var trimLength = 2;
            var stringWrapper = new Wrapper<string>("hello");
            var expected = $"Wrapper<String>{NewLine}" +
                           $"\tValue = he{NewLine}";

            var actual = ObjectPrinter.For<Wrapper<string>>()
                .Printing<string>().TrimmedToLength(trimLength)
                .PrintToString(stringWrapper);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TrimmedToLength_ShouldLeaveTheSame()
        {
            var trimLength = 10;
            var stringWrapper = new Wrapper<string>("hello");
            var expected = $"Wrapper<String>{NewLine}" +
                           $"\tValue = hello{NewLine}";

            var actual = ObjectPrinter.For<Wrapper<string>>()
                .Printing<string>().TrimmedToLength(trimLength)
                .PrintToString(stringWrapper);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Using_ShouldSetCustomPropertyPrinter()
        {
            var stringWrapper = new Wrapper<string>("hello");
            var expected = $"Wrapper<String>{NewLine}" +
                           $"\tValue = custom{NewLine}";

            var actual = ObjectPrinter.For<Wrapper<string>>()
                .Printing(wrapper => wrapper.Value).Using(prop => "custom")
                .PrintToString(stringWrapper);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Using_ShouldSetCustomTypePrinter()
        {
            var stringWrapper = new Wrapper<string>("hello");
            var expected = $"Wrapper<String>{NewLine}" +
                           $"\tValue = custom{NewLine}";

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

            var expected = $"Person{NewLine}" +
                           $"\tName = Def{NewLine}" +
                           $"\tHeight = 1,3{NewLine}" +
                           $"\tAge = age{NewLine}";

            var actual = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<double>().WithCultureInfo(new CultureInfo("ru-RU"))
                .Printing(p => p.Age).Using(a => "age")
                .Printing(p => p.Name).TrimmedToLength(3)
                .PrintToString(person);
            
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestResolvingCircularReferences()
        {
            var cycle = new Cycle();
            var expected = $"Cycle (label 0){NewLine}" +
                           $"\tValue = Cycle (cycle reference to label 0){NewLine}";

            var actual = ObjectPrinter.For<Cycle>().PrintToString(cycle);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestExcludesTypeInAllInnerObjects()
        {
            var student = new Student
            {
                Name = "First",
                Grade = 4,
                Friend = new Student
                {
                    Name = "Second",
                    Grade = 4
                }
            };

            var printer = ObjectPrinter.For<Student>()
                .Excluding<double>();

            var expected = $"Student{NewLine}" +
                           $"\tName = First{NewLine}" +
                           $"\tFriend = Student{NewLine}" +
                           $"\t\tName = Second{NewLine}" +
                           $"\t\tFriend = null{NewLine}";

            var actual = printer.PrintToString(student);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestExcludesPropertyInAllInnerObjects()
        {
            var student = new Student
            {
                Name = "First",
                Grade = 4,
                Friend = new Student
                {
                    Name = "Second",
                    Grade = 4
                }
            };

            var printer = ObjectPrinter.For<Student>()
                .Excluding(s => s.Grade);

            var expected = $"Student{NewLine}" +
                           $"\tName = First{NewLine}" +
                           $"\tFriend = Student{NewLine}" +
                           $"\t\tName = Second{NewLine}" +
                           $"\t\tFriend = null{NewLine}";

            var actual = printer.PrintToString(student);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestGivesMorePriorityToPropertyPrinter()
        {
            var person = new Person
                { Age = 10, Height = 1.3, Name = "Default" };

            var expected = $"Person{NewLine}" +
                           $"\tName = Default{NewLine}" +
                           $"\tHeight = by property{NewLine}" +
                           $"\tAge = 10{NewLine}";

            var actual = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<double>().Using(d => "by type")
                .Printing(p => p.Height).Using(d => "by property")
                .PrintToString(person);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestUsesLastSetPrinting()
        {
            var person = new Person
                { Age = 10, Height = 1.3, Name = "Default" };

            var expected = $"Person{NewLine}" +
                           $"\tName = Default{NewLine}" +
                           $"\tHeight = last{NewLine}" +
                           $"\tAge = 10{NewLine}";

            var actual = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing(p => p.Height).Using(d => "first")
                .Printing(p => p.Height).Using(d => "last")
                .PrintToString(person);

            Assert.AreEqual(expected, actual);
        }
    }
}
