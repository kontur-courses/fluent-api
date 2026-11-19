using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class EdgeCasePrintingTests
    {
        [Test]
        public void PrintingType_UsesSpecifiedCultureForDateTime()
        {
            var value = new DateTimeHolder
            {
                CreatedAt = new DateTime(2026, 11, 1, 5, 40, 12)
            };

            var result = ObjectPrinter.For<DateTimeHolder>()
                .Printing<DateTime>()
                .Using(CultureInfo.GetCultureInfo("fr-FR"))
                .PrintToString(value);

            Assert.That(
                result,
                Does.Contain("CreatedAt = 01/11/2026 05:40:12"));
        }

        [Test]
        public void Printing_UsesCustomFormatterForField()
        {
            var record = new FieldRecord();

            var result = ObjectPrinter.For<FieldRecord>()
                .Printing(item => item.Number)
                .Using(number => $"Number: {number}")
                .PrintToString(record);

            var expected =
                "FieldRecord" + PrintingTestFormat.NewLine +
                "\tNumber = Number: 17" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Printing_IsScopedToSelectedMemberWhenNamesMatch()
        {
            var person = new PersonWithChild
            {
                Name = "Parent",
                Child = new Child
                {
                    Name = "Nested"
                }
            };

            var result = ObjectPrinter.For<PersonWithChild>()
                .Printing(p => p.Name)
                .Using(name => $"Root: {name}")
                .PrintToString(person);

            var expected =
                "PersonWithChild" + PrintingTestFormat.NewLine +
                "\tName = Root: Parent" + PrintingTestFormat.NewLine +
                "\tChild = Child" + PrintingTestFormat.NewLine +
                "\t\tName = Nested" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ExcludingType_DoesNotInvokeExcludedNestedPropertyGetter()
        {
            var parent = new ParentWithTrackedChild();

            var result = ObjectPrinter.For<ParentWithTrackedChild>()
                .Excluding<ChildDetails>()
                .PrintToString(parent);

            var expected =
                "ParentWithTrackedChild" + PrintingTestFormat.NewLine +
                "\tGetterCalls = 0" + PrintingTestFormat.NewLine;

            Assert.Multiple(() =>
            {
                Assert.That(parent.GetterCalls, Is.Zero);
                Assert.That(result, Is.EqualTo(expected));
            });
        }

        [Test]
        public void ExcludingType_ExcludesNullableMembersOfUnderlyingType()
        {
            var record = new OptionalAge
            {
                Age = 25
            };

            var result = ObjectPrinter.For<OptionalAge>()
                .Excluding<int>()
                .PrintToString(record);

            var expected =
                "OptionalAge" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintingNullMember_UsesCustomFormatter()
        {
            var formatterCalls = 0;

            var record = new NullableName
            {
                Name = null
            };

            var result = ObjectPrinter.For<NullableName>()
                .Printing(p => p.Name)
                .Using(name =>
                {
                    formatterCalls++;
                    return name ?? "Не указано";
                })
                .PrintToString(record);

            var expected =
                "NullableName" + PrintingTestFormat.NewLine +
                "\tName = Не указано" + PrintingTestFormat.NewLine;

            Assert.Multiple(() =>
            {
                Assert.That(formatterCalls, Is.EqualTo(1));
                Assert.That(result, Is.EqualTo(expected));
            });
        }

        [Test]
        public void PrintingType_UsesFormatterForNullMembers()
        {
            var person = new NullableName
            {
                Name = null
            };

            var result = ObjectPrinter.For<NullableName>()
                .Printing<string?>()
                .Using(name => name ?? "Отсутствует")
                .PrintToString(person);

            var expected =
                "NullableName" + PrintingTestFormat.NewLine +
                "\tName = Отсутствует" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Printing_DoesNotPrintPrivateMembers()
        {
            var record = new PrivateMemberRecord();

            var result = ObjectPrinter.For<PrivateMemberRecord>()
                .PrintToString(record);

            var expected =
                "PrivateMemberRecord" + PrintingTestFormat.NewLine +
                "\tPublicName = Visible" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_UsesEmptyStringWhenFormattableReturnsNull()
        {
            var result = ObjectPrinter.For<NullFormattable>()
                .PrintToString(new NullFormattable());

            Assert.That(result, Is.EqualTo(PrintingTestFormat.NewLine));
        }

        [Test]
        public void PrintToString_UsesEmptyStringWhenDictionaryKeyToStringReturnsNull()
        {
            var values = new System.Collections.Generic.Dictionary<NullToStringKey, int>
            {
                [new NullToStringKey()] = 42
            };

            var result = ObjectPrinter.For<System.Collections.Generic.Dictionary<NullToStringKey, int>>()
                .PrintToString(values);

            var expected =
                "\t[] = 42" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        private sealed class DateTimeHolder
        {
            public DateTime CreatedAt { get; set; }
        }

        private sealed class FieldRecord
        {
            public int Number = 17;
        }

        private sealed class PersonWithChild
        {
            public string Name { get; set; } = string.Empty;

            public Child Child { get; set; } = new();
        }

        private sealed class Child
        {
            public string Name { get; set; } = string.Empty;
        }

        private sealed class ParentWithTrackedChild
        {
            public int GetterCalls { get; private set; }

            public ChildDetails Details
            {
                get
                {
                    GetterCalls++;
                    return new ChildDetails();
                }
            }
        }

        private sealed class ChildDetails
        {
            public string Value { get; set; } = "Details";
        }

        private sealed class OptionalAge
        {
            public int? Age { get; set; }
        }

        private sealed class NullableName
        {
            public string? Name { get; set; }
        }

        private sealed class PrivateMemberRecord
        {
            private readonly string hiddenField = "Hidden field";

            private string HiddenProperty { get; } = "Hidden property";

            public string PublicName { get; } = "Visible";

            public string ReadPrivateMembers()
            {
                return hiddenField + HiddenProperty;
            }
        }

        private sealed class NullFormattable : IFormattable
        {
            public string ToString(string? format, IFormatProvider? formatProvider)
            {
                return null!;
            }
        }

        private sealed class NullToStringKey
        {
            public override string ToString()
            {
                return null!;
            }

            public override int GetHashCode()
            {
                return 1;
            }

            public override bool Equals(object? obj)
            {
                return obj is NullToStringKey;
            }
        }
    }
}