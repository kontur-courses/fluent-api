using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Solved.Tests
{
    [TestFixture]
    public class ConfigurationTests
    {
        [Test]
        public void Excluding_Type()
        {
            var p = new Person { Name = "Alex" };
            
            var s = ObjectPrinter
                .For<Person>()
                .Excluding<System.Guid>()
                .PrintToString(p);

            s.Should().NotContain(p.Id.ToString());
        }

        [Test]
        public void Excluding_Member()
        {
            var p = new Person { Name = "Alex", Age = 19 };

            var s = ObjectPrinter
                .For<Person>()
                .Excluding(x => x.Age)
                .PrintToString(p);

            s.Should().NotContain("Age").And.Contain("Alex");
        }

        [Test]
        public void Type_Serializer()
        {
            var p = new Person { Age = 19 };

            var s = ObjectPrinter
                .For<Person>()
                .Printing<int>()
                    .Using(i => i.ToString("X"))
                .PrintToString(p);

            s.Should().Contain("13");
        }

        [Test]
        public void Type_Culture()
        {
            var p = new Person { Height = 1.5 };

            var s = ObjectPrinter
                .For<Person>()
                .Printing<double>()
                    .Using(CultureInfo.InvariantCulture)
                .PrintToString(p);

            s.Should().Contain("1.5");
        }

        [Test]
        public void Property_Serializer()
        {
            var p = new Person { Name = "Alex" };

            var s = ObjectPrinter
                .For<Person>()
                .Printing(x => x.Name)
                    .Using(n => n.ToUpper())
                .PrintToString(p);

            s.Should().Contain("ALEX");
        }

        [Test]
        public void TrimmedToLength()
        {
            var p = new Person { Name = "VeryLongName123" };

            var s = ObjectPrinter
                .For<Person>()
                .Printing(x => x.Name)
                    .TrimmedToLength(4)
                .PrintToString(p);

            s.Should().Contain("Very").And.NotContain("VeryLongName123");
        }
    }
}