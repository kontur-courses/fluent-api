using System;
using System.Globalization;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using ObjectPrinterTests;

namespace ObjectPrinting.Solved.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 12.1 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => $"new num {i}")
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(new CultureInfo("ru-RU"))
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).Using(x => $"новое имя челика {x}")
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();

            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        // 1
        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_Can_ExcludeTypeInt()
        {
            var person = MockPerson.GetCoolProgramer;

            var printer = ObjectPrinter.For<Person>().Excluding<int>();
            Approvals.Verify(printer.PrintToString(person));
        }

        [UseReporter(typeof(DiffReporter))]
        [Test] // 6
        public void ObjectPrinter_Can_PropertyNameExclude()
        {
            var person = MockPerson.GetCoolProgramer;

            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Name);
            Approvals.Verify(printer.PrintToString(person));
        }

        [UseReporter(typeof(DiffReporter))]
        [Test] //2 
        public void ObjectPrinter_Can_SerializeForTypeInt()
        {
            var person = MockPerson.GetCoolProgramer;

            var printer = ObjectPrinter.For<Person>().Printing<double>()
                .Using(p => $"какой-то double {p}");
            
            Approvals.Verify(printer.PrintToString(person));
        }

        [UseReporter(typeof(DiffReporter))]
        [Test] // 4
        public void ObjectPrinter_Can_SerializeProperty()
        {
            var person = MockPerson.GetCoolProgramer;

            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).Using(name => $"этого челика зовут {name}");

            Approvals.Verify(printer.PrintToString(person));
        }
        
        [UseReporter(typeof(DiffReporter))]
        [Test] // 3
        public void ObjectPrinter_Can_ChangeCulture()
        {
            var person = MockPerson.GetCoolProgramer;

            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(new CultureInfo("ru-RU"));

            Approvals.Verify(printer.PrintToString(person));
        }
        
        [UseReporter(typeof(DiffReporter))]
        [Test] // 5
        public void ObjectPrinter_Can_TrimString()
        {
            var person = MockPerson.GetCoolProgramer;

            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(7);

            Approvals.Verify(printer.PrintToString(person));
        }
    }
}