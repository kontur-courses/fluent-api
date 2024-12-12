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


        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_Can_ArraySerialize()
        {
            var house = Room.GetStudioHouse();

            var printer = ObjectPrinter.For<Room>();
            
            Approvals.Verify(printer.PrintToString(house));
        }
        
        
        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_Can_WorkWithCombination()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 12.1};

        
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<double>().Using(new CultureInfo("ru-RU"))
                .Printing(p => p.Name).Using(x => $"новое имя челика {x}")
                .Printing(p => p.Name).TrimmedToLength(2)
                .Excluding(p => p.Age);
        
            var s1 = printer.PrintToString(person);
            Approvals.Verify(s1);
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_Can_WorkWithArray()
        {
            var shop = Shop.GetShop();

            var s = shop.PrintToString(x => x.Printing(s => s.NameShop).Using(x => $"ваще прикольный магаз {x}"));
            
            Approvals.Verify(s);
        }
        
        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_Can_WithPrimitiveTypes()
        {
            var shop = TestTypePropetries.GetTestType();

            var s = shop.PrintToString();
            
            Approvals.Verify(s);
        }


        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_Can_WorkWithDict()
        {
            var cookies = CookiesWithData.GetFakeCookiesWithData();

            var s = cookies.PrintToString();

            Approvals.Verify(s);
        }
    }
}