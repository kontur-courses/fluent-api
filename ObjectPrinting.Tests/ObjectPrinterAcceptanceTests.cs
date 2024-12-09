using System.Globalization;
using FluentAssertions;
using ObjectPrinting.Extensions;
using ObjectPrinting.Tests.TestObjects;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    [TestOf(typeof(ObjectPrinter))]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person {Name = "Alex", Age = 19};

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);

            var s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();

            //8. ...с конфигурированием
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        [Test]
        public void PrintNull_Should_NullString()
        {
            var printedString = ObjectPrinter.For<PrintingTestObject>().PrintToString(null);

            printedString.Should().BeEquivalentTo("null");
        }

        [Test]
        public void ExcludingProperty_Should_IgnoresSpecifiedProperty()
        {
            var testObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};

            var printedString = ObjectPrinter.For<PrintingTestObject>()
                .Excluding(o => o.TestInt)
                .PrintToString(testObject);

            printedString.Should().NotContain(nameof(testObject.TestInt));
        }

        [Test]
        public void ExcludingType_Should_IgnoresSpecifiedType()
        {
            var testObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};

            var printedString = ObjectPrinter.For<PrintingTestObject>()
                .Excluding<string>()
                .PrintToString(testObject);

            printedString.Should().NotContain(nameof(testObject.TestString));
        }

        [Test]
        public void UsingCustomSerialization_Should_AppliesCustomFunction()
        {
            var testObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};

            var printedString = ObjectPrinter.For<PrintingTestObject>()
                .Printing<int>(x => x.TestInt)
                .Using(x => $"{x} years")
                .PrintToString(testObject);

            printedString.Should().Contain($"{testObject.TestInt} years");
        }

        [Test]
        public void UsingCustomSerializationForType_Should_AppliesCustomFunctionToAllTypeProperties()
        {
            var testObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};

            var printedString = ObjectPrinter.For<PrintingTestObject>()
                .Printing<int>()
                .Using(x => $"{x} is int")
                .PrintToString(testObject);

            printedString.Should().Contain($"{testObject.TestInt} is int");
        }

        [Test]
        public void UsingCommonCulture_Should_UsesSpecifiedCulture()
        {
            const double testDouble = 1.5;
            const float testFloat = 2.5f;
            var culture = CultureInfo.GetCultureInfo("en-US");
            var testObject = new PrintingTestObject {TestDouble = testDouble, TestFloat = testFloat};

            var printedString = ObjectPrinter.For<PrintingTestObject>()
                .UsingCommonCulture(culture)
                .PrintToString(testObject);

            printedString.Should().ContainAll(testDouble.ToString(culture), testFloat.ToString(culture));
        }

        [Test]
        public void UsingCultureForType_Should_UsesSpecifiedCultureForSpecifiedType()
        {
            const double testDouble = 1.5;
            const float testFloat = 2.5f;
            var culture = CultureInfo.GetCultureInfo("en-Us");
            var testObject = new PrintingTestObject {TestDouble = testDouble, TestFloat = testFloat};

            var printedString = ObjectPrinter.For<PrintingTestObject>()
                .Printing<double>()
                .Using(culture)
                .PrintToString(testObject);

            printedString.Should().ContainAll(testDouble.ToString(culture), testFloat.ToString());
        }

        [Test]
        public void TrimmingStringLength_Should_TrimsToStringLength()
        {
            var testObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};

            var printedString = ObjectPrinter.For<PrintingTestObject>()
                .Printing(o => o.TestString)
                .TrimmedToLength(1)
                .PrintToString(testObject);

            printedString.Should().NotContain("lex");
        }

        [Test]
        [TestCase("Alex", 4, TestName = "LenEqualsStringLength")]
        [TestCase("Alex", 5, TestName = "LenMoreStringLength")]
        public void TrimmingStringLength_Should_NotTrims(string testString, int maxLength)
        {
            var testObject = new PrintingTestObject {TestString = testString, TestInt = 19};

            var printedString = ObjectPrinter.For<PrintingTestObject>()
                .Printing(o => o.TestString)
                .TrimmedToLength(maxLength)
                .PrintToString(testObject);

            printedString.Should().Contain(testObject.TestString);
        }

        [Test]
        public void PrintCircularReference_Should_PrintsMessage()
        {
            var testObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};
            testObject.TestObject = testObject;

            var printedString = ObjectPrinter.For<PrintingTestObject>()
                .PrintToString(testObject);

            printedString.Should().Contain("circular reference");
        }

        [Test]
        public void PrintNonCircularReference_Should_PrintsNormally()
        {
            var firstTestObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};
            var secondTestObject = new PrintingTestObject {TestString = "Alex", TestInt = 19};
            firstTestObject.TestObject = secondTestObject;

            var printedString = ObjectPrinter.For<PrintingTestObject>()
                .PrintToString(firstTestObject);

            printedString.Should().NotContain("circular reference");
        }

        [Test]
        public void PrintArray_Should_AllItems()
        {
            var testObject = new PrintingTestObject
            {
                TestString = "Alex",
                TestInt = 19,
                TestArray = ["Item1", "Item2", "Item3"]
            };

            var printedString = ObjectPrinter.For<PrintingTestObject>()
                .PrintToString(testObject);

            printedString.Should().ContainAll("Item1", "Item2", "Item3");
        }

        [Test]
        public void PrintList_Should_AllItems()
        {
            var testList = new List<object> {"ListItem1", "ListItem2", "ListItem3"};
            var testObject = new PrintingTestObject
            {
                TestString = "Alex",
                TestInt = 19,
                TestList = testList
            };
            
            var printedString = ObjectPrinter.For<PrintingTestObject>()
                .PrintToString(testObject);

            printedString.Should().ContainAll("ListItem1", "ListItem2", "ListItem3");
        }

        [Test]
        public void PrintDictionary_Should_AllItemsWithKeys()
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

            var printedString = ObjectPrinter.For<PrintingTestObject>()
                .PrintToString(testObject);

            printedString.Should().ContainAll("Key1 = 1", "Key2 = 2", "Key3 = 3");
        }
    }
}