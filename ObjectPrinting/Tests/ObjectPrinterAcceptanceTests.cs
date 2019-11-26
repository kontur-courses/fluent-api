using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;
using ObjectPrinting.Tests.TestClasses;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;

        [OneTimeSetUp]
        public void SetUp()
        {
            person = new Person
            {
                Id = Guid.Empty,
                Name = "Vitya",
                Height = 1.55,
                Age = 42
            };
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WithExcludedType()
        {
            var expected = "Person" + Environment.NewLine + 
                           $"\tName = {person.Name}" + Environment.NewLine +
                           $"\tHeight = {person.Height}" + Environment.NewLine + 
                           $"\tAge = {person.Age}" + Environment.NewLine;

            ObjectPrinter
                .SetUp<Person>()
                .Exclude<Guid>()
                .PrintToString(person)
                .Should()
                .Be(expected);
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WithExcludedProperty()
        {
            var expected = "Person" + Environment.NewLine +
                                    $"\tName = {person.Name}" + Environment.NewLine +
                                    $"\tHeight = {person.Height}" + Environment.NewLine +
                                    $"\tAge = {person.Age}" + Environment.NewLine;

            ObjectPrinter
                .SetUp<Person>()
                .Exclude(p => p.Id)
                .PrintToString(person)
                .Should()
                .Be(expected);
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WithCustomSerializationOfType()
        {
            var expected = "Person" + Environment.NewLine +
                                    "\tId = Guid" + Environment.NewLine +
                                    $"\tName = {person.Name}" + Environment.NewLine +
                                    $"\tHeight = {person.Height}" + Environment.NewLine +
                                    $"\tAge = {person.Age:X}" + Environment.NewLine;

            ObjectPrinter
                .SetUp<Person>()
                .For<int>()
                .Use(i => i.ToString("X"))
                .PrintToString(person)
                .Should()
                .Be(expected);
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WithCustomSerializationOfProperty()
        {
            Func<double, string> serializer = height => $"{(int)(height * 100)} cm";

            var expected = "Person" + Environment.NewLine +
                                    "\tId = Guid" + Environment.NewLine +
                                    $"\tName = {person.Name}" + Environment.NewLine +
                                    $"\tHeight = {serializer(person.Height)}" + Environment.NewLine +
                                    $"\tAge = {person.Age}" + Environment.NewLine;

            ObjectPrinter
                .SetUp<Person>()
                .For(person => person.Height)
                .Use(serializer)
                .PrintToString(person)
                .Should()
                .Be(expected);
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WithPropertySerializationOverTypeSerialization()
        {
            Func<double, string> typeSerializer = d => $"(double){d}";
            Func<double, string> propertySerializer = height => $"{(int)(height * 100)} cm";

            var expected = "Person" + Environment.NewLine +
                                    "\tId = Guid" + Environment.NewLine +
                                    $"\tName = {person.Name}" + Environment.NewLine +
                                    $"\tHeight = {propertySerializer(person.Height)}" + Environment.NewLine +
                                    $"\tAge = {person.Age}" + Environment.NewLine;

            ObjectPrinter
                .SetUp<Person>()
                .For<double>()
                .Use(typeSerializer)
                .For(person => person.Height)
                .Use(propertySerializer)
                .PrintToString(person)
                .Should()
                .Be(expected);
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WithCustomCultureInfoOfType()
        {
            var expected = "Person" + Environment.NewLine +
                                    "\tId = Guid" + Environment.NewLine +
                                    $"\tName = {person.Name}" + Environment.NewLine +
                                    $"\tHeight = {person.Height.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine +
                                    $"\tAge = {person.Age}" + Environment.NewLine;

            ObjectPrinter
                .SetUp<Person>()
                .For<double>()
                .Use(CultureInfo.InvariantCulture)
                .PrintToString(person)
                .Should()
                .Be(expected);
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WithTrimmedStringProperty()
        {
            var expected = "Person" + Environment.NewLine +
                                    "\tId = Guid" + Environment.NewLine +
                                    $"\tName = {person.Name.Substring(0, 3)}" + Environment.NewLine +
                                    $"\tHeight = {person.Height}" + Environment.NewLine +
                                    $"\tAge = {person.Age}" + Environment.NewLine;
            ObjectPrinter
                .SetUp<Person>()
                .For(p => p.Name)
                .TrimToLength(3)
                .PrintToString(person)
                .Should()
                .Be(expected);
        }


        [Test]
        public void PrintingToString_ReturnsCorrectString_WithArray()
        {
            var nameSet = new NameSet {Names = new[] {"Beebop", "Valentine", "Jet Black"}};
            var expected = "NameSet" + Environment.NewLine +
                                     "\tNames = " + Environment.NewLine +
                                     "\t\tBeebop" + Environment.NewLine +
                                     "\t\tValentine" + Environment.NewLine +
                                     "\t\tJet Black" + Environment.NewLine;

            ObjectPrinter.SetUp<NameSet>().PrintToString(nameSet).Should().Be(expected);
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WithCustomSerializationOfCollection()
        {
            var nameSet = new NameSet {Names = new[] {"Beebop", "Valentine", "Jet Black"}};
            Func<string[], string> serializer = names => string.Join("/", names);

            var expected = "NameSet" + Environment.NewLine +
                                     "\tNames = Beebop/Valentine/Jet Black" + Environment.NewLine;

            ObjectPrinter
                .SetUp<NameSet>()
                .For<string[]>()
                .Use(serializer)
                .PrintToString(nameSet)
                .Should()
                .Be(expected);
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WithList()
        {
            var results = new RaceResults {Racers = new List<string> {"McLaren", "Ferrari", "Ford"}};
            var expected = "RaceResults" + Environment.NewLine +
                                         "\tRacers = " + Environment.NewLine + 
                                         $"\t\t{results.Racers[0]}" + Environment.NewLine +
                                         $"\t\t{results.Racers[1]}" + Environment.NewLine +
                                         $"\t\t{results.Racers[2]}" + Environment.NewLine;

            ObjectPrinter.SetUp<RaceResults>().PrintToString(results).Should().Be(expected);
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WithDictionary()
        {
            var studentMarks = new StudentMarks
            {
                Marks = new Dictionary<string, int>
                {
                    {"Anton", 3},
                    {"Andrey", 4},
                    {"Tamara", 5}
                }
            };

            var expected = "StudentMarks" + Environment.NewLine +
                                          "\tMarks = " + Environment.NewLine +
                                          "\t\tAnton = 3" + Environment.NewLine +
                                          "\t\tAndrey = 4" + Environment.NewLine +
                                          "\t\tTamara = 5" + Environment.NewLine;

            ObjectPrinter.SetUp<StudentMarks>().PrintToString(studentMarks).Should().Be(expected);
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WhenEnumerableIsEmpty()
        {
            var nameSet = new NameSet { Names = new string[0]};
            var expected = "NameSet" + Environment.NewLine +
                                     $"\tNames = Empty {nameSet.Names.GetType().Name}" + Environment.NewLine;

            ObjectPrinter.SetUp<NameSet>().PrintToString(nameSet).Should().Be(expected);
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WhenObjectHasCycledReference()
        {
            var college = new College {StudentList = new List<Student>()};
            college.StudentList.Add(new Student("Vasiliy", college));
            college.StudentList.Add(new Student("Svetlana", college));

            var expected = "College" + Environment.NewLine +
                                     "\tStudentList = " + Environment.NewLine +
                                     "\t\tStudent" + Environment.NewLine +
                                     $"\t\t\tName = {college.StudentList[0].Name}" + Environment.NewLine +
                                     "\t\t\tPlaceOfStudy = (this) College" + Environment.NewLine +
                                     "\t\tStudent" + Environment.NewLine +
                                     $"\t\t\tName = {college.StudentList[1].Name}" + Environment.NewLine +
                                     "\t\t\tPlaceOfStudy = (this) College" + Environment.NewLine;

            ObjectPrinter.SetUp<College>().PrintToString(college).Should().Be(expected);
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WhenEnumerableHasEqualObjects()
        {
            var college = new College() {StudentList = new List<Student>()};
            var student = new Student("Dimon" , college);
            college.StudentList.Add(student);
            college.StudentList.Add(student);

            var expected = "College" + Environment.NewLine +
                                     "\tStudentList = " + Environment.NewLine +
                                     "\t\tStudent" + Environment.NewLine +
                                     $"\t\t\tName = {college.StudentList[0].Name}" + Environment.NewLine +
                                     "\t\t\tPlaceOfStudy = (this) College" + Environment.NewLine +
                                     "\t\tStudent" + Environment.NewLine +
                                     $"\t\t\tName = {college.StudentList[1].Name}" + Environment.NewLine +
                                     "\t\t\tPlaceOfStudy = (this) College" + Environment.NewLine;

            ObjectPrinter.SetUp<College>().PrintToString(college).Should().Be(expected);
        }

        [Test] public void PrintingToString_ThrowsArgumentException_WhenTypeIsExcludedTwice()
        {

            Action action = () => ObjectPrinter.SetUp<Person>().Exclude<int>().Exclude<int>();
            action.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void PrintingToString_ThrowsArgumentException_WhenPropertyIsExcludedTwice()
        {

            Action action = () => ObjectPrinter.SetUp<Person>().Exclude(person => person.Age).Exclude(person => person.Age);
            action.ShouldThrow<ArgumentException>();
        }


        [Test]
        public void PrintingToString_ReturnsCorrectString_WhenPropertyIsTrimmedToLengthTwice()
        {
            var expected = "Person" + Environment.NewLine +
                           "\tId = Guid" + Environment.NewLine +
                           $"\tName = {person.Name.Substring(0, 3)}" + Environment.NewLine +
                           $"\tHeight = {person.Height}" + Environment.NewLine +
                           $"\tAge = {person.Age}" + Environment.NewLine;

            ObjectPrinter.SetUp<Person>()
                .For(person => person.Name)
                .TrimToLength(5).
                For(person => person.Name)
                .TrimToLength(3)
                .PrintToString(person)
                .Should()
                .Be(expected);
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WhenTrimmingLengthIsBiggerThanActualStringLength()
        {
            var expected = "Person" + Environment.NewLine +
                           "\tId = Guid" + Environment.NewLine +
                           $"\tName = {person.Name}" + Environment.NewLine +
                           $"\tHeight = {person.Height}" + Environment.NewLine +
                           $"\tAge = {person.Age}" + Environment.NewLine;

            ObjectPrinter.SetUp<Person>()
                .For(person => person.Name)
                .TrimToLength(42)
                .PrintToString(person)
                .Should()
                .Be(expected);
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WithLastDeclaredSerializationOfType()
        {
            var expected = "Person" + Environment.NewLine +
                           "\tId = Guid" + Environment.NewLine +
                           $"\tName = {person.Name}" + Environment.NewLine +
                           $"\tHeight = {person.Height}" + Environment.NewLine +
                           $"\tAge = {person.Age} years" + Environment.NewLine;

            ObjectPrinter
                .SetUp<Person>()
                .For<int>()
                .Use(i => i.ToString("X"))
                .For<int>()
                .Use(i => $"{i} years")
                .PrintToString(person)
                .Should()
                .Be(expected);
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WithLastDeclaredSerializationOfProperty()
        {
            var expected = "Person" + Environment.NewLine +
                           "\tId = Guid" + Environment.NewLine +
                           $"\tName = _{person.Name}_" + Environment.NewLine +
                           $"\tHeight = {person.Height}" + Environment.NewLine +
                           $"\tAge = {person.Age}" + Environment.NewLine;

            ObjectPrinter
                .SetUp<Person>()
                .For(person => person.Name)
                .Use(name => $"xxx_!{name}!_xxx")
                .For(person => person.Name)
                .Use(name => $"_{name}_")
                .PrintToString(person)
                .Should()
                .Be(expected);
        }

        [Test]
        public void PrintingToString_ReturnsCorrectString_WithLastDeclaredCultureOfType()
        {
            var expected = "Person" + Environment.NewLine +
                           "\tId = Guid" + Environment.NewLine +
                           $"\tName = {person.Name}" + Environment.NewLine +
                           $"\tHeight = {person.Height.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine +
                           $"\tAge = {person.Age}" + Environment.NewLine;

            ObjectPrinter
                .SetUp<Person>()
                .For<double>()
                .Use(CultureInfo.GetCultureInfo("ru-RU"))
                .For<double>()
                .Use(CultureInfo.InvariantCulture)
                .PrintToString(person)
                .Should()
                .Be(expected);
        }
    }
}