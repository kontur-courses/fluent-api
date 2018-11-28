using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Tests;

namespace ObjectPrintingTests.Tests
{
    [TestFixture]
    public class PrintingConfig_Should
    {
        private PrintingConfig<Person> printer;
        private Person person;


        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
            person = new Person { Name = "Alex", Age = 19, Height = 1.0001 };
        }

        [Test]
        public void Demo()
        {
            var printer = ObjectPrinter
                .For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<double>().Using(x => (x - 1).ToString())
                //3. Для числовых типов указать культуру
                .Printing<float>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(x => x.Name).Using(x => x.ToLower())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing<string>().TrimmedToLength(3)
                //6. Исключить из сериализации конкретного свойства
                .ExcludingProperty(x => x.Age);
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием

            var s1 = printer.PrintToString(person);
        }

        [Test]
        public void PrintToString_ContainsAllProperties_WhenGetsNoAdditionProperties()
        {
            var printing = printer.PrintToString(person);

            foreach (var property in GetProperties(person))
            {
                printing.Should()
                    .Contain(property.Name);
            }

            GetProperties(person).Should().OnlyContain(x => printing.Contains(x.Name));
        }

        [Test]
        public void PrintToString_DoNotContainExcludedTypes()
        {
            var printer = this.printer.Excluding<int>();

            var printing = printer.PrintToString(person);

            GetPropertiesWithCurrentType<int>(person)
                .Should()
                .NotContain(x => printing.Contains(x.Name));
        }

        [Test]
        public void PrintToString_ContainsNotExcludedTypes_WhenExcludingIsHappened()
        {
            var printing = printer.Excluding<int>()
                .PrintToString(person);

            GetPropertiesWithNotCurrentType<int>(person)
                .Should()
                .OnlyContain(x => printing.Contains(x.Name));
        }

        [Test]
        public void PrintToString_ContainsCorrectNumber_WhenThereIsSpecialCulture()
        {
            var culture = CultureInfo.GetCultureInfo(10);
            var printing = printer
                .Printing<double>()
                .Using(culture)
                .PrintToString(person);
            var numberWithCulture = person.Height.ToString(culture);

            printing.Should().Contain(numberWithCulture);
        }

        [Test]
        public void PrintToString_ContainsPropertyWithSpecialPrinting_WhenGetsModifiedPrinting()
        {
            Func<int, string> intPrinting = x => "99999";

            var printing = printer
                    .Printing(x => x.Age)
                    .Using(intPrinting)
                    .PrintToString(person);
            var printedAge = intPrinting(person.Age);

            printing.Should().Contain(printedAge);
        }

        [Test]
        public void PrintToString_ContainsPropertyWithCommonPrinting_WhenGetsModifiedPrintingWithSameType()
        {
            var person = new Person { Age = 1, Height = 2, Name = "ab", Weight = 14 };
            var printing = printer
                .Printing(x => x.Height)
                .Using(x => "9999")
                .PrintToString(person);

            printing.Should().Contain(person.Weight.ToString());
        }

        [Test]
        public void Printing_DoesNotContainProperty_WhenExcludesCurrentProperty()
        {
            person.Surname = "Surname";

            var printing = printer.ExcludingProperty(x => x.Surname)
                .PrintToString(person);

            printing.Should().NotContain(person.Surname);
        }

        [Test]
        public void ExcludingProperty_ThrowsException_WhenGetsMethodInsteadOfProperty()
        {
            Assert.Throws<ArgumentException>(() => printer.ExcludingProperty(x => x.GetNextPerson()));
        }

        [Test]
        public void ExcludingProperty_ThrowsException_WhenRefersToNotGivenObject()
        {
            Assert.Throws<ArgumentException>(() => printer.ExcludingProperty(x => new Person().Age));
        }

        [Test]
        public void ExcludingProperty_ThrowsException_WhenExpressionRefersToGivenConfig()
        {
            Assert.Throws<ArgumentException>(() => printer.ExcludingProperty(x => x));
        }

        [Test]
        public void PrintToString_DoNotExcludeObject_WhenExcludeOtherObjectWithSameLocalName()
        {
            var printing = printer.ExcludingProperty(x => x.Parent.Age)
                .PrintToString(person);

            var agePrinting = person.Age.ToString();

            printing.Should().Contain(agePrinting);
        }

