using ApprovalTests.Reporters;
using ApprovalTests;
using ObjectSerializerTests.ClassesToSerialize;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObjectSerializer;

namespace ObjectSerializerTests.TimePeriodTests
{
    [TestFixture]
    public class TimePeriodTests
    {
        private TimePeriod serializePeriod;

        [OneTimeSetUp]
        public void Init()
        {
            var guid = new Guid("07406af6-61a9-434f-aefe-f99a10cdadfd");
            var startTime = new DateTime(2024, 12, 12);
            var endTime = new DateTime(2024, 12, 14);

            serializePeriod = new TimePeriod(guid, startTime, endTime, 20, 10);
        }


        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_CanExclude_FieldsAndPropertiesOfType()
        {
            var printer = ObjectPrinter.For<TimePeriod>()
                .Exclude<Guid>();

            Approvals.Verify(printer.PrintToString(serializePeriod));
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_CanConfigPrint_FieldsAndPropertiesOfType()
        {
            var printer = ObjectPrinter.For<TimePeriod>()
                .For<Guid>().Using(p => $"Guid:{p.ToString()}");

            Approvals.Verify(printer.PrintToString(serializePeriod));
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_CanApplyCulture_ForTypesHavingCulture()
        {
            var printer = ObjectPrinter.For<TimePeriod>()
                .For<double>()
                .Using(new CultureInfo("ru-RU"));

            Approvals.Verify(printer.PrintToString(serializePeriod));
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_CanConfigPrint_ForFieldOrProperty()
        {
            var printer = ObjectPrinter.For<TimePeriod>()
                .For(p => p.TotalPlaces)
                    .Using(h => $"Всего мест {h}")
                .For(p => p.BookedPlaces)
                    .Using(h => $"Забронировано {h}");

            Approvals.Verify(printer.PrintToString(serializePeriod));
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_CanExclude_FieldOrProperty()
        {
            var printer = ObjectPrinter.For<TimePeriod>()
                .Exclude(p => p.TotalPlaces);

            Approvals.Verify(printer.PrintToString(serializePeriod));
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_CanApply_FewConfigurationMethods()
        {
            var printer = ObjectPrinter.For<TimePeriod>()
                .Exclude<Guid>()
                .For<DateTime>()
                    .Using(i => i.ToString("d"))
                .Exclude(p => p.TotalPlaces);

            Approvals.Verify(printer.PrintToString(serializePeriod));
        }
    }
}
