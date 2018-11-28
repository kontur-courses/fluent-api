using System;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;

        [SetUp]
        public void Init()
        {
            person = new Person(Guid.NewGuid(), "Alex", 192.57, 21, 15);
        }

        [Test]
        public void ObjectPrinter_Should_ExcludePropertyByType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<int>()
                .Exclude<Guid>()
                .Exclude<float>()
                .Exclude<string>();
            var result = printer.PrintToString(person);
            var expectedResult = "Person\r\n\tHeight = 192,57\r\n";

            result.Should().BeEquivalentTo(expectedResult);
            Console.WriteLine(result);
        }

        [Test]
        public void ObjectPrinter_Should_ExcludePropertyByName()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude(p => p.Height)
                .Exclude(p => p.ArmLength)
                .Exclude(p => p.Id);
            var result = printer.PrintToString(person);
            var expectedResult = "Person\r\n\tName = Alex\r\n\tAge = 21\r\n\tNumberChildren = 2\r\n";

            result.Should().BeEquivalentTo(expectedResult);
            Console.WriteLine(result);
        }

        [Test]
        public void ObjectPrinter_Should_PerformAlternativeSerializationByType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Serializer<double>().Using(i => "Height = " + ((int)i).ToString())
                .Serializer<Guid>().Using(i => "Id = Guid");
            var result = printer.PrintToString(person);
            var expectedResult =
                "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 192\r\n\tAge = 21\r\n\tArmLength = 15\r\n\tNumberChildren = 2\r\n";

            result.Should().BeEquivalentTo(expectedResult);
            Console.WriteLine(result);
        }

        [Test]
        public void ObjectPrinter_Should_HandleNestedMembers()
        {
            var printer = ObjectPrinter.For<Person>();
            var result = printer.PrintToString(person);
            var expectedResult =
                "Person\r\n\tId = Guid\r\n\t\tEmpty = Guid\r\n\tName = Alex\r\n\tHeight = 192,57" +
                "\r\n\tAge = 21\r\n\tArmLength = 15\r\n\tNumberChildren = 2\r\n";

            result.Should().BeEquivalentTo(expectedResult);
            Console.WriteLine(result);
        }

        [Test]
        public void ObjectPrinter_Should_PerformAlternativeSerializationByName()
        {
            var printer = ObjectPrinter.For<Person>()
                .Serializer(p => p.Height).Using(i => "Height = " + ((int)i).ToString())
                .Serializer(p => p.NumberChildren).Using(i => "NumberChildren = 3");
            var result = printer.PrintToString(person);
            var expectedResult =
                "Person\r\n\tId = Guid\r\n\t\tEmpty = Guid\r\n\tName = Alex\r\n\tHeight = 192" +
                "\r\n\tAge = 21\r\n\tArmLength = 15\r\n\tNumberChildren = 3\r\n";

            result.Should().BeEquivalentTo(expectedResult);
            Console.WriteLine(result);
        }

        [Test]
        public void ObjectPrinter_Should_TrimStringMembers()
        {
            var printer = ObjectPrinter.For<Person>()
                .Serializer(p => p.Name).TrimmedToLength(2);
            var result = printer.PrintToString(person);
            var expectedResult =
                "Person\r\n\tId = Guid\r\n\t\tEmpty = Guid\r\n\tName = Al\r\n\tHeight = 192,57" +
                "\r\n\tAge = 21\r\n\tArmLength = 15\r\n\tNumberChildren = 2\r\n";

            result.Should().BeEquivalentTo(expectedResult);
            Console.WriteLine(result);
        }

        [Test]
        public void ObjectPrinter_Should_ChangeCultureInfoForNumbers()
        {
            person.Age *= -1; person.ArmLength *= -1;
            var myCultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            myCultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            myCultureInfo.NumberFormat.NegativeSign = "~~";
            var printer = ObjectPrinter.For<Person>()
                .Serializer(p => p.Age).SetCultureInfo(myCultureInfo)
                .Serializer(p => p.ArmLength).SetCultureInfo(myCultureInfo)
                .Serializer(p => p.Height).SetCultureInfo(myCultureInfo);
            var result = printer.PrintToString(person);
            var expectedResult =
                "Person\r\n\tId = Guid\r\n\t\tEmpty = Guid\r\n\tName = Alex\r\n\tHeight = 192.57" +
                "\r\n\tAge = ~~21\r\n\tArmLength = ~~15\r\n\tNumberChildren = 2\r\n";

            result.Should().BeEquivalentTo(expectedResult);
            Console.WriteLine(result);
        }

        [Test]
        public void ObjectPrinter_Should_HaveSyntacticSugarWithConfiguration()
        {
            var result = person.Serialize(config => config.Exclude<int>());
            var expectedResult =
                "Person\r\n\tId = Guid\r\n\t\tEmpty = Guid\r\n\tName = Alex\r\n\tHeight = 192,57" +
                "\r\n\tArmLength = 15\r\n";

            result.Should().BeEquivalentTo(expectedResult);
            Console.WriteLine(result);
        }

        [Test]
        public void ObjectPrinter_Should_HaveSyntacticSugar()
        {
            var result = person.Serialize();
            var expectedResult =
                "Person\r\n\tId = Guid\r\n\t\tEmpty = Guid\r\n\tName = Alex\r\n\tHeight = 192,57" +
                "\r\n\tAge = 21\r\n\tArmLength = 15\r\n\tNumberChildren = 2\r\n";

            result.Should().BeEquivalentTo(expectedResult);
            Console.WriteLine(result);
        }

        [Test]
        public void ObjectPrinter_Should_SerializeCollections()
        {
            var anton = new Person(Guid.NewGuid(), "Anton", 183.7, 20, 60.4f);
            var vadim = new Person(Guid.NewGuid(), "Vadim", 182.22, 22, 97.5f);
            var statistic = new Statistic("GroupStatistic", person);
            statistic.Logbook.Add(anton.Name, 1);
            statistic.Logbook.Add(vadim.Name, 1);
            statistic.Persons.Add(anton);
            statistic.Persons.Add(vadim);
            var result = ObjectPrinter.For<Statistic>().PrintToString(statistic);
            var expectedResult = "Statistic\r\n\tName = GroupStatistic\r\n\tLogbook = Dictionary`2\r\n\t\t\t{\r\n\t\t\t" +
                                 "KeyValuePair`2\r\n\t\t\t\tKey = Anton\r\n\t\t\t\tValue = 1\r\n\t\t\tKeyValuePair`2\r\n\t\t\t\t" +
                                 "Key = Vadim\r\n\t\t\t\tValue = 1\r\n\t\t\t}\r\n\tPersons = List`1\r\n\t\t\t{\r\n\t\t\t" +
                                 "Person\r\n\t\t\t\tId = Guid\r\n\t\t\t\t\tEmpty = Guid\r\n\t\t\t\tName = Anton\r\n\t\t\t\t" +
                                 "Height = 183,7\r\n\t\t\t\tAge = 20\r\n\t\t\t\tArmLength = 60,4\r\n\t\t\t\t" +
                                 "NumberChildren = 2\r\n\t\t\tPerson\r\n\t\t\t\tId = Guid\r\n\t\t\t\t\tEmpty = Guid\r\n\t\t\t\t" +
                                 "Name = Vadim\r\n\t\t\t\tHeight = 182,22\r\n\t\t\t\tAge = 22\r\n\t\t\t\tArmLength = 97,5\r\n\t\t\t\t" +
                                 "NumberChildren = 2\r\n\t\t\t}\r\n\tGroupLeader = Person\r\n\t\tId = Guid\r\n\t\t\tEmpty = Guid\r\n\t\t" +
                                 "Name = Alex\r\n\t\tHeight = 192,57\r\n\t\tAge = 21\r\n\t\tArmLength = 15\r\n\t\tNumberChildren = 2\r\n";
            result.Should().BeEquivalentTo(expectedResult);
            Console.WriteLine(result);
        }
    }
}