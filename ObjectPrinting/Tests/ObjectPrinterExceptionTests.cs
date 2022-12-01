using System;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterExceptionTests
    {
        [Test]
        public void ExcludeProperty_ShouldThrowException_WhenExcludeObjectItself()
        {
            var person = new Person {Name = "Alex", Age = 19};
            
            var ex = Assert.Throws<MissingMemberException>(() =>
            {
                var printer = ObjectPrinter.For<Person>()
                    .ExcludeProperty(p => p);
                printer.PrintToString(person);
            });
        }
        
        [Test]
        public void ExcludeProperty_ShouldThrowException_WhenExcludeNotMember()
        {
            var person = new Person {Name = "Alex", Age = 19};
            
            var ex = Assert.Throws<MissingMemberException>(() =>
            {
                var printer = ObjectPrinter.For<Person>()
                    .ExcludeProperty(p => "123");
                printer.PrintToString(person);
            });
        }
        
        [Test]
        public void OverrideSerializeMethod_ShouldThrowException_WhenExcludeNotMember()
        {
            var person = new Person {Name = "Alex", Age = 19};
            
            var ex = Assert.Throws<MissingMemberException>(() =>
            {
                var printer = ObjectPrinter.For<Person>()
                    .ConfigForProperty(p => "123")
                    .UseSerializeMethod(p => "1");
                printer.PrintToString(person);
            });
        }
        
        [Test]
        public void OverrideSerializeMethod_ShouldThrowException_WhenExcludeObjectItself()
        {
            var person = new Person {Name = "Alex", Age = 19};
            
            var ex = Assert.Throws<MissingMemberException>(() =>
            {
                var printer = ObjectPrinter.For<Person>()
                    .ConfigForProperty(p => p)
                    .UseSerializeMethod(p => "0");
                printer.PrintToString(person);
            });
        }
    }
}