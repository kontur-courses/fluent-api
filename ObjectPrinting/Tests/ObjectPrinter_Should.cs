using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        private Person person;
        
        [SetUp]
        public void SetUp()
        {
            person = new Person {Name = "Alex", Age = 20, Height = 173};
        }
        
        [Test]
        public void ShouldExcludeType()
        {
            var result = person.PrintToString(conf => conf
                .Exclude<string>()
                .Exclude<Guid>());
            var expected =
                "Person\r\n" +
                "\tHeight = 173\r\n" +
                "\tAge = 20\r\n";
            result.Should().Be(expected);
        }
        
        [Test]
        public void ShouldExcludeProperty()
        {
            var result = person.PrintToString(conf => conf.Exclude(x => x.Age));
            var expected =
                "Person\r\n" +
                "\tId = 00000000-0000-0000-0000-000000000000\r\n" +
                "\tName = Alex\r\n" +
                "\tHeight = 173\r\n";
            result.Should().Be(expected);
        }
        
        [Test]
        public void ShouldTrim()
        {
            var result = person.PrintToString(conf =>
                conf.SetAltSerialize(p => p.Name)
                    .TrimmedToLength(2));
            var expected =
                "Person\r\n" +
                "\tId = 00000000-0000-0000-0000-000000000000\r\n" +
                "\tName = Al\r\n" +
                "\tHeight = 173\r\n" + 
                "\tAge = 20\r\n";
            result.Should().Be(expected);
        }

        [Test]
        public void PrintToStringNormal_ShouldBeEqualExtensionMethod()
        {
            var normalRes = ObjectPrinter.For<Person>()
                .Exclude<int>()
                .SetAltSerialize<double>()
                    .Using(CultureInfo.InvariantCulture)
                .SetAltSerialize(x => x.Name)
                    .Using(x => x.ToUpper())
                .SetAltSerialize(x => x.Name)
                    .TrimmedToLength(2)
                .PrintToString(person);
            
            var extensionResult = person.PrintToString(conf => conf
                .Exclude<int>()
                .SetAltSerialize<double>()
                    .Using(CultureInfo.InvariantCulture)
                .SetAltSerialize(x => x.Name)
                    .Using(x => x.ToUpper())
                .SetAltSerialize(x => x.Name)
                    .TrimmedToLength(2));
            extensionResult.Should().Be(normalRes);
        }
        
        [Test]
        public void ShouldBePrintWithDefaults()
        {
            var res = person.PrintToString();
            var expected =
                "Person\r\n" +
                "\tId = 00000000-0000-0000-0000-000000000000\r\n" +
                "\tName = Alex\r\n" +
                "\tHeight = 173\r\n"+
                "\tAge = 20\r\n";
            res.Should().Be(expected);
        }

        
        [Test]
        public void ShouldBePrintNull_WhenNull()
        {
            person = null;
            var result = person.PrintToString();
            var expected ="null\r\n";
            result.Should().Be(expected);
        }
    }
}