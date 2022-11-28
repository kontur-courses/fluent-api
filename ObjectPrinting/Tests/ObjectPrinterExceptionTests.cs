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
                    .ConfigForProperty(p => p)
                    .ExcludeFromConfig();
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
                    .ConfigForProperty(p => "123")
                    .ExcludeFromConfig();
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
                    .OverrideSerializeMethod(p=>"1")
                    .SetConfig();
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
                    .OverrideSerializeMethod(p=>"0")
                    .SetConfig();
                printer.PrintToString(person);
            });
        }
    }
}