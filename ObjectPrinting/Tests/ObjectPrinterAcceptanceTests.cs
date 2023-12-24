using System.Collections.Generic;
using System.Globalization;
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
            _person = new Person { Name = "Alex", Age = 19, Height = 1.2 };
        }

        private Person _person;

        [Test]
        public void PrintToString_ShouldReturnStringWithEveryObjectProperty()
        {
            var printer = ObjectPrinter.For<Person>();

            var personWithEveryProperty = printer.PrintToString(_person);

            personWithEveryProperty
                .Should()
                .Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 1,2\r\n\tAge = 19\r\n");
        }

        public void PrintToStringExcludingIntType_ShouldReturnStringWithNoIntProperty()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<int>();

            var personWithNoIntProperties = printer.PrintToString(_person);

            personWithNoIntProperties.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 1,2\r\n");
        }

        public void PrintToStringExcludingPropertyName_ShouldReturnStringWithNoNameProperty()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(person => person.Name);

            var personWithNoNameProperty = printer.PrintToString(_person);

            personWithNoNameProperty.Should().Be("Person\r\n\tId = Guid\r\n\tHeight = 1,2\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToStringUsingDoubleCultureInfo_ShouldReturnStringWithDoubleTypePropertyOfSetCulture()
        {
            var culture = new CultureInfo("en-US");
            var printer = ObjectPrinter.For<Person>().Printing<double>().Using<double>(culture);

            var personWithUSCultureForDouble = printer.PrintToString(_person);

            personWithUSCultureForDouble
                .Should()
                .Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 1.2\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToStringList_ShouldReturnStringWithEveryListObject()
        {
            var persons = new List<Person> { new Person { Age = 1, Height = 1.2 }, new Person { Age = 5 } };
            var printer = ObjectPrinter.For<Person>();

            var listOfPersongWithEveryItem = printer.PrintToString(persons);

            listOfPersongWithEveryItem
                .Should()
                .Be("Person\r\n\tId = Guid\r\n\tName = null\r\n\tHeight = 1,2\r\n\tAge = 1\r\n" +
                    "Person\r\n\tId = Guid\r\n\tName = null\r\n\tHeight = 0\r\n\tAge = 5\r\n");
        }

        [Test]
        public void PrintToStringArray_ShouldReturnStringWithEveryArrayObject()
        {
            var persons = new[] { new Person { Age = 1, Height = 1.2 }, new Person { Age = 5 } };
            var printer = ObjectPrinter.For<Person>();

            var arrayOfPersonsWithEveryItem = printer.PrintToString(persons);

            arrayOfPersonsWithEveryItem
                .Should()
                .Be("Person\r\n\tId = Guid\r\n\tName = null\r\n\tHeight = 1,2\r\n\tAge = 1\r\n" +
                    "Person\r\n\tId = Guid\r\n\tName = null\r\n\tHeight = 0\r\n\tAge = 5\r\n");
        }

        [Test]
        public void PrintToStringUsingCustomIntSerialization_ShouldReturnStringWithCustomSerializationForInt()
        {
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(x => (x * 2).ToString());

            var personWithEveryIntPropertyMultByTwo = printer.PrintToString(_person);

            personWithEveryIntPropertyMultByTwo
                .Should()
                .Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 1,2\r\n\tAge = 38\r\n");
        }

        [Test]
        public void
            PrintToStringUsingCustomPropertySerialization_ShouldReturnStringWithCustomSerializationForNameProperty()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Printing(person => person.Name)
                .Using(name => $"My name is {name}");

            var personWithCustomNamePropertySerializer = printer.PrintToString(_person);

            personWithCustomNamePropertySerializer
                .Should()
                .Be("Person\r\n\tId = Guid\r\n\tName = My name is Alex\r\n\tHeight = 1,2\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToStringTrimmingPropertySerialization_ShouldReturnStringWithTrimmedNameProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(person => person.Name)
                .TrimmedToLength(2);

            var personWithTrimmedNameProperty = printer.PrintToString(_person);

            personWithTrimmedNameProperty
                .Should()
                .Be("Person\r\n\tId = Guid\r\n\tName = Al\r\n\tHeight = 1,2\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToStringObjectWithRefOnItself_ShouldReturnStringWithNoRecursedProperties()
        {
            var student = new Student();
            var anotherStudent = new Student { Teacher = student };
            student.Teacher = anotherStudent;
            var printer = ObjectPrinter.For<Student>();

            var studentWithTeacherRefOnItself = printer.PrintToString(student);

            studentWithTeacherRefOnItself
                .Should()
                .Be("Student\r\n\tTeacher = Student\r\n\t\t"
                    + "Teacher = Student\r\n\t\tId = Guid\r\n\t\tName = null\r\n\t\tHeight = 0\r\n\t\tAge = 0"
                    + "\r\n\tId = Guid\r\n\tName = null\r\n\tHeight = 0\r\n\tAge = 0\r\n");
        }

        [Test]
        public void PrintToStringObjectWithDictionary_ShouldReturnStringObjectWithSerializedDictionary()
        {
            var printer = ObjectPrinter.For<File>();
            var file = new File { Name = "file", Attributes = new Dictionary<string, string> { { "a", "b" } } };

            var fileWithDictionaryProperty = printer.PrintToString(file);

            fileWithDictionaryProperty
                .Should()
                .Be(
                    "File\r\n\tName = file\r\n\tAttributes = \r\n\t\ta : b\r\n\tSimilarNames = null\r\n\tCopies = null\r\n");
        }

        [Test]
        public void PrintToStringObjectWithList_ShouldReturnStringObjectWithSerializedList()
        {
            var printer = ObjectPrinter.For<File>();
            var file = new File
            {
                Name = "file",
                SimilarNames = new List<string> { "oleg.jpg", "oleg.png" }
            };

            var fileWithListProperty = printer.PrintToString(file);

            fileWithListProperty
                .Should()
                .Be("File\r\n\tName = file\r\n\tAttributes = null\r\n\t"
                    + "SimilarNames = \r\n\t\toleg.jpg\r\n\t\toleg.png\r\n\tCopies = null\r\n");
        }

        [Test]
        public void PrintToStringObjectWithArray_ShouldReturnStringObjectWithSerializedList()
        {
            var printer = ObjectPrinter.For<File>();
            var file = new File
            {
                Name = "file",
                Copies = new[] { "oleg.jpg", "oleg.png" }
            };

            var fileWithListProperty = printer.PrintToString(file);

            fileWithListProperty
                .Should()
                .Be("File\r\n\tName = file\r\n\tAttributes = null\r\n\tSimilarNames = null\r\n\t"
                    + "Copies = \r\n\t\toleg.jpg\r\n\t\toleg.png\r\n");
        }

        [Test]
        public void PrintToStringDictionaryOfObjects_ShouldReturnStringWithSerializedDictionary()
        {
            var printer = ObjectPrinter.For<Person>();
            var dict = new Dictionary<Person, string>
            {
                { _person, "22" },
                { new Person(), "12" }
            };

            var fileWithListProperty = printer.PrintToString(dict);

            fileWithListProperty
                .Should()
                .Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 1,2\r\n\tAge = 19\r\n : 22\r\n"
                    + "Person\r\n\tId = Guid\r\n\tName = null\r\n\tHeight = 0\r\n\tAge = 0\r\n : 12\r\n");
        }
    }
}