        [Test]
        public void PrintToString_ExcludeExcludedObject_WhenTheyAreNested()
        {
            var parent = new Person { Age = 40 };
            person.Parent = parent;

            var printing = printer.ExcludingProperty(x => x.Parent.Age)
                .PrintToString(person);
            var agePrinting = person.Age.ToString();

            printing.Should().Contain(agePrinting);
        }

        [Test]
        public void Printing_ReturnsEmptyString_WhenExlucdeTypeOfPrintingItem()
        {
            var printing = printer.Excluding<Person>()
                .PrintToString(person);

            printing.Should().Be(string.Empty);
        }

        [Test]
        public void PrintToString_ExcludeRepeatingItem_WhenThereAreTwoDirectionConnectedItem()
        {
            var parent = new Person { Age = 40 };
            person.Parent = parent;
            parent.Parent = person;

            var printing = printer.PrintToString(person);
            var printingAge = parent.Age.ToString();

            ParseByWhiteSpaces(printing)
                .Where(x => printingAge == x)
                .Should()
                .HaveCount(1);
        }

        [Test]
        public void PrintToString_ExcludeRepeatingItem_WhenThereAreACycle()
        {
            var parent = new Person {Age = 40};
            person.Parent = parent;
            var grandParent = new Person {Age = 60};
            parent.Parent = grandParent;
            grandParent.Relative = person;

            var printing = printer.PrintToString(person);
            Console.WriteLine(printing);
            ParseByWhiteSpaces(printing).Where(x => x == person.Age.ToString())
                .Should()
                .HaveCount(1);
        }

        [Test]
        public void PrintToString_ContainsAddedProperty()
        {
            person.Surname = "Surname";

            var printing = printer.PrintToString(person);

            printing.Should().Contain(person.Surname);
        }

        [Test]
        public void PrintToString_TrimsString_WhenIncludesCurrentOption()
        {
            var printing = printer.Printing<string>()
                .TrimmedToLength(3)
                .PrintToString(person);
            Console.WriteLine(printing);
            printing.Should().NotContain(person.Name)
                .And
                .Contain(person.Name.Substring(0, 3));
        }

        [Test]
        public void PrintToString_RemoveTooDeepItems()
        {
            var previousPerson = person;
            for (var index = 0; index < 100; index++)
            {
                var newPerson = new Person {Age = index};
                previousPerson.Relative = new Person();
                previousPerson = newPerson;
            }

            var printing = printer.PrintToString(person);

            printing.Should().NotContain("99");
        }

        [Test]
        public void PrintToString_PrintingEmptyItems_AsNull()
        {
           printer.PrintToString(person).Should().Contain("null");
        }

        [Test]
        public void PrintToString_IncludesAllItemsInCollection()
        {
            var objects = new object[] {1, "aaaab"};

            var printing = ObjectPrinter.For<object[]>()
                .PrintToString(objects);

            foreach (var obj in objects)
            {
                printing.Should().Contain(obj.ToString());
            }

        }

        [Test]
        public void PrintToString_ExcludeItemsInCollection_WhenTheyHaveExcludedTypesObjects()
        {
            var objects = new object[] {1, "aaaab"};

            var printing = ObjectPrinter.For<object[]>()
                .Excluding<string>()
                .PrintToString(objects);

            printing.Should().NotContain("aaaab");
        }

        [Test]
        public void PrintToString_UsingCanWorkWithTrim()
        {
            person.Name = "PersonName";

            var printing = printer.Printing<string>()
                .TrimmedToLength(3)
                .Printing<string>()
                .Using(x => x + x)
                .PrintToString(person);

            printing.Should().NotContain(person.Name);
        }

        private IEnumerable<PropertyInfo> GetProperties(object obj)
        {
            return obj.GetType().GetProperties();
        }

        private IEnumerable<PropertyInfo> GetPropertiesWithCurrentType<T>(object obj)
            => GetProperties(obj).Where(x => x.PropertyType == typeof(T));

        private IEnumerable<PropertyInfo> GetPropertiesWithNotCurrentType<T>(object obj)
            => GetProperties(obj).Where(x => x.PropertyType != typeof(T));

        private IEnumerable<string> ParseByWhiteSpaces(string line)
        {
            var regex = new Regex(@"\s");
            return regex.Split(line);
        }
    }
}