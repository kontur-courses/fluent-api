using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        [SetUp]
        public void SetUp()
        {
            person = new Person { Name = "Alex", Age = 19, Height = 1.2 };
        }

        private Person person;

        [Test]
        public void PrintToString_ShouldReturnStringWithEveryObjectProperty()
        {
            var printer = ObjectPrinter.For<Person>();

            var personWithEveryProperty = printer.PrintToString(person);

            personWithEveryProperty
                .Should()
                .Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\t" +
                    "Name = Alex\r\n\tHeight = 1,2\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToString_ShouldExcludeProperty_WhenItsTypeExcluded()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<int>();

            var personWithoutIntProperties = printer.PrintToString(person);

            personWithoutIntProperties.Should().Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\t" +
                                                   "Name = Alex\r\n\tHeight = 1,2\r\n");
        }

        [Test]
        public void PrintToString_ShouldExcludeProperty_WhenItsNameExcluded()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(person => person.Name);

            var personWithoutNameProperty = printer.PrintToString(person);

            personWithoutNameProperty.Should().Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\t" +
                                                  "Height = 1,2\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToString_ShouldUseTypeCulture_WhenItsSpecified()
        {
            var culture = new CultureInfo("en-US");
            var printer = ObjectPrinter.For<Person>().Printing<double>().WithCulture<double>(culture);

            var personWithUSCultureForDouble = printer.PrintToString(person);

            personWithUSCultureForDouble
                .Should()
                .Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\t" +
                    "Height = 1.2\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnStringWithEveryListObject_WhenListPassed()
        {
            var persons = new List<Person> { new() { Age = 1, Height = 1.2 }, new() { Age = 5 } };
            var printer = ObjectPrinter.For<List<Person>>();

            var listOfPersonsWithEveryItem = printer.PrintToString(persons);

            listOfPersonsWithEveryItem
                .Should()
                .Be(
                    "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = null\r\n\tHeight = 1,2\r\n\tAge = 1\r\n" +
                    "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = null\r\n\tHeight = 0\r\n\tAge = 5\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnStringWithEveryArrayObject_WhenArrayPassed()
        {
            var persons = new[] { new Person { Age = 1, Height = 1.2 }, new Person { Age = 5 } };
            var printer = ObjectPrinter.For<Person[]>();

            var arrayOfPersonsWithEveryItem = printer.PrintToString(persons);

            arrayOfPersonsWithEveryItem
                .Should()
                .Be(
                    "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = null\r\n\tHeight = 1,2\r\n\tAge = 1\r\n" +
                    "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = null\r\n\tHeight = 0\r\n\tAge = 5\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnStringWithEveryDictionaryObject_WhenDictionaryPassed()
        {
            var printer = ObjectPrinter.For<Dictionary<Person, string>>();
            var dict = new Dictionary<Person, string>
            {
                { person, "22" },
                { new Person(), "12" }
            };

            var fileWithListProperty = printer.PrintToString(dict);

            fileWithListProperty
                .Should()
                .Be("KeyValuePair`2\r\n\tKey = Person\r\n\t\tId = 00000000-0000-0000-0000-000000000000\r\n\t\t" +
                    "Name = Alex\r\n\t\tHeight = 1,2\r\n\t\tAge = 19\r\n\tValue = 22\r\nKeyValuePair`2\r\n\t" +
                    "Key = Person\r\n\t\tId = 00000000-0000-0000-0000-000000000000\r\n\t\t" +
                    "Name = null\r\n\t\tHeight = 0\r\n\t\tAge = 0\r\n\tValue = 12\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnStringWithCustomSerializationForType_WhenItsSpecified()
        {
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(x => (x * 2).ToString());

            var personWithEveryIntPropertyMultByTwo = printer.PrintToString(person);

            personWithEveryIntPropertyMultByTwo
                .Should()
                .Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\t" +
                    "Name = Alex\r\n\tHeight = 1,2\r\n\tAge = 38\r\n");
        }

        [Test]
        public void
            PrintToString_ShouldReturnStringWithCustomSerializationProperty_WhenItsSpecified()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Printing(person => person.Name)
                .Using(name => $"My name is {name}");

            var personWithCustomNamePropertySerializer = printer.PrintToString(person);

            personWithCustomNamePropertySerializer
                .Should()
                .Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = My name is Alex\r\n\t" +
                    "Height = 1,2\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnStringWithCustomLength_WhenItsSpecified()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(person => person.Name)
                .TrimToLength(2);

            var personWithTrimmedNameProperty = printer.PrintToString(person);

            personWithTrimmedNameProperty
                .Should()
                .Be(
                    "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Al\r\n\tHeight = 1,2\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnStringWithoutRecursion_WhenObjectRefersOnItself()
        {
            var student = new Student { Name = "Alex", Age = 10 };
            var anotherStudent = new Student { Teacher = student, Name = "Vovchik", Age = 15 };
            student.Teacher = anotherStudent;
            var printer = ObjectPrinter.For<Student>();

            var studentWithTeacherRefOnItself = printer.PrintToString(student);

            studentWithTeacherRefOnItself
                .Should()
                .Be("Student\r\n\tTeacher = Student\r\n\t\tTeacher = Recursive property\r\n\t\t" +
                    "Friend = null\r\n\t\tId = 00000000-0000-0000-0000-000000000000\r\n\t\tName = Vovchik\r\n\t\t" +
                    "Height = 0\r\n\t\tAge = 15\r\n\tFriend = null\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\t" +
                    "Name = Alex\r\n\tHeight = 0\r\n\tAge = 10\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnStringWithoutRecursion_WhenLinkedListRecured()
        {
            var firstLinkedNode = new LinkedNode<int>();
            var secondLinkedNode = new LinkedNode<int>();
            var thirdLinkedNode = new LinkedNode<int>();
            firstLinkedNode.Next = secondLinkedNode;
            secondLinkedNode.Next = thirdLinkedNode;
            thirdLinkedNode.Next = firstLinkedNode;
            var printer = ObjectPrinter.For<LinkedNode<int>>();

            var printedList = printer.PrintToString(firstLinkedNode);

            printedList.Should().Be("LinkedNode`1\r\n\tValue = 0\r\n\tNext = LinkedNode`1\r\n\t\tValue = 0\r\n\t\t" +
                                    "Next = LinkedNode`1\r\n\t\t\tValue = 0\r\n\t\t\tNext = Recursive property\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnStringWithCustomRecursionSerialization_WhenItsSpecified()
        {
            var student = new Student { Name = "Alex", Age = 10 };
            var anotherStudent = new Student { Teacher = student, Name = "Vovchik", Age = 15 };
            student.Teacher = anotherStudent;
            var printer = ObjectPrinter.For<Student>().WithCyclicLinkMessage(() => "Recursion");

            var studentWithTeacherRefOnItself = printer.PrintToString(student);

            studentWithTeacherRefOnItself
                .Should()
                .Be("Student\r\n\tTeacher = Student\r\n\t\tTeacher = Recursion\r\n\t\t" +
                    "Friend = null\r\n\t\tId = 00000000-0000-0000-0000-000000000000\r\n\t\tName = Vovchik\r\n\t\t" +
                    "Height = 0\r\n\t\tAge = 15\r\n\tFriend = null\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\t" +
                    "Name = Alex\r\n\tHeight = 0\r\n\tAge = 10\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnStringWithIgnoredRecuresdProperty_WhenItsSpecified()
        {
            var student = new Student { Name = "Alex", Age = 10 };
            var anotherStudent = new Student { Teacher = student, Name = "Vovchik", Age = 15 };
            student.Teacher = anotherStudent;
            var printer = ObjectPrinter.For<Student>().WithCyclicLinkIgnored();

            var studentWithTeacherRefOnItself = printer.PrintToString(student);

            studentWithTeacherRefOnItself
                .Should()
                .Be("Student\r\n\tTeacher = Student\r\n\t\tTeacher = \r\n\t\t" +
                    "Friend = null\r\n\t\tId = 00000000-0000-0000-0000-000000000000\r\n\t\tName = Vovchik\r\n\t\t" +
                    "Height = 0\r\n\t\tAge = 15\r\n\tFriend = null\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\t" +
                    "Name = Alex\r\n\tHeight = 0\r\n\tAge = 10\r\n");
        }

        [Test]
        public void PrintToString_ShouldThrowException_WhenPropertyRecursed()
        {
            var student = new Student { Name = "Alex", Age = 10 };
            var anotherStudent = new Student { Teacher = student, Name = "Vovchik", Age = 15 };
            student.Teacher = anotherStudent;
            var printer = ObjectPrinter.For<Student>().WithCyclicLinkException();

            Assert.Throws<ArgumentException>(() => printer.PrintToString(student));
        }

        [Test]
        public void PrintToString_ShouldPrintAllObjects_WhenTheyHaveConnectedRefs()
        {
            var firstStudent = new Student { Name = "Alex", Age = 10 };
            var secondStudent = new Student { Name = "Miha", Age = 15 };
            var thirdstudent = new Student { Name = "Petr", Age = 12 };
            firstStudent.Teacher = thirdstudent;
            firstStudent.Friend = secondStudent;
            secondStudent.Teacher = thirdstudent;
            var printer = ObjectPrinter.For<Student>();

            var notRecursedStudents = printer.PrintToString(firstStudent);

            notRecursedStudents.Should()
                .Be("Student\r\n\tTeacher = Student\r\n\t\tTeacher = null\r\n\t\t" +
                    "Friend = null\r\n\t\tId = 00000000-0000-0000-0000-000000000000\r\n\t\tName = Petr\r\n\t\t" +
                    "Height = 0\r\n\t\tAge = 12\r\n\tFriend = Student\r\n\t\tTeacher = Student\r\n\t\t\t" +
                    "Teacher = null\r\n\t\t\tFriend = null\r\n\t\t\tId = 00000000-0000-0000-0000-000000000000\r\n\t\t\t" +
                    "Name = Petr\r\n\t\t\tHeight = 0\r\n\t\t\tAge = 12\r\n\t\tFriend = null\r\n\t\t" +
                    "Id = 00000000-0000-0000-0000-000000000000\r\n\t\tName = Miha\r\n\t\tHeight = 0\r\n\t\tAge = 15\r\n\t" +
                    "Id = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 10\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnStringObjectWithSerializedDictionary_WhenObjectHasDictionary()
        {
            var printer = ObjectPrinter.For<File>();
            var file = new File { Name = "file", Attributes = new Dictionary<string, string> { { "a", "b" } } };

            var fileWithDictionaryProperty = printer.PrintToString(file);

            fileWithDictionaryProperty
                .Should()
                .Be("File\r\n\tName = file\r\n\tAttributes = \r\n\t\tKeyValuePair`2\r\n\t\tKey = a\r\n\t\t" +
                    "Value = b\r\n\tSimilarNames = null\r\n\tCopies = null\r\n\tfield = null\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnStringObjectWithSerializedList_WhenObjectHasList()
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
                .Be("File\r\n\tName = file\r\n\tAttributes = null\r\n\tSimilarNames = \r\n\t\toleg.jpg\r\n\t\t" +
                    "oleg.png\r\n\tCopies = null\r\n\tfield = null\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnStringObjectWithSerializedArray_WhenObjectHasArray()
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
                .Be("File\r\n\tName = file\r\n\tAttributes = null\r\n\tSimilarNames = null\r\n\tCopies = \r\n\t\t" +
                    "oleg.jpg\r\n\t\toleg.png\r\n\tfield = null\r\n");
        }

        [Test]
        public void PrintToString_ShouldPrintListWithNullValue_WhenObjectHasEmptyList()
        {
            var printer = ObjectPrinter.For<File>();
            var file = new File
            {
                Name = "file"
            };

            var fileWithListProperty = printer.PrintToString(file);

            fileWithListProperty
                .Should()
                .Be("File\r\n\tName = file\r\n\tAttributes = null\r\n\tSimilarNames = null\r\n\t" +
                    "Copies = null\r\n\tfield = null\r\n");
        }

        [Test]
        public void PrintToString_ShouldPrintArrayWithNullValue_WhenObjectHasEmptyArray()
        {
            var printer = ObjectPrinter.For<File>();
            var file = new File
            {
                Name = "file"
            };

            var fileWithListProperty = printer.PrintToString(file);

            fileWithListProperty
                .Should()
                .Be("File\r\n\tName = file\r\n\tAttributes = null\r\n\tSimilarNames = null\r\n\t" +
                    "Copies = null\r\n\tfield = null\r\n");
        }

        [Test]
        public void PrintToString_ShouldPrintDictionaryWithNullValue_WhenObjectHasEmptyDictionary()
        {
            var printer = ObjectPrinter.For<File>();
            var file = new File
            {
                Name = "file"
            };

            var fileWithListProperty = printer.PrintToString(file);

            fileWithListProperty
                .Should()
                .Be("File\r\n\tName = file\r\n\tAttributes = null\r\n\tSimilarNames = null\r\n\t" +
                    "Copies = null\r\n\tfield = null\r\n");
        }

        [Test]
        public void TrimToLength_ShouldThrowArgumentOutOfRangeException_WhenPassedNegativeLength()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(person => person.Name);


            Assert.Throws<ArgumentOutOfRangeException>(() => printer.TrimToLength(-1));
        }

        [Test]
        public void PrintToString_ShouldThrowArgumentOutOfRangeException_WhenTrimToLengthBiggerThanPropertyLength()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(person => person.Name).TrimToLength(20);


            Assert.Throws<ArgumentOutOfRangeException>(() => printer.PrintToString(person));
        }
    }
}