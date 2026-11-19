using System;
using System.Collections.Generic;
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

            Assert.That(result, Does.Contain("CreatedAt = 01/11/2026 05:40:12"));
        }

        [Test]
        public void PrintingStringMember_TrimsValueToZeroLength()
        {
            var person = new Person
            {
                Name = "Alexander"
            };

            var result = ObjectPrinter.For<Person>()
                .Printing(pers => pers.Name)
                .TrimmedToLength(0)
                .PrintToString(person);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("Name = " + Environment.NewLine));

                Assert.That(result, Does.Not.Contain("Alexander"));
            });
        }

        [Test]
        public void PrintingMember_UsesCustomFormatterForField()
        {
            var record = new FieldRecord();

            var result = ObjectPrinter.For<FieldRecord>()
                .Printing(item => item.Number)
                .Using(number => $"Number: {number}")
                .PrintToString(record);

            Assert.That(result, Does.Contain("Number = Number: 17"));
        }

        [Test]
        public void PrintingMember_IsScopedToSelectedMemberWhenNamesMatch()
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
                .Printing(item => item.Name)
                .Using(name => $"Root: {name}")
                .PrintToString(person);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("Name = Root: Parent"));
                Assert.That(result, Does.Contain("Name = Nested"));
                Assert.That(result, Does.Not.Contain("Root: Nested"));
            });
        }

        [Test]
        public void ExcludingType_DoesNotInvokeExcludedNestedPropertyGetter()
        {
            var parent = new ParentWithTrackedChild();

            var result = ObjectPrinter.For<ParentWithTrackedChild>()
                .Excluding<ChildDetails>()
                .PrintToString(parent);

            Assert.Multiple(() =>
            {
                Assert.That(parent.GetterCalls, Is.Zero);
                Assert.That(result, Does.Not.Contain("Details = "));
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

            Assert.That(result, Does.Not.Contain("Age = "));
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
                .Printing(item => item.Name)
                .Using(name =>
                {
                    formatterCalls++;
                    return name ?? "Не указано";
                })
                .PrintToString(record);

            Assert.Multiple(() =>
            {
                Assert.That(formatterCalls, Is.EqualTo(1));
                Assert.That(result, Does.Contain("Name = Не указано"));
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

            Assert.That(result, Does.Contain("Name = Отсутствует"));
        }
        
        // [Test]
        // public void TrimmedToLength_AppliesAfterCustomFormatter()
        // {
        //     var person = new Person
        //     {
        //         Name = "Alex"
        //     };
        //
        //     var result = ObjectPrinter.For<Person>()
        //         .Printing(p => p.Name)
        //         .Using(name => $"Hello, {name}!")
        //         .TrimmedToLength(9)
        //         .PrintToString(person);
        //
        //     Assert.That(result, Does.Contain("Name = Hello, Al"));
        // }

        [Test]
        public void PrintToString_PrintsMultidimensionalArrayElements()
        {
            var numbers = new[,]
            {
                { 1, 2 },
                { 3, 4 }
            };

            var result = ObjectPrinter.For<int[,]>()
                .PrintToString(numbers);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("[0] = 1"));
                Assert.That(result, Does.Contain("[1] = 2"));
                Assert.That(result, Does.Contain("[2] = 3"));
                Assert.That(result, Does.Contain("[3] = 4"));
            });
        }

        [Test]
        public void PrintToString_PrintsSortedDictionaryEntries()
        {
            var scores = new SortedDictionary<string, int>
            {
                ["Alex"] = 95,
                ["Bob"] = 87
            };

            var result = ObjectPrinter.For<SortedDictionary<string, int>>()
                .PrintToString(scores);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("[Alex] = 95"));
                Assert.That(result, Does.Contain("[Bob] = 87"));
            });
        }

        [Test]
        public void PrintingType_UsesFormatterForDictionaryKeys()
        {
            var values = new Dictionary<int, string>
            {
                [7] = "Seven"
            };

            var result = ObjectPrinter.For<Dictionary<int, string>>()
                .Printing<int>()
                .Using(number => $"key-{number}")
                .PrintToString(values);

            Assert.That(result, Does.Contain("[key-7] = Seven"));
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
    }
}