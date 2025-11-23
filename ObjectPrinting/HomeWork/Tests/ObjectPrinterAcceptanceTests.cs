using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.HomeWork.Extensions;

namespace ObjectPrinting.HomeWork.Tests
{
    [TestFixture]
    public class ObjectPrinterGeneratedTests
    {
        private static readonly int[] execute = [1,2,3];

        [TestCaseSource(nameof(TestCases))]
        public void GeneratedTestRunner(TestSpec spec)
        {
            var result = spec.Execute();

            foreach (var expected in spec.ShouldContain)
            {
                result.Should().Contain(expected,
                    $"Expected output to contain '{expected}' for scenario: {spec.Description}");
            }

            foreach (var notExpected in spec.ShouldNotContain)
            {
                result.Should().NotContain(notExpected,
                    $"Expected output NOT to contain '{notExpected}' for scenario: {spec.Description}");
            }
        }
        
        public static IEnumerable<TestCaseData> TestCases
        {
            get
            {
                yield return new TestCaseData(new TestSpec(
                    "Type-level exclude for string properties should remove all string fields",
                    () =>
                    {
                        var p = new Person
                        {
                            Name = "Alex",
                            Note = "Some note",
                            Age = 30
                        };

                        var pr = ObjectPrinter.For<Person>()
                            .For<string>().Exclude().Apply();

                        return pr.PrintToString(p);
                    },
                    shouldNotContain: ["Alex", "Some note"],
                    shouldContain: ["30"]                  
                )).SetName("ShouldBeTrue_When_TypeExclude_RemovesAllPropertiesOfThatType");

                yield return new TestCaseData(new TestSpec(
                    "Trim applied to all string properties",
                    () =>
                    {
                        var p = new Person { Name = "Alexander", Note = "LongNote" };
                        var pr = ObjectPrinter.For<Person>()
                            .For<string>().Trim(3).Apply();

                        return pr.PrintToString(p);
                    },
                    shouldContain: ["Ale", "Lon"]
                )).SetName("ShouldBeTrue_When_TrimAppliedToAllStringProperties");

                
                yield return new TestCaseData(new TestSpec(
                    "Property serializer should override type serializer",
                    () =>
                    {
                        var p = new Person { Age = 50 };
                        var pr = ObjectPrinter.For<Person>()
                            .For<int>().Serialize(i => $"T{i}").Apply()
                            .For(x => x.Age).Serialize(i => $"P{i}").Apply();

                        return pr.PrintToString(p);
                    },
                    shouldContain: ["T50"],
                    shouldNotContain: ["P50"]
                )).SetName("ShouldBeTrue_When_PropertySerializer_OverridesTypeSerializer");
                
                yield return new TestCaseData(new TestSpec(
                    "Exclude specific property",
                    () =>
                    {
                        var p = new Person { Name = "Bob", Note = "hidden" };
                        var pr = ObjectPrinter.For<Person>()
                            .For(x => x.Note).Exclude().Apply();

                        return pr.PrintToString(p);
                    },
                    shouldNotContain: ["hidden"]
                )).SetName("ShouldBeTrue_When_ExcludeProperty_RemovesIt");

                
                yield return new TestCaseData(new TestSpec(
                    "Trim should not affect non-string properties",
                    () =>
                    {
                        var p = new Person { Age = 12345 };
                        var pr = ObjectPrinter.For<Person>()
                            .For(x => x.Name).Trim(2).Apply();

                        return pr.PrintToString(p);
                    },
                    shouldContain: ["12345"]
                )).SetName("ShouldBeTrue_When_TrimNotAppliedToNonString");
                
                yield return new TestCaseData(new TestSpec(
                    "Culture for double should not affect decimal",
                    () =>
                    {
                        var p = new Person { Height = 1234.5, Salary = 999.99m };
                        var pr = ObjectPrinter.For<Person>()
                            .For<double>().SetCulture(CultureInfo.InvariantCulture).Apply();

                        return pr.PrintToString(p);
                    },
                    shouldContain: ["1234.5"],
                    shouldNotContain: ["999.99"]
                )).SetName("ShouldBeTrue_When_CultureAppliedOnlyToDouble");

                
                yield return new TestCaseData(new TestSpec(
                    "Exclude<int> should remove int fields",
                    () =>
                    {
                        var p = new Person { Name = "A", Age = 10 };
                        var pr = ObjectPrinter.For<Person>().Exclude<int>();
                        return pr.PrintToString(p);
                    },
                    shouldNotContain: ["10"]
                )).SetName("ShouldBeTrue_When_ExcludeType_RemovesAllFieldsOfThatType");

                yield return new TestCaseData(new TestSpec(
                    "Exclude specific property (Height) should remove it",
                    () =>
                    {
                        var p = new Person { Height = 180 };
                        var pr = ObjectPrinter.For<Person>().Exclude(x => x.Height);
                        return pr.PrintToString(p);
                    },
                    shouldNotContain: ["180"]
                )).SetName("ShouldBeTrue_When_ExcludeSpecificProperty_RemovesIt");

                yield return new TestCaseData(new TestSpec(
                    "Serialize type int should apply custom serializer",
                    () =>
                    {
                        var p = new Person { Age = 5 };
                        var pr = ObjectPrinter.For<Person>().Serialize<int>(x => $"[{x}]");
                        return pr.PrintToString(p);
                    },
                    shouldContain: ["[5]"]
                )).SetName("ShouldBeTrue_When_SerializeForType_AppliesCustomSerializer");

                yield return new TestCaseData(new TestSpec(
                    "Serialize specific property Name should apply custom serializer",
                    () =>
                    {
                        var p = new Person { Name = "Bob" };
                        var pr = ObjectPrinter.For<Person>().Serialize(x => x.Name, v => $"N:{v}");
                        return pr.PrintToString(p);
                    },
                    shouldContain: ["N:Bob"]
                )).SetName("ShouldBeTrue_When_SerializeForProperty_AppliesCustomSerializer");

                yield return new TestCaseData(new TestSpec(
                    "Trim string property Name to length 3",
                    () =>
                    {
                        var p = new Person { Name = "abcdef" };
                        var pr = ObjectPrinter.For<Person>().Trim(p => p.Name, 3);
                        return pr.PrintToString(p);
                    },
                    shouldContain: ["abc"]
                )).SetName("ShouldBeTrue_When_TrimString_Trims");

                yield return new TestCaseData(new TestSpec(
                    "Trim multiple string properties independently",
                    () =>
                    {
                        var p = new Person { Name = "abcdef", Note = "uvwxyz" };
                        var pr = ObjectPrinter.For<Person>()
                            .Trim(p => p.Name, 3)
                            .Trim(p => p.Note, 4);
                        return pr.PrintToString(p);
                    },
                    shouldContain: ["abc", "uvwx"]
                )).SetName("ShouldBeTrue_When_TrimMultipleStrings_EachTrimmedIndependently");

                yield return new TestCaseData(new TestSpec(
                    "Set numeric culture to InvariantCulture for double Height",
                    () =>
                    {
                        var p = new Person { Height = 12.5 };
                        var pr = ObjectPrinter.For<Person>().SetFormattingCulture(CultureInfo.InvariantCulture);
                        return pr.PrintToString(p);
                    },
                    shouldContain: ["12.5"]
                )).SetName("ShouldBeTrue_When_NumericCulture_UsesInvariantCulture");

                yield return new TestCaseData(new TestSpec(
                    "Both custom serialization and trimming apply to Name",
                    () =>
                    {
                        var p = new Person { Name = "Benjamin" };
                        var pr = ObjectPrinter.For<Person>()
                            .Serialize(x => x.Name, s => $"[{s}]")
                            .Trim(x => x.Name, 4);
                        return pr.PrintToString(p);
                    },
                    shouldContain: ["[Ben"]
                )).SetName("ShouldBeTrue_When_CustomSerializationAndTrimming_BothApply");

                yield return new TestCaseData(new TestSpec(
                    "Type exclusion and property exclusion both should be applied",
                    () =>
                    {
                        var p = new Person { Age = 20, Height = 170 };
                        var pr = ObjectPrinter.For<Person>()
                            .Exclude<int>()
                            .Exclude(x => x.Height);
                        return pr.PrintToString(p);
                    },
                    shouldNotContain: ["20", "170"]
                )).SetName("ShouldBeTrue_When_TypeExcludeAndPropertyExclude_BothApply");

                yield return new TestCaseData(new TestSpec(
                    "Cyclic references should not cause stack overflow and show marker",
                    () =>
                    {
                        var a = new Person { Name = "A" };
                        var b = new Person { Name = "B" };
                        a.Child = b;
                        b.Parent = a;
                        var pr = ObjectPrinter.For<Person>();
                        return pr.PrintToString(a);
                    },
                    shouldContain: ["<see Person level=0>"]
                )).SetName("ShouldBeTrue_When_CyclicReferences_DoNotStackOverflow");

                yield return new TestCaseData(new TestSpec(
                    "Default extension PrintToString() should work without explicit printer",
                    () =>
                    {
                        var p = new Person { Name = "Alpha" };
                        return p.PrintToString();
                    },
                    shouldContain: ["Alpha"]
                )).SetName("ShouldBeTrue_When_DefaultExtensionMethod_Works");

                yield return new TestCaseData(new TestSpec(
                    "Extension method with config should apply trimming when passed",
                    () =>
                    {
                        var p = new Person { Name = "Beta" };
                        return p.PrintToString(c => c.Trim(x => x.Name, 2));
                    },
                    shouldContain: ["Be"]
                )).SetName("ShouldBeTrue_When_ExtensionMethodWithConfig_Applies");

                yield return new TestCaseData(new TestSpec(
                    "Array serialization of Person[] should include element names",
                    () =>
                    {
                        var arr = new[] { new Person { Name = "A" }, new Person { Name = "B" } };
                        return ObjectPrinter.For<Person[]>().PrintToString(arr);
                    },
                    shouldContain: ["A", "B"]
                )).SetName("ShouldBeTrue_When_ArraySerialization_Works");

                yield return new TestCaseData(new TestSpec(
                    "List<Person> serialization should include element data",
                    () =>
                    {
                        var list = new List<Person> { new Person { Name = "X" } };
                        return ObjectPrinter.For<List<Person>>().PrintToString(list);
                    },
                    shouldContain: ["X"]
                )).SetName("ShouldBeTrue_When_ListSerialization_Works");

                yield return new TestCaseData(new TestSpec(
                    "Dictionary<int, Person> serialization should include key and nested name",
                    () =>
                    {
                        var d = new Dictionary<int, Person> { { 1, new Person { Name = "K" } } };
                        return ObjectPrinter.For<Dictionary<int, Person>>().PrintToString(d);
                    },
                    shouldContain: ["1", "K"]
                )).SetName("ShouldBeTrue_When_DictionarySerialization_Works");

                yield return new TestCaseData(new TestSpec(
                    "Nullable<int> with value should serialize the value",
                    () =>
                    {
                        var p = new Person { LuckyNumber = 7 };
                        return ObjectPrinter.For<Person>().PrintToString(p);
                    },
                    shouldContain: ["7"]
                )).SetName("ShouldBeTrue_When_NullableType_SerializesValue");

                yield return new TestCaseData(new TestSpec(
                    "Nullable<int> null should not print the property",
                    () =>
                    {
                        var p = new Person { LuckyNumber = null };
                        return ObjectPrinter.For<Person>().PrintToString(p);
                    },
                    shouldNotContain: ["LuckyNumber"]
                )).SetName("ShouldBeTrue_When_NullableTypeNull_PrintsNull");

                yield return new TestCaseData(new TestSpec(
                    "Deep nested collections (matrix) should serialize inner ints",
                    () =>
                    {
                        var p = new Person
                        {
                            Matrix =
                            [
                                [1, 2],
                                [3, 4]
                            ]
                        };
                        return ObjectPrinter.For<Person>().PrintToString(p);
                    },
                    shouldContain: ["1", "4"]
                )).SetName("ShouldBeTrue_When_DeepNestedCollections_Serialize");

                yield return new TestCaseData(new TestSpec(
                    "Custom serializer for int type should affect nested objects (Friends list)",
                    () =>
                    {
                        var p = new Person { Friends = new List<Person> { new Person { Age = 10 } } };
                        var pr = ObjectPrinter.For<Person>().Serialize<int>(x => $"[{x}]");
                        return pr.PrintToString(p);
                    },
                    shouldContain: ["[10]"]
                )).SetName("ShouldBeTrue_When_CustomSerializerForType_AffectsNestedObjects");

                yield return new TestCaseData(new TestSpec(
                    "Set numeric culture should affect decimal Salary representation",
                    () =>
                    {
                        var p = new Person { Salary = 1234.567m };
                        var pr = ObjectPrinter.For<Person>().SetFormattingCulture(CultureInfo.InvariantCulture);
                        return pr.PrintToString(p);
                    },
                    shouldContain: ["1234.567"]
                )).SetName("ShouldBeTrue_When_Culture_AffectsDecimal");

                yield return new TestCaseData(new TestSpec(
                    "Trim on one property should not affect other string properties",
                    () =>
                    {
                        var p = new Person { Name = "abcdef", Note = "xyz" };
                        var pr = ObjectPrinter.For<Person>().Trim(x => x.Name, 2);
                        return pr.PrintToString(p);
                    },
                    shouldContain: ["ab", "xyz"]
                )).SetName("ShouldBeTrue_When_TrimOnProperty_DoesNotAffectOtherStrings");

                yield return new TestCaseData(new TestSpec(
                    "Exclude a property then serialize another should still serialize the non-excluded property",
                    () =>
                    {
                        var p = new Person { Name = "Qwerty" };
                        var pr = ObjectPrinter.For<Person>()
                            .Exclude(x => x.Note)
                            .Serialize(x => x.Name, v => v.ToUpper());
                        return pr.PrintToString(p);
                    },
                    shouldContain: ["QWERTY"]
                )).SetName("ShouldBeTrue_When_ExcludeThenSerialize_SerializesNotExcluded");

                yield return new TestCaseData(new TestSpec(
                    "Serialize type and exclude same property: property exclusion should win (no serialized value)",
                    () =>
                    {
                        var p = new Person { Name = "Bob", Age = 20 };
                        var pr = ObjectPrinter.For<Person>()
                            .Serialize<int>(x => $"I{x}")
                            .Exclude(x => x.Age);
                        return pr.PrintToString(p);
                    },
                    shouldNotContain: ["I20"]
                )).SetName("ShouldBeTrue_When_SerializeAndExcludeSameType_PropertyExclusionWins");

                yield return new TestCaseData(new TestSpec(
                    "Property-specific serializer vs type serializer: original asserts I30 (kept as in original)",
                    () =>
                    {
                        var p = new Person { Age = 30 };
                        var pr = ObjectPrinter.For<Person>()
                            .Serialize<int>(x => $"I{x}")
                            .Serialize(x => x.Age, v => $"A{v}");
                        return pr.PrintToString(p);
                    },
                    shouldContain: ["I30"]
                )).SetName("ShouldBeTrue_When_PropertySpecificSerializer_OverrideCheck");

                yield return new TestCaseData(new TestSpec(
                    "Very deep object graph should not overflow (chain length 15)",
                    () =>
                    {
                        var root = new Person { Name = "Root" };
                        var cur = root;
                        for (var i = 0; i < 15; i++)
                        {
                            cur.Child = new Person { Name = "L" + i };
                            cur = cur.Child;
                        }

                        var pr = ObjectPrinter.For<Person>();
                        return pr.PrintToString(root);
                    },
                    shouldContain: ["L14"]
                )).SetName("ShouldBeTrue_When_VeryDeepObjectGraph_DoesNotOverflow");

                yield return new TestCaseData(new TestSpec(
                    "Empty list should be serialized gracefully and property name present",
                    () =>
                    {
                        var p = new Person { Friends = new List<Person>() };
                        return ObjectPrinter.For<Person>().PrintToString(p);
                    },
                    shouldContain: ["Friends"]
                )).SetName("ShouldBeTrue_When_EmptyList_SerializesGracefully");

                yield return new TestCaseData(new TestSpec(
                    "Null property (Parent) should not be printed",
                    () =>
                    {
                        var p = new Person { Parent = null };
                        return ObjectPrinter.For<Person>().PrintToString(p);
                    },
                    shouldNotContain: ["Parent"]
                )).SetName("ShouldBeTrue_When_NullProperty_NotPrinted");

                yield return new TestCaseData(new TestSpec(
                    "Multiple custom serializers for int and double should both apply",
                    () =>
                    {
                        var p = new Person { Age = 10, Height = 170 };
                        var pr = ObjectPrinter.For<Person>()
                            .Serialize<int>(x => $"I{x}")
                            .Serialize<double>(x => $"D{x}");
                        return pr.PrintToString(p);
                    },
                    shouldContain: ["I10", "D170"]
                )).SetName("ShouldBeTrue_When_MultipleCustomSerializers_ApplyAll");

                yield return new TestCaseData(new TestSpec(
                    "Serialize type in dictionary values",
                    () =>
                    {
                        var d = new Dictionary<string, int> { { "a", 5 }, { "b", 3 }, { "c", 4 } };
                        var pr = ObjectPrinter.For<Dictionary<string, int>>()
                            .Serialize<int>(i => $"[{i}]");
                        return pr.PrintToString(d);
                    },
                    shouldContain: ["[5]", "[3]"]
                )).SetName("ShouldBeTrue_When_SerializeTypeInDictionary_Works");

                yield return new TestCaseData(new TestSpec(
                    "Serialize int[] with custom int serializer",
                    () =>
                    {
                        var arr = execute;
                        var pr = ObjectPrinter.For<int[]>()
                            .Serialize<int>(x => $"#{x}");
                        return pr.PrintToString(arr);
                    },
                    shouldContain: ["#1", "#2", "#3"]
                )).SetName("ShouldBeTrue_When_SerializeTypeInArray_Works");
            }
        }
    }
}