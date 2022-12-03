using System.Collections.Generic;
using System.Globalization;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private string text;
        private Person person;

        [SetUp]
        public void CreatePerson()
        {
            person = new Person
            {
                Name = "Bill",
                Height = 175.3,
                Age = 35
            };
        }

        [Test]
        public void PrintingConfig_EmptyInput_ShouldReturnEmptyLine()
        {
            var common = "";
            var config = ObjectPrinter.For<string>();

            text = config.PrintToString(common, 10);
            var expected = "";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_StringInput_ShouldReturnSameString()
        {
            var common = "some text";
            var config = ObjectPrinter.For<string>();

            text = config.PrintToString(common, 10);
            var expected = @"some text;
";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_MassiveInput()
        {
            var common = new [] { 2.0, 5.3, 10.154, 13.15, -1.65 };
            var config = ObjectPrinter.For<double[]>();

            text = config.PrintToString(common, 10);
            var expected = @"(enum) 
	[2, 5,3, 10,154, 13,15, -1,65]
";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_ListInput()
        {
            var common = new List<int>() { 2, 5, 10, 13, -1 };
            var config = ObjectPrinter.For<List<int>>();

            text = config.PrintToString(common, 10);
            var expected = @"(enum) 
	[2, 5, 10, 13, -1]
";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_DictionaryIntIntInput()
        {
            var common = new Dictionary<int, int>()
            {
                [2] = 5,
                [10] = 13,
                [4] = -1
            };
            var config = ObjectPrinter.For<Dictionary<int, int>>();

            text = config.PrintToString(common, 10);
            var expected = @"(dict) 
	2 = 5;
	10 = 13;
	4 = -1;
";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_DictionaryStringIntInput()
        {
            var common = new Dictionary<string, int>()
            {
                ["five"] = 5,
                ["thirteen"] = 13,
                ["minus one"] = -1
            };
            var config = ObjectPrinter.For<Dictionary<string, int>>();

            text = config.PrintToString(common, 10);
            var expected = @"(dict) 
	five = 5;
	thirteen = 13;
	minus one = -1;
";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_ListOfPersonInput()
        {
            var common = new List<Person>() { person, person, person };
            var config = ObjectPrinter.For<List<Person>>();

            text = config.PrintToString(common, 10);
            var expected = @"(enum) 
	[Person:
	Id: 00000000-0000-0000-0000-000000000000;
	Name: Bill;
	Height: 175,3;
	Age: 35;
, Person:
	Id: 00000000-0000-0000-0000-000000000000;
	Name: Bill;
	Height: 175,3;
	Age: 35;
, Person:
	Id: 00000000-0000-0000-0000-000000000000;
	Name: Bill;
	Height: 175,3;
	Age: 35;
]
";
            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_DeleteIntTypeFromPerson()
        {
            var config = ObjectPrinter.For<Person>();

            config.SelectType<int>().IgnoreType();
            text = config.PrintToString(person, 10);

            var expected = @"Person:
	Id: 00000000-0000-0000-0000-000000000000;
	Name: Bill;
	Height: 175,3;
";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_IgnoreIntPropertyThanOverrideIntType_ShouldIgnoreAllInt()
        {
            var config = ObjectPrinter.For<Person>();

            config.SelectType<int>()
                .IgnoreType()
                .SelectProperty(x => x.Age)
                .PrintAs(x => x + " полных лет");
            text = config.PrintToString(person, 10);

            var expected = @"Person:
	Id: 00000000-0000-0000-0000-000000000000;
	Name: Bill;
	Height: 175,3;
";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_OverrideStringTypeThanOverrideStringProp_ShouldProcessWithProp()
        {
            var config = ObjectPrinter.For<Person>();

            config.SelectType<string>()
                .PrintAs(x => x.ToUpper())
                .SelectProperty(x => x.Name)
                .PrintAs(x => $"Lower name: {x.ToLower()}");
            text = config.PrintToString(person, 10);

            var expected = @"Person:
	Id: 00000000-0000-0000-0000-000000000000;
	Lower name: bill
	Height: 175,3;
	Age: 35;
";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_DeletePropertyFromPerson()
        {
            var config = ObjectPrinter.For<Person>();

            config.SelectProperty(x => x.Name).IgnoreProperty();
            text = config.PrintToString(person, 10);

            var expected = @"Person:
	Id: 00000000-0000-0000-0000-000000000000;
	Height: 175,3;
	Age: 35;
";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_CustomTypeSerialize()
        {
            var config = ObjectPrinter.For<Person>();

            config.SelectType<string>().PrintAs(x => x.ToUpper());
            person.FriendsNames = new List<string>() { "Alex", "Allay", "King" };

            text = config.PrintToString(person, 10);

            var expected = @"Person:
	Id: 00000000-0000-0000-0000-000000000000;
	Name: BILL;
	Height: 175,3;
	Age: 35;
	(enum) FriendsNames:
		[ALEX, ALLAY, KING]
";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_CustomPropSerialize()
        {
            var config = ObjectPrinter.For<Person>();

            config.SelectProperty(x => x.Age).PrintAs(x => $"Возраст: {x} полных лет;");

            text = config.PrintToString(person, 10);

            var expected = @"Person:
	Id: 00000000-0000-0000-0000-000000000000;
	Name: Bill;
	Height: 175,3;
	Возраст: 35 полных лет;
";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_SetCultureForDouble()
        {
            var config = ObjectPrinter.For<double>();

            config.SetCulture<double>(CultureInfo.InvariantCulture);

            text = config.PrintToString(153.2, 10);

            var expected = @"153.2;
";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_TrimmedName()
        {
            var config = ObjectPrinter.For<Person>();

            config.SelectProperty(x => x.Name).TrimmedToLength(2);

            text = config.PrintToString(person, 10);

            var expected = @"Person:
	Id: 00000000-0000-0000-0000-000000000000;
	Bi
	Height: 175,3;
	Age: 35;
";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_CyclingObjectReference()
        {
            var config = ObjectPrinter.For<Person>();

            person.Dad = person;
            text = config.PrintToString(person, 10);

            var expected = @"Person:
	Id: 00000000-0000-0000-0000-000000000000;
	Name: Bill;
	Height: 175,3;
	Age: 35;
	Person: cycle;
";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_LowNesting()
        {
            var config = ObjectPrinter.For<Person>();

            person.Dad = new Person
            {
                Name = "Ron",
                Height = 170,
                Age = 78,
                Dad = new Person
                {
                    Name = "Lay",
                    Height = 189,
                    Age = 69
                }
            };

            text = config.PrintToString(person, 1);

            var expected = @"Person:
	Id: 00000000-0000-0000-0000-000000000000;
	Name: Bill;
	Height: 175,3;
	Age: 35;
	Person:
		Id: 00000000-0000-0000-0000-000000000000;
		Name: Ron;
		Height: 170;
		Age: 78;
";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_BigObject()
        {
            var person = new Person
            {
                Name = "Bill",
                Age = 35,
                ArmsLenght = new int[] { 540, 600 },
                FriendsNames = new List<string>() { "Rick", "Morgan", "Alex" },
                Dad = new Person
                {
                    Name = "Alex",
                    Age = 65,
                    Awards = new Dictionary<string, int>()
                    {
                        ["Mayor"] = 40,
                        ["President"] = 55
                    }
                },
                Awards = new Dictionary<string, int>()
                {
                    ["Best friend"] = 12,
                    ["Best dad"] = 25,
                    ["Best worker"] = 29
                }
            };

            var printer = ObjectPrinter.For<Person>();
             
            text = person.PrintToString(10);

            var expected = @"Person:
	Id: 00000000-0000-0000-0000-000000000000;
	Name: Bill;
	Height: 0;
	Age: 35;
	Person:
		Id: 00000000-0000-0000-0000-000000000000;
		Name: Alex;
		Height: 0;
		Age: 65;
		(dict) Awards:
			Mayor = 40;
			President = 55;
	(enum) ArmsLenght:
		[540, 600]
	(enum) FriendsNames:
		[Rick, Morgan, Alex]
	(dict) Awards:
		Best friend = 12;
		Best dad = 25;
		Best worker = 29;
";

            text.Should().Be(expected);
        }

        [TearDown]
        public void TearDown()
        {
            var testCont = TestContext.CurrentContext;
            var fileName = testCont.Test.ID;
            File.WriteAllText(@"..\\text.txt", text);
            text = null;
        }
    }
}