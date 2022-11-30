using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using ObjectPrinting.Tests.DTOs;

namespace ObjectPrinting.Tests
{
    public class ObjectExtensionTests
    {
        [Test]
        public void ObjectExtension_PrintToString_ShouldPrintObject()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var result = person.PrintToString();

            result.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n");
        }

        [Test]
        public void ObjectExtension_PrintToStringWi_ShouldPrintObject()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var result = person.PrintingWithConfigure(configs => new PrintingConfig<Person>()
                .ConfigureProperty(x => x.Name)
                .SetSerialization(x => x.ToUpper()));

            result.Should().Be("Person\r\n\tId = Guid\r\n\tName = ALEX\r\n\tHeight = 0\r\n\tAge = 19\r\n");
        }

        [Test]
        public void ObjectExtension_PrintListToString_ShouldPrintList()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var list = new List<Person>
            {
                person,
                person,
                person
            };

            var result = list.PrintListToString();

            result.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n"
                               + "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n"
                               + "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n");
        }

        [Test]
        public void ObjectExtension_PrintingListWithConfigure_ShouldPrintList()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 170.5 };
            var list = new List<Person>
            {
                person,
                person,
                person
            };

            var result = list.PrintingListWithConfigure(config => new PrintingConfig<Person>()
                .SetCulture<double>(CultureInfo.InvariantCulture));

            result.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 170.5\r\n\tAge = 19\r\n"
                               + "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 170.5\r\n\tAge = 19\r\n"
                               + "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 170.5\r\n\tAge = 19\r\n");
        }
    }
}