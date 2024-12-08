using System.Globalization;
using ObjectPrinting.Utilits.Configs;
using ObjectPrinting.Utilits.Objects;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person? _person;

        [SetUp]
        public void SetUp()
        {
            _person = new Person()
            {
                Id = new Guid(),
                FirstName = "Ivan",
                SecondName = "Ivanov",
                Height = 180.2,
                Age = 25,
            };
        }

        [Test]
        public void Demo_TypeExcluding()
        {
            //1. Исключить из сериализации свойства определенного типа.
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>();

            Console.WriteLine(printer.PrintToString(_person!));
        }

        [Test]
        public void Demo_AlternativeSerializationForType()
        {
            //2. Указать альтернативный способ сериализации для определенного типа.
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(x => $"Formating for int {x}");

            Console.WriteLine(printer.PrintToString(_person!));
        }

        [Test]
        public void Demo_CultureInfo()
        {
            //3. Для числовых типов указать культуру.
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(new CultureInfo("en-US"));

            Console.WriteLine(printer.PrintToString(_person!));
        }

        [Test]
        public void Demo_AlternativeSerializationForProperty()
        {
            //4. Настроить сериализацию конкретного свойства.
            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.FirstName).Using(x => $"Formating for FirstName {x}");

            Console.WriteLine(printer.PrintToString(_person!));
        }

        [Test]
        public void Demo_TrimmedEnd()
        {
            //5. Настроить обрезание строковых свойств
            //   (метод должен быть виден только для строковых свойств).
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.SecondName).TrimEnd(4);

            Console.WriteLine(printer.PrintToString(_person!));
        }

        [Test]
        public void Demo_PropertyExcluding()
        {
            //6. Исключить из сериализации конкретного свойства.
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age);
            Console.WriteLine(printer.PrintToString(_person!));
        }

        [Test]
        public void Demo_DefaultExtension()
        {
            //7. Синтаксический сахар в виде метода расширения,
            //   сериализующего по-умолчанию
            Console.WriteLine(_person.PrintToString());
        }

        [Test]
        public void Demo_ExtensionWithConfig()
        {
            //8. Синтаксический сахар в виде метода расширения,
            //   с конфигурированием
            var result = _person.PrintToString(
                printer => printer.Excluding(person => person!.Id));
            Console.WriteLine(result);
        }

        [Test]
        public void Demo_Default()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<int>().Using(x => $"Formating for int {x}")
                .Printing<double>().Using(new CultureInfo("en-US"))
                .Printing(x => x.FirstName).Using(x => $"Formating for FirstName {x}")
                .Printing(x => x.SecondName).TrimEnd(4)
                .Excluding(x => x.Age);

            Console.WriteLine(printer.PrintToString(_person!));
        }
    }
}