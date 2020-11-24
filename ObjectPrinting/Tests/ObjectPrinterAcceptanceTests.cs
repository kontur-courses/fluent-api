using System;
using System.Globalization;
using System.Runtime.Serialization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterAcceptanceTests
    {
        [SetUp]
        public void SetUp()
        {
            person = new Person
            {
                Name = "Alex",
                Age = 19,
                Height = 1.85,
                Surname = "Brown"
            };
            printer = ObjectPrinter.For<Person>();
        }

        private Person person;
        private PrintingConfig<Person> printer;

        [Test]
        public void PrintToString_PrintingAll_IfHasNoConfig()
        {
            var heightToString = person.Height.ToString(CultureInfo.CurrentCulture);
            printer.PrintToString(person).Should().Be("Person\r\n" +
                                                      "\tId = Guid\r\n" +
                                                      "\tName = Alex\r\n" +
                                                      $"\tHeight = {heightToString}\r\n" +
                                                      "\tAge = 19\r\n" +
                                                      "\tFriend = null\r\n" +
                                                      "\tSurname = Brown\r\n");
        }

        [Test]
        public void PrintToString_NotPrintingType_IfTypeExcluded()
        {
            printer = printer.Excluding<int>();
            printer.PrintToString(person).Should().NotContain(nameof(person.Age));
        }

        [Test]
        public void PrintToString_NotPrintingProperty_IfPropertyExcluded()
        {
            printer = printer.Excluding(x => x.Height);
            printer.PrintToString(person).Should().NotContain(nameof(person.Height));
        }

        [Test]
        public void PrintToString_NotPrintingField_IfFieldExcluded()
        {
            printer = printer.Excluding(x => x.Surname);
            printer.PrintToString(person).Should().NotContain(nameof(person.Surname));
        }

        [Test]
        public void PrintToString_PrintingWithCulture_IfCultureWasSet()
        {
            printer = printer.SetCultureInfo<double>(CultureInfo.InvariantCulture);
            printer.PrintToString(person).Should().Contain($"{nameof(person.Height)} = 1.85");
        }

        [Test]
        public void PrintToString_PrintingPropertyByAlternateSerializing()
        {
            printer = printer.SetAlternateSerialize(x => x.Name, x => x + "!!");
            printer.PrintToString(person).Should().Contain($"{nameof(person.Name)} = Alex!!");
        }

        [Test]
        public void PrintToString_PrintingFieldByAlternateSerializing()
        {
            printer = printer.SetAlternateSerialize(x => x.Surname, x => x + "!!");
            printer.PrintToString(person).Should().Contain($"{nameof(person.Surname)} = Brown!!");
        }

        [Test]
        public void PrintToString_PrintingTypeByAlternateSerializing()
        {
            printer = printer.SetAlternateSerialize<int>(x => x + "!!!");
            printer.PrintToString(person).Should().Contain($"{nameof(person.Age)} = 19!!!");
        }

        [Test]
        public void PrintToString_PrintingTrimmedProperty()
        {
            person.Name = "Alexander";
            printer = printer.SetTrimming(x => x.Name, 4);
            printer.PrintToString(person).Should().Contain($"{nameof(person.Name)} = Alex\r\n");
        }

        [Test]
        public void PrintToString_PrintingTrimmedField()
        {
            person.Surname = "Brown";
            printer = printer.SetTrimming(x => x.Surname, 1);
            printer.PrintToString(person).Should().Contain($"{nameof(person.Surname)} = B\r\n");
        }

        [Test]
        public void PrintToString_ThrowsSerializationException_IfCircularReference()
        {
            person.Friend = person;
            Action act = () => printer.PrintToString(person);
            act.Should().Throw<SerializationException>();
        }

        [Test]
        public void PrintToString_ThrowsSerializationException_IfCircularReferenceBetween3objects()
        {
            var person2 = new Person();
            var person3 = new Person();
            person.Friend = person2;
            person2.Friend = person3;
            person3.Friend = person;
            Action act = () => printer.PrintToString(person);
            act.Should().Throw<SerializationException>();
        }
    }
}