using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterTests
    {
        [Test]
        public void DefaultSerialization()
        {
            var expectedSerialization =
                "Person" + Environment.NewLine +
                '\t' + "Id = Guid" + Environment.NewLine +
                '\t' + "Name = Vasiliy" + Environment.NewLine +
                '\t' + "Height = 0" + Environment.NewLine +
                '\t' + "Age = 19" + Environment.NewLine;
            var person = new Person {Name = "Vasiliy", Age = 19};

            var serialized = person.PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void TypeExclusion()
        {
            var expectedSerialization =
                "Person" + Environment.NewLine +
                '\t' + "Name = Vasiliy" + Environment.NewLine +
                '\t' + "Height = 0" + Environment.NewLine +
                '\t' + "Age = 19" + Environment.NewLine;
            var person = new Person {Name = "Vasiliy", Age = 19};

            var serialized = person.Printing()
                .Exclude<Guid>()
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void MultipleTypesExclusion()
        {
            var expectedSerialization =
                "Person" + Environment.NewLine +
                '\t' + "Height = 0" + Environment.NewLine +
                '\t' + "Age = 19" + Environment.NewLine;
            var person = new Person {Name = "Vasiliy", Age = 19};

            var serialized = person.Printing()
                .Exclude<Guid>()
                .Exclude<string>()
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void TypeExclusionOfExcludedType()
        {
            var expectedSerialization =
                "Person" + Environment.NewLine +
                '\t' + "Name = Vasiliy" + Environment.NewLine +
                '\t' + "Height = 0" + Environment.NewLine +
                '\t' + "Age = 19" + Environment.NewLine;
            var person = new Person {Name = "Vasiliy", Age = 19};

            var serialized = person.Printing()
                .Exclude<Guid>()
                .Exclude<Guid>()
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void CustomTypeSerialization()
        {
            var person = new Person {Name = "Vasiliy", Age = 19};
            var guid = person.Id.ToString();
            var expectedSerialization =
                "Person" + Environment.NewLine +
                '\t' + $"Id = {guid}" + Environment.NewLine +
                '\t' + "Name = Vasiliy" + Environment.NewLine +
                '\t' + "Height = 0" + Environment.NewLine +
                '\t' + "Age = 19" + Environment.NewLine;

            var serialized = person.Printing()
                .For<Guid>().Use(s => s.ToString())
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void CustomTypeSerializationPrecedenceOverFinalTypes()
        {
            var person = new Person {Name = "Vasiliy", Age = 19};
            var expectedSerialization = "Person" + Environment.NewLine
                                                 + '\t' + "Id = Guid" + Environment.NewLine
                                                 + '\t' + "Name = Vasiliy" + Environment.NewLine
                                                 + '\t' + "Height = 0" + Environment.NewLine
                                                 + '\t' + "Age = NO" + Environment.NewLine;

            var serialized = person.Printing()
                .For<int>().Use(s => "NO")
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void RedefinitionOfCustomTypeSerialization()
        {
            var person = new Person {Name = "Vasiliy", Age = 19};
            var expectedSerialization = "Person" + Environment.NewLine
                                                 + '\t' + "Id = Guid" + Environment.NewLine
                                                 + '\t' + "Name = Vasiliy" + Environment.NewLine
                                                 + '\t' + "Height = 0" + Environment.NewLine
                                                 + '\t' + "Age = 19" + Environment.NewLine;
            var serialized = person.Printing()
                .For<Guid>().Use(s => s.ToString())
                .For<Guid>().Use(s => s.GetType().Name)
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void CultureInfoForNumericTypes_RussianCulture()
        {
            var person = new Person {Name = "Vasiliy", Age = 19, Height = 179.5d};
            var expectedSerialization =
                "Person" + Environment.NewLine +
                '\t' + "Id = Guid" + Environment.NewLine +
                '\t' + "Name = Vasiliy" + Environment.NewLine +
                '\t' + "Height = 179,5" + Environment.NewLine +
                '\t' + "Age = 19" + Environment.NewLine;

            var serialized = person.Printing()
                .For<double>().Use(CultureInfo.CreateSpecificCulture("ru-ru"))
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void CultureInfoForNumericTypes_InvariantCulture()
        {
            var person = new Person {Name = "Vasiliy", Age = 19, Height = 181.7d};
            var expectedSerialization =
                "Person" + Environment.NewLine +
                '\t' + "Id = Guid" + Environment.NewLine +
                '\t' + "Name = Vasiliy" + Environment.NewLine +
                '\t' + "Height = 181.7" + Environment.NewLine +
                '\t' + "Age = 19" + Environment.NewLine;

            var serialized = person.Printing()
                .For<double>().Use(CultureInfo.InvariantCulture)
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void CustomPropertySerialization()
        {
            var person = new Person {Name = "Vasiliy", Age = 19};
            var expectedSerialization =
                "Person" + Environment.NewLine +
                '\t' + "Id = Guid" + Environment.NewLine +
                '\t' + "Name = Vasiliy" + Environment.NewLine +
                '\t' + "Height = N/A" + Environment.NewLine +
                '\t' + "Age = 19" + Environment.NewLine;

            var serialized = person.Printing()
                .For(p => p.Height).Use(h => h < 1 ? "N/A" : h.ToString(CultureInfo.InvariantCulture))
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void StringTypesTrimming()
        {
            var expectedSerialization =
                "Person" + Environment.NewLine +
                '\t' + "Id = Guid" + Environment.NewLine +
                '\t' + "Name = Alex" + Environment.NewLine +
                '\t' + "Height = 0" + Environment.NewLine +
                '\t' + "Age = 19" + Environment.NewLine;
            var person = new Person {Name = "Alexander", Age = 19};

            var serialized = person.Printing()
                .For<string>().Trim(4)
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void StringPropertiesTrimming()
        {
            var expectedSerialization =
                "Person" + Environment.NewLine +
                '\t' + "Id = Guid" + Environment.NewLine +
                '\t' + "Name = Alex" + Environment.NewLine +
                '\t' + "Height = 0" + Environment.NewLine +
                '\t' + "Age = 19" + Environment.NewLine;
            var person = new Person {Name = "Alexander", Age = 19};

            var serialized = person.Printing()
                .For(p => p.Name).Trim(4)
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void PropertyExclusion()
        {
            var expectedSerialization =
                "Person" + Environment.NewLine +
                '\t' + "Id = Guid" + Environment.NewLine +
                '\t' + "Name = Vasiliy" + Environment.NewLine +
                '\t' + "Height = 0" + Environment.NewLine;
            var person = new Person {Name = "Vasiliy", Age = 19};

            var serialized = person.Printing()
                .Exclude(p => p.Age)
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void MultiplePropertiesExclusion()
        {
            var expectedSerialization =
                "Person" + Environment.NewLine +
                '\t' + "Name = Vasiliy" + Environment.NewLine +
                '\t' + "Height = 0" + Environment.NewLine;
            var person = new Person {Name = "Vasiliy", Age = 19};

            var serialized = person.Printing()
                .Exclude(p => p.Age)
                .Exclude(p => p.Id)
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void ReferenceLoopsHandling()
        {
            var selfReferrer = new SelfReferrer();

            Action printSelfReferrer = () => selfReferrer.PrintToString();

            printSelfReferrer.Should().NotThrow();
        }

        [Test]
        public void MaximumNesting()
        {
            var expectedSerialization =
                "SelfReferrer" + Environment.NewLine +
                '\t' + "X = 42" + Environment.NewLine +
                '\t' + "Self = SelfReferrer" + Environment.NewLine;
            var selfReferrer = new SelfReferrer();

            var serialized = selfReferrer.Printing()
                .WithMaximumNesting(1)
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void ArrayPrinting()
        {
            var expectedSerialization =
                "Int32[]" + Environment.NewLine +
                "\t - " + "1" + Environment.NewLine +
                "\t - " + "2" + Environment.NewLine +
                "\t - " + "4" + Environment.NewLine +
                "\t - " + "3" + Environment.NewLine;
            var arr = new[] {1, 2, 4, 3};

            var serialized = arr.PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void ListPrinting()
        {
            var expectedSerialization =
                "List`1" + Environment.NewLine +
                "\t - " + "h" + Environment.NewLine +
                "\t - " + "e" + Environment.NewLine +
                "\t - " + "l" + Environment.NewLine +
                "\t - " + "l" + Environment.NewLine +
                "\t - " + "o" + Environment.NewLine;
            string[] input = {"h", "e", "l", "l", "o"};
            var list = new List<string>(input);

            var serialized = list.PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void DictionaryPrinting()
        {
            var expectedSerialization =
                "Dictionary`2" + Environment.NewLine +
                "\t" + "hello" + " - " + "world" + Environment.NewLine +
                "\t" + "cold" + " - " + "outside" + Environment.NewLine;

            var dict = new Dictionary<string, string> {{"hello", "world"}, {"cold", "outside"}};

            var serialized = dict.PrintToString();

            serialized.Should().Be(expectedSerialization);
        }
    }
}