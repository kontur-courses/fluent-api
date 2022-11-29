using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private PrintingConfig<Person> printingConfig;
        private readonly Person person = new Person { Child = new Person()
        {
            Name = "Maks",
            Surname = "Davletbaev",
            Height = 7.2,
            Width = 1.8f,
            Age = 0
        }, Car = new Car()
            {
                Color = 123,
                Company = "Tesla"
            },
            Name = "Alex", Surname = "Tsvetkov", Age = 19, Height = 5.6, Width = 7.2f };

        [SetUp]
        public void SetUp()
        {
            printingConfig = ObjectPrinter.For<Person>();
        }
        
        [Test]
        public void Demo()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(x => x.Age)
                .Printing(x => x.Name).Using(x => x.ToString() + "BBB")
                .Printing<float>().Using(CultureInfo.InvariantCulture)
                .Printing<double>().Using(x => (x + 100).ToString())
                .Printing(x => x.Name).TrimmedToLength(5);

            var s1 = printer.PrintToString(person);
            Console.WriteLine(s1);

            var s2 = person.PrintToString();
            Console.WriteLine(s2);

            var s3 = person.PrintToString(x => x.Excluding(x => x.Age));
            Console.WriteLine(s3);
        }

        [Test]
        public void PrintingConfig_WithExcludeType_ShouldReturnString()
        {
            var personConfig = printingConfig.Excluding<Guid>();

            var result = personConfig.PrintToString(person);
            
            StringAssert.DoesNotContain("Id", result);
        }
        
        [Test]
        public void PrintingConfig_WithExcludeProperty_ShouldReturnString()
        {
            var personConfig = printingConfig.Excluding(x => x.Age);

            var result = personConfig.PrintToString(person);
            
            StringAssert.DoesNotContain("Age", result);;
        }
        
        [Test]
        public void PrintingConfig_WithCustomSerializeType_ShouldReturnString()
        {
            var personConfig = printingConfig.Printing<Guid>().Using(x => "id111");

            var result = personConfig.PrintToString(person);
            
            StringAssert.DoesNotContain("Guid", result);
        }
        
        [Test]
        public void PrintingConfig_WithCustomSerializeProperty_ShouldReturnString()
        {
            var personConfig = printingConfig.Printing(x => x.Name).Using(x => "IDK");

            var result = personConfig.PrintToString(person);
            
            StringAssert.DoesNotContain(person.Name, result);
        }
        
        [Test]
        public void PrintingConfig_WithTrimStringProperty_ShouldReturnString()
        {
            const int length = 2;
            var subName = person.Name[..length];
            var personConfig = printingConfig.Printing(x => x.Name).TrimmedToLength(length);

            var result = personConfig.PrintToString(person);
            
            StringAssert.DoesNotContain(person.Name, result);
            StringAssert.Contains(subName, result);
        }
    }
}