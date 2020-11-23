using System;
using System.Globalization;
using System.Runtime.Serialization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [SetUp]
        public void SetUp()
        {
            person = new Person();
            printer = ObjectPrinter.For<Person>();
        }

        private Person person;
        private PrintingConfig<Person> printer;

        [Test]
        public void PrintToString_PrintingAll_IfHasNoConfig()
        {
            person.Name = "Alex";
            person.Age = 19;
            person.Height = 180;
            person.Surname = "Brown";
            printer.PrintToString(person).Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 180\r\n\t" +
                                                      "Age = 19\r\n\tFriend = null\r\n\tSurname = Brown\r\n");
        }

        [Test]
        public void PrintToString_NotPrintingType_IfTypeExcluded()
        {
            printer = printer.Excluding<int>();
            printer.PrintToString(person).Should().NotContain("Age");
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
            person.Height = 0.5;
            printer = printer.SetCultureInfo<double>(CultureInfo.InvariantCulture);
            printer.PrintToString(person).Should().Contain($"{nameof(person.Height)} = 0.5");
        }

        [Test]
        public void PrintToString_PrintingPropertyByAlternateSerializing()
        {
            person.Name = "Alex";
            printer = printer.SetAlternateSerialize(x => x.Name, x => x + "!!");
            printer.PrintToString(person).Should().Contain($"{nameof(person.Name)} = Alex!!");
        }

        [Test]
        public void PrintToString_PrintingFieldByAlternateSerializing()
        {
            person.Surname = "Brown";
            printer = printer.SetAlternateSerialize(x => x.Surname, x => x + "!!");
            printer.PrintToString(person).Should().Contain($"{nameof(person.Surname)} = Brown!!");
        }

        [Test]
        public void PrintToString_PrintingTypeByAlternateSerializing()
        {
            person.Age = 18;
            printer = printer.SetAlternateSerialize<int>(x => x + "!!!");
            printer.PrintToString(person).Should().Contain($"{nameof(person.Age)} = 18!!!");
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