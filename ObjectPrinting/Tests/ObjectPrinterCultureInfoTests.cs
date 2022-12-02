using System.Globalization;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class ObjectPrinterCultureInfoTests
    {
        [Test]
        public void SetCulture_Should_ChangeFormat()
        {
            var p = new Person {Name = "Alex", Height = 35.5, Weight = 83.3};
            var cultureInfo = new CultureInfo("ru");
            var printer = ObjectPrinter.For<Person>()
                .SetCulture(cultureInfo);
            Approvals.Verify(printer.PrintToString(p));
        }

        [Test]
        public void SetCulture_Should_OverridePreviousSetCulture()
        {
            var p = new Person {Name = "Alex", Height = 35.5, Weight = 83.3};
            var cultureInfo1 = new CultureInfo("ru");
            var cultureInfo2 = new CultureInfo("en");
            var printer = ObjectPrinter.For<Person>()
                .SetCulture(cultureInfo1)
                .SetCulture(cultureInfo2);
            Approvals.Verify(printer.PrintToString(p));
        }

        [Test]
        public void SetCulture_ShouldNot_OverridePreviousMethod()
        {
            var p = new Person {Name = "Alex", Height = 35.5, Weight = 83.3};
            var cultureInfo = new CultureInfo("ru");
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p => p.Height)
                .UseSerializeMethod(f => "123haha")
                .SetCulture(cultureInfo);
            Approvals.Verify(printer.PrintToString(p));
        }

        [Test]
        public void UseSerializeMethod_Should_OverridePreviousSetCulture()
        {
            var p = new Person {Name = "Alex", Height = 35.5, Weight = 83.3};
            var cultureInfo = new CultureInfo("ru");
            var printer = ObjectPrinter.For<Person>()
                .SetCulture(cultureInfo)
                .ConfigForProperty(p => p.Height)
                .UseSerializeMethod(f => "123haha");
            Approvals.Verify(printer.PrintToString(p));
        }
    }
}