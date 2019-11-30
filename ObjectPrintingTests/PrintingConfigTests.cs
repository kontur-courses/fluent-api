using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Configs;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class PrintingConfigTests
    {
        private static readonly string newLine = Environment.NewLine;

        private static readonly Person person = new Person
        {
            Id = new Guid(),
            Name = "Alex",
            Surname = "Suvorov",
            Age = 45,
            Height = 172.6,
            Citizenship = "Russian"
        };


        [TestCase('b')]
        [TestCase(false)]
        [TestCase(true)]
        [TestCase((short) 314)]
        [TestCase((ushort) 314)]
        [TestCase(314)]
        [TestCase((uint) 314)]
        [TestCase((long) 314)]
        [TestCase((ulong) 314)]
        [TestCase(3.14159)]
        [TestCase(3.14159f)]
        [TestCase("this is string")]
        [TestCase("")]
        public void PrintToString_PrimitiveTypeObject_ShouldReturnValueOfToString<T>(T obj)
        {
            var printer = ObjectPrinter.For<T>();
            var expected = obj.ToString();

            var actual = printer.PrintToString(obj);

            actual.Should().BeEquivalentTo(expected);
        }


        private static IEnumerable<TestCaseData> GeneratePrimitiveTypeObjectAndSerialize()
        {
            yield return new TestCaseData("simple text", (Func<string, string>) (x => "other text"));

            yield return new TestCaseData(3.1415, (Func<double, string>) (x => $"{x / 2}"));

            yield return new TestCaseData(31415, (Func<int, string>) (x => $"{x / 2}"));

            yield return new TestCaseData(3.1415f, (Func<float, string>) (x => $"{x / 2}"));
        }

        [TestCaseSource(nameof(GeneratePrimitiveTypeObjectAndSerialize))]
        public void Serializing_PrimitiveTypeObjectBySerialize_ShouldReturnResultOfSerialize<T>(
            T obj, Func<T, string> serialize)
        {
            var printer = ObjectPrinter.For<T>().Serializing<T>().Using(serialize);
            var expected = serialize(obj);

            var actual = printer.PrintToString(obj);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Excluding_TypeWhichObjectPropertiesHaveNotContains_ShouldNotThrowException()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<float>();

            var expected =
                $"Id = {person.Id}{newLine}" +
                $"Name = {person.Name}{newLine}" +
                $"Surname = {person.Surname}{newLine}" +
                $"Height = {person.Height}{newLine}" +
                $"Age = {person.Age}{newLine}" +
                $"Citizenship = {person.Citizenship}";

            var actual = printer.PrintToString(person);

            actual.Should().BeEquivalentTo(expected);
        }

        private static IEnumerable<TestCaseData> GenerateObjectAndExcludedTypesAndSerializingResult()
        {
            yield return new TestCaseData(
                    person,
                    string.Empty,
                    $"Id = {person.Id}{newLine}Height = {person.Height}{newLine}Age = {person.Age}")
                .SetName("excluding all property with string type");

            yield return new TestCaseData(
                    person,
                    double.NaN,
                    $"Id = {person.Id}{newLine}Name = {person.Name}{newLine}Surname = {person.Surname}" +
                    $"{newLine}Age = {person.Age}{newLine}Citizenship = {person.Citizenship}")
                .SetName("excluding property with double type");

            yield return new TestCaseData(
                    person,
                    Guid.Empty,
                    $"Name = {person.Name}{newLine}" +
                    $"Surname = {person.Surname}{newLine}Height = {person.Height}{newLine}" +
                    $"Age = {person.Age}{newLine}Citizenship = {person.Citizenship}")
                .SetName("excluding property with Guid type");

            yield return new TestCaseData(
                    person,
                    int.MinValue,
                    $"Id = {person.Id}{newLine}Name = {person.Name}{newLine}" +
                    $"Surname = {person.Surname}{newLine}Height = {person.Height}{newLine}" +
                    $"Citizenship = {person.Citizenship}")
                .SetName("excluding property with int type");
        }

        [TestCaseSource(nameof(GenerateObjectAndExcludedTypesAndSerializingResult))]
        public void Exclude_Type_ObjectsWithThisTypeShouldNotSerialize<T>(Person p, T _, string expected)
        {
            var printer = ObjectPrinter.For<Person>().Excluding<T>();

            var actual = printer.PrintToString(p);

            actual.Should().BeEquivalentTo(expected);
        }

        private static IEnumerable<TestCaseData> GetObjectsVariousTypes()
        {
            yield return new TestCaseData(new Person());

            yield return new TestCaseData(new List<int>());

            yield return new TestCaseData(new Stack<string>());
        }

        [TestCase("text")]
        [TestCase(314)]
        [TestCase(3.14f)]
        [TestCase(3.14)]
        [TestCaseSource(nameof(GetObjectsVariousTypes))]
        public void Serializing_ForType_ShouldReturnRightContextForThisType<T>(T obj)
        {
            var printer = ObjectPrinter.For<T>();

            var expected = new PropertySerializingConfig<T, T>(printer);
            var actual = printer.Serializing<T>();

            actual.Should().BeOfType(expected.GetType());
        }

        private static IEnumerable<TestCaseData> GenerateObjAndObjToExcludedPropertyFuncAndSerializingResult()
        {
            Expression<Func<Person, string>> personToName = s => s.Name;
            Expression<Func<Person, string>> personToSurname = s => s.Surname;
            Expression<Func<Person, int>> personToAge = s => s.Age;
            Expression<Func<Person, double>> personToHeight = s => s.Height;
            Expression<Func<Person, Guid>> personToId = s => s.Id;
            Expression<Func<Person, string>> personToCitizenship = s => s.Citizenship;

            yield return new TestCaseData(
                person,
                personToId,
                $"Name = {person.Name}{newLine}" +
                $"Surname = {person.Surname}{newLine}Height = {person.Height}{newLine}" +
                $"Age = {person.Age}{newLine}Citizenship = {person.Citizenship}");

            yield return new TestCaseData(
                person,
                personToName,
                $"Id = {person.Id}{newLine}" +
                $"Surname = {person.Surname}{newLine}Height = {person.Height}{newLine}" +
                $"Age = {person.Age}{newLine}Citizenship = {person.Citizenship}");

            yield return new TestCaseData(
                person,
                personToSurname,
                $"Id = {person.Id}{newLine}Name = {person.Name}{newLine}" +
                $"Height = {person.Height}{newLine}" +
                $"Age = {person.Age}{newLine}Citizenship = {person.Citizenship}");

            yield return new TestCaseData(
                person,
                personToHeight,
                $"Id = {person.Id}{newLine}Name = {person.Name}{newLine}" +
                $"Surname = {person.Surname}{newLine}" +
                $"Age = {person.Age}{newLine}Citizenship = {person.Citizenship}");

            yield return new TestCaseData(
                person,
                personToAge,
                $"Id = {person.Id}{newLine}Name = {person.Name}{newLine}" +
                $"Surname = {person.Surname}{newLine}Height = {person.Height}{newLine}" +
                $"Citizenship = {person.Citizenship}");

            yield return new TestCaseData(
                person,
                personToCitizenship,
                $"Id = {person.Id}{newLine}Name = {person.Name}{newLine}" +
                $"Surname = {person.Surname}{newLine}Height = {person.Height}{newLine}" +
                $"Age = {person.Age}");
        }

        [TestCaseSource(nameof(GenerateObjAndObjToExcludedPropertyFuncAndSerializingResult))]
        public void Excluding_PropertyByFunc_ShouldNotPrintExcludedProperty<T>(Person obj,
            Expression<Func<Person, T>> objToExcludedProperty, string expected)
        {
            var printer = ObjectPrinter.For<Person>().Excluding<T>(objToExcludedProperty);
            var actual = printer.PrintToString(obj);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void PrintToString_WithCyclingReference_ShouldNotThrowException()
        {
            var obj = new PersonWithParent {Name = "Bob", Age = 42};
            obj.Parent = obj;
            
            var expected = $"Name = Bob{newLine}Age = 42{newLine}Parent = [Cyclic reference detected]";

            var actual = ObjectPrinter
                .For<PersonWithParent>()
                .PrintToString(obj);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void PrintToString_WithComplexType()
        {
            var parent = new PersonWithParent() { Name = "Mike", Age = 62 };
            var person1 = new PersonWithParent {Name = "Bob", Age = 42};
            var person2 = new PersonWithParent {Name = "Alice", Age = 42,  Parent = parent};
            
            var container = new PersonWithParentContainer {Person1 = person1, Person2 = person2};
            var expected = @"Person1
	Name = Bob
	Age = 42
	Parent = null
Person2
	Name = Alice
	Age = 42
	Parent
		Name = Mike
		Age = 62
		Parent = null";

            var actual = ObjectPrinter
                .For<PersonWithParentContainer>()
                .PrintToString(container);

            actual.Should().BeEquivalentTo(expected);
        }

        private static IEnumerable<TestCaseData> GenerateCollectionAndSerializingResult()
        {
            yield return new TestCaseData(new List<int> {3, 1, 4}, $"[3,{newLine}1,{newLine}4]{newLine}");

            yield return new TestCaseData(new HashSet<string> {"first", "second"}, $"[first,{newLine}second]{newLine}");

            yield return new TestCaseData(new Dictionary<double, bool> {[0.0] = true, [-3.14] = false},
                $"[Key = 0{newLine}Value = True,{newLine}Key = -3.14{newLine}Value = False]{newLine}");
        }

        [TestCaseSource(nameof(GenerateCollectionAndSerializingResult))]
        public void PrintToString_WithCollection_ShouldRightSerializeCollection(IEnumerable enumerable, string expected)
        {
            var printer = ObjectPrinter.For<IEnumerable>();

            var actual = printer.PrintToString(enumerable);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void PrintToString_ObjectWithCollectionTypeProperty_ShouldRightSerializeThisProperty()
        {
            var person1 = new Person() {Name = "Tom", Age = 14};
            var person2 = new Person() {Name = "Bob", Age = 13};
            var @class = new Class() {Students = new List<Person>() {person1, person2}, ClassNumber = 7};
            var printer = ObjectPrinter.For<Class>().Excluding<double>().Excluding<Guid>();
            var newLine = Environment.NewLine;
            var expected = $"students = [Name = Tom{newLine}Surname = null{newLine}Age = 14{newLine}" +
                           $"Citizenship = null,{newLine}Name = Bob{newLine}Surname = null" +
                           $"{newLine}Age = 13{newLine}Citizenship = null]{newLine + newLine}classNumber = 7";

            var actual = printer.PrintToString(@class);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Serializing_ComplexObjectBySerialize_ShouldReturnResultOfSerialize()
        {
            string Serialize(Person s) => "person serialized by personSerializer";

            var printer = ObjectPrinter.For<Person>().Serializing<Person>().Using(Serialize);
            var expected = Serialize(person);

            var actual = printer.PrintToString(person);

            actual.Should().BeEquivalentTo(expected);
        }


        [Test]
        public void Serializing_WithCulture_ShouldReturnResultOfSerializationUsingCulture()
        {
            var obj = 0.42;
            var printer = ObjectPrinter.For<double>().Serializing<double>().Using(CultureInfo.InvariantCulture);
            var expected = "0.42";

            var actual = printer.PrintToString(obj);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Serializing_SpecifyPropertySerialization_ShouldReturnResultOfThisSerializationForThisProperty()
        {
            var @class = new Class() {ClassNumber = 15, Students = new List<Person> {person}};
            string Serialize(List<Person> s) => "students serialized by studentsSerializer";
            var printer = ObjectPrinter.For<Class>().Serializing(x => x.Students).Using(Serialize);
            var expected = "Students = " + Serialize(@class.Students) + newLine + "ClassNumber = 15";

            var actual = printer.PrintToString(@class);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Serializing_BySubstringGivingProperty_ShouldReturnRightValue()
        {
            var printer = ObjectPrinter.For<Person>().Serializing(x => x.Surname).Take(4);
            var expected = $"Id = {person.Id}{newLine}Name = {person.Name}{newLine}Surname = " +
                           $"{person.Surname.Substring(0, 4)}{newLine}Height = {person.Height}{newLine}" +
                           $"Age = {person.Age}{newLine}Citizenship = {person.Citizenship}";

            var actual = printer.PrintToString(person);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Excluding_DifferentTypes_ShouldReturnNewObjectPrinterForEachExcluding()
        {
            var obj = new SimplePerson() {Name = "Bob", Age = 42};
            var printer = ObjectPrinter.For<SimplePerson>();
            var printerWithoutString = printer.Excluding<string>();
            var printerWithoutInt = printer.Excluding<int>();

            var expectedForPrinterWithoutInt = "Name = Bob";

            var actualForPrinterWithoutInt = printerWithoutInt.PrintToString(obj);

            actualForPrinterWithoutInt.Should().BeEquivalentTo(expectedForPrinterWithoutInt);
        }

        [Test]
        public void Excluding_DifferentProperties_ShouldReturnNewObjectPrinterForEachExcluding()
        {
            var obj = new SimplePerson {Name = "Bob", Age = 42};
            var printer = ObjectPrinter.For<SimplePerson>();
            var printerWithoutName = printer.Excluding(x => x.Name);
            var printerWithoutAge = printer.Excluding(x => x.Age);

            var expectedForPrinterWithoutAge = "Name = Bob";

            var actualForPrinterWithoutInt = printerWithoutAge.PrintToString(obj);

            actualForPrinterWithoutInt.Should().BeEquivalentTo(expectedForPrinterWithoutAge);
        }

        [Test]
        public void Serializing_CreateDifferentSerializeConfigsForOneType_ConfigsShouldBeDifferent()
        {
            var obj = new SimplePerson {Name = "Bob", Age = 42};
            var printer = ObjectPrinter.For<SimplePerson>();
            var first = printer.Serializing<string>().Using(x => "First");
            var second = printer.Serializing<string>().Using(x => "Second");
            var firstExpected = $"Name = First{newLine}Age = 42";
            var secondExpected = $"Name = Second{newLine}Age = 42";

            var firstActual = first.PrintToString(obj);
            var secondActual = second.PrintToString(obj);

            firstActual.Should().BeEquivalentTo(firstExpected);
            secondActual.Should().BeEquivalentTo(secondExpected);
        }

        [Test]
        public void Serializing_CreateDifferentSerializerConfigsForOneProperty_ConfigsShouldBeDifferent()
        {
            var obj = new SimplePerson {Name = "Bob", Age = 42};
            var printer = ObjectPrinter.For<SimplePerson>();
            var first = printer.Serializing(x => x.Age).Using(y => "0");
            var second = printer.Serializing(x => x.Age).Using(y => "100");
            var firstExpected = $"Name = Bob{newLine}Age = 0";
            var secondExpected = $"Name = Bob{newLine}Age = 100";

            var firstActual = first.PrintToString(obj);
            var secondActual = second.PrintToString(obj);

            firstActual.Should().BeEquivalentTo(firstExpected);
            secondActual.Should().BeEquivalentTo(secondExpected);
        }
    }
}