using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace ObjectPrinting.Solved.Tests
{
    public static class Sources
    {
        private static readonly Person _alexPerson = new Person { Name = "Alex", Age = 19 };

        public static IEnumerable<TestCaseData> Source_ShouldSerializeCustomlyWhenGivenCustomSerializer()
        {
            Expression<Func<Person, int>> selector = p => p.Age;
            Func<int, string> serializer = p => $"_-_ {p} _-_";
            yield return new(
                _alexPerson,
                selector,
                serializer,
                new Dictionary<string, int> {
                    { "Age", _alexPerson.Age }
                }
                )
            {
                TypeArgs = [typeof(Person), typeof(int)]
            };
        }

        public static IEnumerable<TestCaseData> Source_ShouldExcludeTypePropertyFromSerialization_WhenOrderedTo()
        {
            yield return new(
                _alexPerson,
                new HashSet<string>(["Id"]))
            { TypeArgs = [typeof(Person), typeof(Guid)] };
            yield return new(
                _alexPerson,
                new HashSet<string>(["Name", "Bio"]))
            { TypeArgs = [typeof(Person), typeof(string)] };
            var jakePerson = new Person() { Name = "Jake", Age = 8, Bio = "Cool" };
            yield return new(
                jakePerson,
                new HashSet<string>(["Height", "Weight"]))
            { TypeArgs = [typeof(Person), typeof(double)] };
        }

        public static IEnumerable<TestCaseData> Source_ShouldAllowFormattingCultureWithNumericTypes()
        {
            Person person = new() { Weight = 6, Height = 0.5 };
            yield return new(
                person,
                CultureInfo.GetCultureInfo("en-US"),
                new Dictionary<string, double>{
                    { "Weight", person.Weight },
                    { "Height", person.Height }
                })
            { TypeArgs = [typeof(Person), typeof(double)] };
            yield return new(
                person,
                CultureInfo.GetCultureInfo("ru-RU"),
                new Dictionary<string, double>{
                    { "Weight", person.Weight },
                    { "Height", person.Height }
                })
            { TypeArgs = [typeof(Person), typeof(double)] };
        }

        public static IEnumerable<TestCaseData> Source_ShouldSerializePropertiesAndFields_WhenGivenCustomSerializer()
        {
            Person person = new() { Weight = 6, Height = 0.5 };
            Expression<Func<Person, int>> selector = p => p.Age;
            Func<int, string> serializer = p => $"Majestic {p}";
            yield return new(
                person,
                selector,
                serializer,
                new Dictionary<string, int>{
                    { "Age", person.Age }
                })
            { TypeArgs = [typeof(Person), typeof(int)] };

            Expression<Func<Parent, Parent>> parentSelector = p => p.Child;
            Func<Parent, string> parentSerializer = p => $"Parent: {p}";
            Parent parent = new() { Name = "A", Child = new Parent { Name = "B" } };
            yield return new(
                parent,
                parentSelector,
                parentSerializer,
                new Dictionary<string, Parent>
                {
                    { "Child", parent.Child }
                })
            { TypeArgs = [typeof(Parent), typeof(Parent)] };
        }

        public static IEnumerable<TestCaseData> Source_ShouldTrimStringProperties_WhenOrderedTo()
        {
            Expression<Func<Person, string>> selector = person => person.Name;
            Dictionary<string, string> formattablePropNamesAndValues = new()
            {
                { "Name", "Alex" }
            };
            yield return new(_alexPerson, selector, formattablePropNamesAndValues, 2);
            formattablePropNamesAndValues = new()
            {
                { "Name", "Qwertyuiop" }
            };
            yield return new(new Person() { Name = "Qwertyuiop" }, selector, formattablePropNamesAndValues, 0);
        }

        public static IEnumerable<TestCaseData> Source_ShouldExcludeGivenPropertyFromSerialization_WhenOrderedTo()
        {
            Expression<Func<Person, Guid>> selector = person => person.Id;
            yield return new(_alexPerson, selector);
        }
    }

    internal static class Helpers
    {
        public static void TestSerializableProperties<TOwner, TPropType>(
            TOwner owner,
            IDictionary<string, TPropType> formattablePropNamesAndValues,
            PrintingConfig<TOwner> config,
            Func<TPropType, string> propToString)
        {
            var result = config.PrintToString(owner);

            var propsFormatted = GetSerializedProperties(result);
            using (new AssertionScope())
            {
                AssertFormattableProps(
                    formattablePropNamesAndValues,
                    propsFormatted,
                    propToString);
            }
        }

        public static IDictionary<string, string> GetSerializedProperties(string properties)
        {
            return properties
                .Split("\r\n")
                .Where(s => s.Contains('='))
                .Select(s => s.Split('='))
                .ToDictionary(s => s[0].Trim(), s => s[1].Trim());
        }

        public static void AssertFormattableProps<TPropType>(
            IDictionary<string, TPropType> formattablePropNamesAndValues,
            IDictionary<string, string> propsFormatted,
            Func<TPropType, string> paramToString)
        {
            foreach ((string paramName, TPropType paramValue) in formattablePropNamesAndValues)
            {
                var formattedProp = propsFormatted[paramName];
                string expected = paramToString(paramValue);
                formattedProp.Should().Be(expected);
            }
        }
    }

    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

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

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();

            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        // 1
        [TestCaseSource(typeof(Sources), nameof(Sources.Source_ShouldExcludeTypePropertyFromSerialization_WhenOrderedTo))]
        public void ShouldExcludeTypePropertyFromSerialization_WhenOrderedTo<TObject, TExcludeType>(
            TObject owner,
            HashSet<string> typesToExclude)
        {
            var printer = ObjectPrinter.For<TObject>().Excluding<TExcludeType>();

            Helpers.GetSerializedProperties(printer.PrintToString(owner))
                .Should()
                .NotContainKeys(typesToExclude);
        }

        // 2
        [TestCaseSource(typeof(Sources), nameof(Sources.Source_ShouldSerializeCustomlyWhenGivenCustomSerializer))]
        public void ShouldSerializeCustomly_WhenGivenCustomSerializer<TOwner, TPropType>(
            TOwner owner,
            Expression<Func<TOwner, TPropType>> selector,
            Func<TPropType, string> serializer,
            IDictionary<string, TPropType> formattablePropNamesAndValues)
        {
            var printer = ObjectPrinter.For<TOwner>().Printing(selector).Using(serializer);

            Helpers.TestSerializableProperties(
                owner,
                formattablePropNamesAndValues,
                printer,
                serializer
                );
        }

        // 3
        [TestCaseSource(typeof(Sources), nameof(Sources.Source_ShouldAllowFormattingCultureWithNumericTypes))]
        public void ShouldAllowFormattingCultureWithNumericTypes<TOwner, TFormattableType>(
            TOwner owner,
            CultureInfo culture,
            IDictionary<string, TFormattableType> formattablePropNamesAndValues
            )
            where TFormattableType : INumber<TFormattableType>
        {
            var printer = ObjectPrinter.For<TOwner>().Printing<TFormattableType>().Using(culture);

            Helpers.TestSerializableProperties(
                owner,
                formattablePropNamesAndValues,
                printer,
                p => p.ToString(null, culture));
        }

        // 4
        [TestCaseSource(typeof(Sources), nameof(Sources.Source_ShouldSerializePropertiesAndFields_WhenGivenCustomSerializer))]
        public void ShouldSerializePropertiesAndFields_WhenGivenCustomSerializer<TOwner, TPropType>(
            TOwner owner,
            Expression<Func<TOwner, TPropType>> selector,
            Func<TPropType, string> serializer,
            IDictionary<string, TPropType> formattablePropNamesAndValues
            )
        {
            var printer = ObjectPrinter.For<TOwner>().Printing(selector).Using(serializer);

            Helpers.TestSerializableProperties(
                owner,
                formattablePropNamesAndValues,
                printer,
                serializer);
        }

        // 5
        [TestCaseSource(typeof(Sources), nameof(Sources.Source_ShouldTrimStringProperties_WhenOrderedTo))]
        public void ShouldTrimStringProperties_WhenOrderedTo<TOwner>(
            TOwner owner,
            Expression<Func<TOwner, string>> selector,
            IDictionary<string, string> formattablePropNamesAndValues,
            int maxLen)
        {
            var printer = ObjectPrinter.For<TOwner>().Printing(selector).TrimmedToLength(maxLen);

            Helpers.TestSerializableProperties(
                owner,
                formattablePropNamesAndValues,
                printer,
                s => s.Length < maxLen ? s : s[..maxLen]);
        }

        // 6
        [TestCaseSource(typeof(Sources), nameof(Sources.Source_ShouldExcludeGivenPropertyFromSerialization_WhenOrderedTo))]
        public void ShouldExcludeGivenPropertyFromSerialization_WhenOrderedTo<TOwner, TPropType>(
            TOwner owner,
            Expression<Func<TOwner, TPropType>> selector
            )
        {
            var printer = ObjectPrinter.For<TOwner>().Excluding(selector);

            var result = printer.PrintToString(owner);

            var propsFormatted = Helpers.GetSerializedProperties(result);
            propsFormatted.Keys.Should().NotContain(PrintingConfig<TOwner>.GetMemberExpression(selector).Member.Name);
        }

        [Test]
        public void ShouldNotSerializeCustomly_WhenPropertyIsExcluded()
        {
            var person = new Person { Name = "Kevin" };
            var printer = ObjectPrinter
                .For<Person>()
                .Excluding(person => person.Name)
                .Printing(person => person.Name)
                .Using(property => $"ignore {property}");

            var result = printer.PrintToString(person);

            result.Should().Be($"Person#1\r\n\tId = Guid {Guid.Empty}\r\n\tHeight = 0\r\n\tWeight = 0\r\n\tAge = 0\r\n\tBio = null\r\n");
        }

        [Test]
        public void ShouldSerializeCustomlyWithLastGivenSerializer()
        {
            var person = new Person { Name = "Kevin" };
            var printer = ObjectPrinter
                .For<Person>()
                .Printing(person => person.Name)
                .Using(property => $"Cool {property}")
                .Printing(person => person.Name)
                .Using(property => $"Real cool {property}");

            var result = printer.PrintToString(person);

            result.Should().Be($"Person#1\r\n\tId = Guid {Guid.Empty}\r\n\tName = Real cool Kevin\r\n\tHeight = 0\r\n\tWeight = 0\r\n\tAge = 0\r\n\tBio = null\r\n");
        }

        [Test]
        public void ObjectPrinterExcludingShouldAffectAnotherInstanceOfSameClass()
        {
            var parent = new Parent { Name = "A", Child = new Parent { Name = "B" } };
            var printer = ObjectPrinter.For<Parent>().Excluding(parent => parent.Name);

            var result = printer.PrintToString(parent);

            result.Should().Be("Parent#1\r\n\tChild = Parent#2\r\n\t\tChild = null\r\n");
        }

        [Test]
        public void ShouldNotThrowStackOverflowException_WhenReferringToSelf()
        {
            var parent = new Parent { Name = "A", Child = new Parent { Name = "B" } };
            parent.Child.Child = parent;
            var printer = ObjectPrinter.For<Parent>();

            var result = printer.PrintToString(parent);

            result.Should().Be("Parent#1\r\n\tName = A\r\n\tChild = Parent#2\r\n\t\tName = B\r\n\t\tChild = <Parent#1>\r\n");
        }
    }
}