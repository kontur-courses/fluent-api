using System.Globalization;
using ObjectPrinting.Extensions;
using ObjectPrinting.Tests.TestObjects;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private static readonly VerifySettings settings = new();

        [SetUp]
        public void SetUp()
        {
            settings.UseDirectory("TestResults");
        }

        [Test]
        public Task TestObjectPrinter_ExcludingTypes_CustomSerialization_Culture_Trimming()
        {
            var person = new PrintingTestObject {TestString = "Alex", TestInt = 19};
            person.TestObject = person;

            var printer = ObjectPrinter<PrintingTestObject>.Configure()
                .Excluding<Guid>()
                .Printing<int>().Using(i => i.ToString("X"))
                .Printing<PrintingTestObject, double>(CultureInfo.InvariantCulture)
                .Printing(p => p.TestString).TrimmedToLength(10)
                .Excluding(p => p.TestInt);

            var printedString = printer.PrintToString(person);

            return Verify(printedString, settings);
        }

        [Test]
        public Task TestDefaultSerialization_ExtensionMethod()
        {
            var person = new PrintingTestObject {TestString = "Alex", TestInt = 19};

            var printedString = person.PrintToString();

            return Verify(printedString, settings);
        }

        [Test]
        public Task TestConfiguredSerialization_ExtensionMethod()
        {
            var person = new PrintingTestObject {TestString = "Alex", TestInt = 19};

            var printedString = person.PrintToString(s => s.Excluding(p => p.TestString));

            return Verify(printedString, settings);
        }

        [Test]
        public Task PrintNull_Should_NullString()
        {
            var printedString = new ObjectPrinter<PrintingTestObject>().GetConfiguration().PrintToString(null);

            return Verify(printedString, settings);
        }

        [Test]
        public Task ExcludingProperty_Should_IgnoresSpecifiedProperty()
        {
            var testObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};

            var printedString = ObjectPrinter<PrintingTestObject>.Configure()
                .Excluding(o => o.TestInt)
                .PrintToString(testObject);

            return Verify(printedString, settings);
        }

        [Test]
        public Task ExcludingType_Should_IgnoresSpecifiedType()
        {
            var testObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};

            var printedString = ObjectPrinter<PrintingTestObject>.Configure()
                .Excluding<string>()
                .PrintToString(testObject);

            return Verify(printedString, settings);
        }

        [Test]
        public Task UsingCustomSerialization_Should_AppliesCustomFunction()
        {
            var testObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};

            var printedString = ObjectPrinter<PrintingTestObject>.Configure()
                .Printing(x => x.TestInt)
                .Using(x => $"{x} years")
                .PrintToString(testObject);

            return Verify(printedString, settings);
        }

        [Test]
        public Task UsingCustomSerializationForType_Should_AppliesCustomFunctionToAllTypeProperties()
        {
            var testObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};

            var printedString = ObjectPrinter<PrintingTestObject>.Configure()
                .Printing<int>()
                .Using(x => $"{x} is int")
                .PrintToString(testObject);

            return Verify(printedString, settings);
        }

        [Test]
        public Task UsingCommonCulture_Should_UsesSpecifiedCulture()
        {
            const double testDouble = 1.5;
            const float testFloat = 2.5f;
            var culture = CultureInfo.GetCultureInfo("en-US");
            var testObject = new PrintingTestObject {TestDouble = testDouble, TestFloat = testFloat};

            var printedString = ObjectPrinter<PrintingTestObject>.Configure()
                .UsingCommonCulture(culture)
                .PrintToString(testObject);

            return Verify(printedString, settings);
        }

        [Test]
        public Task UsingCultureForType_Should_UsesSpecifiedCultureForSpecifiedType()
        {
            const double testDouble = 1.5;
            const float testFloat = 2.5f;
            var culture = CultureInfo.GetCultureInfo("en-Us");
            var testObject = new PrintingTestObject {TestDouble = testDouble, TestFloat = testFloat};

            var printedString = ObjectPrinter<PrintingTestObject>.Configure()
                .Printing<PrintingTestObject, double>(culture)
                .PrintToString(testObject);

            return Verify(printedString, settings);
        }

        [Test]
        public Task TrimmingStringLength_Should_TrimsToStringLength()
        {
            var testObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};

            var printedString = ObjectPrinter<PrintingTestObject>.Configure()
                .Printing(o => o.TestString)
                .TrimmedToLength(1)
                .PrintToString(testObject);

            return Verify(printedString, settings);
        }

        [Test]
        [TestCase("Alex", 4, TestName = "LenEqualsStringLength")]
        [TestCase("Alex", 5, TestName = "LenMoreStringLength")]
        public Task TrimmingStringLength_Should_NotTrims(string testString, int maxLength)
        {
            var testObject = new PrintingTestObject {TestString = testString, TestInt = 19};

            var printedString = ObjectPrinter<PrintingTestObject>.Configure()
                .Printing(o => o.TestString)
                .TrimmedToLength(maxLength)
                .PrintToString(testObject);

            return Verify(printedString, settings);
        }

        [Test]
        public Task PrintCircularReference_Should_PrintsMessage()
        {
            var testObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};
            testObject.TestObject = testObject;

            var printedString = ObjectPrinter<PrintingTestObject>.Configure()
                .PrintToString(testObject);

            return Verify(printedString, settings);
        }

        [Test]
        public Task PrintNonCircularReference_Should_PrintsNormally()
        {
            var firstTestObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};
            var secondTestObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};
            firstTestObject.TestObject = secondTestObject;

            var printedString = ObjectPrinter<PrintingTestObject>.Configure()
                .PrintToString(firstTestObject);

            return Verify(printedString, settings);
        }

        [Test]
        public Task PrintArray_Should_AllItems()
        {
            var testObject = new PrintingTestObject
            {
                TestString = "Alex",
                TestInt = 19,
                TestArray = ["Item1", "Item2", "Item3"]
            };

            var printedString = ObjectPrinter<PrintingTestObject>.Configure()
                .PrintToString(testObject);

            return Verify(printedString, settings);
        }

        [Test]
        public Task PrintList_Should_AllItems()
        {
            var testList = new List<object> {"ListItem1", "ListItem2", "ListItem3"};
            var testObject = new PrintingTestObject
            {
                TestString = "Alex",
                TestInt = 19,
                TestList = testList
            };

            var printedString = ObjectPrinter<PrintingTestObject>.Configure()
                .PrintToString(testObject);

            return Verify(printedString, settings);
        }

        [Test]
        public Task PrintDictionary_Should_AllItemsWithKeys()
        {
            var testDictionary = new Dictionary<string, object>
            {
                {"Key1", 1},
                {"Key2", 2},
                {"Key3", 3}
            };
            var testObject = new PrintingTestObject
            {
                TestString = "Alex",
                TestInt = 19,
                TestDictionary = testDictionary
            };

            var printedString = ObjectPrinter<PrintingTestObject>.Configure()
                .PrintToString(testObject);

            return Verify(printedString, settings);
        }

        [Test]
        public Task TestPrimitiveTypes()
        {
            var testObject = new PrimitiveTypesTestObject
            {
                BoolValue = true,
                ByteValue = 255,
                ShortValue = 32767,
                IntValue = 2147483647,
                LongValue = 9223372036854775807,
                DecimalValue = 123.45m,
                DoubleValue = 123.45,
                FloatValue = 123.45f,
                CharValue = 'A',
            };

            var printedString = ObjectPrinter<PrimitiveTypesTestObject>.Configure().PrintToString(testObject);

            return Verify(printedString, settings);
        }

        [Test]
        public Task PrintCircularReference_Should_HandleMultipleReferences()
        {
            var testObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};
            var otherObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};
            testObject.TestObject = otherObject;
            otherObject.TestObject = testObject;

            var printedString = ObjectPrinter<PrintingTestObject>.Configure().PrintToString(testObject);

            return Verify(printedString, settings);
        }
    }
}