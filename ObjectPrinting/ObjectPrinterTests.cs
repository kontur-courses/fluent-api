using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private class Simple
        {
            public int Number { get; set; }
            public string Text { get; set; }
            public double Ratio { get; set; }
            public DateTime When { get; set; }
            public Guid Key { get; set; }
        }

        private class WithField
        {
            public int Prop { get; set; }
            public string Field;
            private string PrivateField;

            public WithField(int prop, string field, string pfield)
            {
                Prop = prop;
                Field = field;
                PrivateField = pfield;
            }
        }

        private class Node
        {
            public string Name { get; set; }
            public Node Next { get; set; }
        }

        private class Inner { public int Value { get; set; } }

        private class Outer { public string Name { get; set; } public Inner Child { get; set; } }

        private class CollectionHolder
        {
            public int[] Array { get; set; }
            public List<string> List { get; set; }
            public Dictionary<string, int> Dict { get; set; }
        }

        [Test]
        public void SerializesPublicProperties()
        {
            var s = new Simple { Number = 42, Text = "hi" };

            var result = ObjectPrinter.For<Simple>().PrintToString(s);

            result.Should().Contain("Number = 42");
            result.Should().Contain("Text = hi");
        }

        [Test]
        public void SerializesPublicFields()
        {
            var s = new WithField(1, "f", "pf");

            var result = ObjectPrinter.For<WithField>().PrintToString(s);

            result.Should().Contain("Prop = 1");
            result.Should().Contain("Field = f");
            result.Should().NotContain("PrivateField = pf");
        }

        [Test]
        public void FormatsGuid_DecimalDateTime()
        {
            var g = Guid.Parse("11111111-2222-3333-4444-555555555555");
            var s = new Simple { When = new DateTime(2020, 1, 2), Key = g };

            var result = ObjectPrinter.For<Simple>().PrintToString(s);

            result.Should().Contain(g.ToString());
            result.Should().Contain("2020");
        }

        [Test]
        public void NullObjectWillReturnNull()
        {
            var result = ObjectPrinter.For<Simple>().PrintToString(null);

            result.Should().Be("null");
        }

        [Test]
        public void NullStringProperty_PrintsNull()
        {
            var s = new Simple { Number = 1, Text = null };

            var result = ObjectPrinter.For<Simple>().PrintToString(s);

            result.Should().Contain("Text = null");
        }

        [Test]
        public void NestedObject_IsSerializedRecursively()
        {
            var o = new Outer { Name = "outer", Child = new Inner { Value = 7 } };

            var result = ObjectPrinter.For<Outer>().PrintToString(o);

            result.Should().Contain("Name = outer");
            result.Should().Contain("Value = 7");
        }

        [Test]
        public void ExcludeType_RemovesAllMembersOfThatType()
        {
            var s = new Simple { Number = 7, Text = "keep" };

            var result = ObjectPrinter.For<Simple>()
                .ExcludeType(typeof(int))
                .PrintToString(s);

            result.Should().NotContain("Number");
            result.Should().Contain("Text = keep");
        }

        [Test]
        public void AlternativeSerialization_OverridesTypeFormatting()
        {
            var s = new Simple { Number = 5 };

            var result = ObjectPrinter.For<Simple>()
                .AlternativeSerialization(typeof(int), i => i * 2)
                .PrintToString(s);

            result.Should().Contain("Number = 10");
            result.Should().NotContain("Number = 5");
        }

        [Test]
        public void SetNumericCulture_AffectsDouble()
        {
            var s = new Simple { Ratio = 1.5 };

            var invariant = ObjectPrinter.For<Simple>()
                .SetNumericCulture(CultureInfo.InvariantCulture)
                .PrintToString(s);

            var german = ObjectPrinter.For<Simple>()
                .SetNumericCulture(new CultureInfo("de-DE"))
                .PrintToString(s);

            invariant.Should().Contain("Ratio = 1.5");
            german.Should().Contain("Ratio = 1,5");
        }

        [Test]
        public void Truncate_ShortensLongStrings()
        {
            var s = new Simple { Text = "abcdefgh" };

            var result = ObjectPrinter.For<Simple>()
                .Truncate(3)
                .PrintToString(s);

            result.Should().Contain("Text = abc");
            result.Should().NotContain("abcdefgh");
        }

        [Test]
        public void Truncate_LeavesShortStringsIntact()
        {
            var s = new Simple { Text = "ab" };

            var result = ObjectPrinter.For<Simple>()
                .Truncate(5)
                .PrintToString(s);

            result.Should().Contain("Text = ab");
        }

        [Test]
        public void ExcludeMember_RemovesSpecificMember()
        {
            var s = new Simple { Number = 1, Text = "t" };

            var result = ObjectPrinter.For<Simple>()
                .ExcludeMember(x => x.Number)
                .PrintToString(s);

            result.Should().NotContain("Number");
            result.Should().Contain("Text = t");
        }

        [Test]
        public void SetMemberSerialization_Using_AppliesCustomSerializer()
        {
            var s = new Simple { Text = "abc" };

            var result = ObjectPrinter.For<Simple>()
                .SetMemberSerialization(x => x.Text)
                .Using(v => $"<{v}>")
                .PrintToString(s);

            result.Should().Contain("Text = <abc>");
        }

        [Test]
        public void SetMemberSerialization_TerminalPrintToString()
        {
            var s = new Simple { Number = 9, Text = "x" };

            var result = ObjectPrinter.For<Simple>()
                .SetMemberSerialization(x => x.Number)
                .PrintToString(s);

            result.Should().Contain("Number = 9");
        }

        [Test]
        public void SetMemberSerialization_Exclude_RemovesMember()
        {
            var s = new Simple { Number = 9, Text = "x" };

            var result = ObjectPrinter.For<Simple>()
                .SetMemberSerialization(x => x.Number)
                .Exclude()
                .PrintToString(s);

            result.Should().NotContain("Number");
            result.Should().Contain("Text = x");
        }

        [Test]
        public void SetMemberSerialization_CanContinueChain()
        {
            var s = new Simple { Number = 9, Text = "abc" };

            var result = ObjectPrinter.For<Simple>()
                .SetMemberSerialization(x => x.Text)
                .Using(v => v.ToUpper())
                .ExcludeType(typeof(int))
                .PrintToString(s);

            result.Should().Contain("Text = ABC");
            result.Should().NotContain("Number");
        }

        [Test]
        public void SetMemberSerialization_OverridesOtherRules()
        {
            // Member-сериализатор должен иметь приоритет над AlternativeSerialization типа
            var s = new Simple { Number = 3 };

            var result = ObjectPrinter.For<Simple>()
                .AlternativeSerialization(typeof(int), i => i * 100)
                .SetMemberSerialization(x => x.Number)
                .Using(n => n.ToString())
                .PrintToString(s);

            result.Should().Contain("Number = 3");
            result.Should().NotContain("Number = 300");
        }

        [Test]
        public void CyclicReference_DoesNotOverflow()
        {
            var a = new Node { Name = "A" };
            var b = new Node { Name = "B", Next = a };
            a.Next = b;

            string result = null;
            Action act = () => result = ObjectPrinter.For<Node>().PrintToString(a);

            act.Should().NotThrow();
            result.Should().Contain("Name = A");
            result.Should().Contain("Name = B");
            result.Should().Contain("<cycle>");
        }

        [Test]
        public void SelfCyclicReference_DoesNotOverflow()
        {
            var a = new Node { Name = "A" };
            a.Next = a;

            Action act = () => ObjectPrinter.For<Node>().PrintToString(a);

            act.Should().NotThrow();
        }

        [Test]
        public void Collections_Array_List_Dictionary()
        {
            var holder = new CollectionHolder
            {
                Array = new[] { 1, 2, 3 },
                List = new List<string> { "a", "b" },
                Dict = new Dictionary<string, int> { ["k1"] = 10, ["k2"] = 20 }
            };

            var result = ObjectPrinter.For<CollectionHolder>().PrintToString(holder);

            result.Should().Contain("1").And.Contain("3");
            result.Should().Contain("a").And.Contain("b");
            result.Should().Contain("k1").And.Contain("10");
            result.Should().Contain("k2").And.Contain("20");
        }

        [Test]
        public void EmptyCollection_DoesNotThrow()
        {
            var holder = new CollectionHolder
            {
                Array = Array.Empty<int>(),
                List = new List<string>(),
                Dict = new Dictionary<string, int>()
            };

            Action act = () => ObjectPrinter.For<CollectionHolder>().PrintToString(holder);

            act.Should().NotThrow();
        }

        [Test]
        public void ExtensionPrintToStringEqualsUsesDefaultConfig()
        {
            var s = new Simple { Number = 5 };

            var extensionResult = s.PrintToString();
            var result = ObjectPrinter.For<Simple>()
                .StandardSerialization()
                .PrintToString(s);
            extensionResult.Should().Contain("Number = 5");
            result.Should().Contain("Number = 5");
        }

        [Test]
        public void Config_CanBeReusedForMultipleObjects()
        {
            var config = ObjectPrinter.For<Simple>().Truncate(3);

            var a = config.PrintToString(new Simple { Text = "abcdef" });
            var b = config.PrintToString(new Simple { Text = "ghijkl" });

            a.Should().Contain("Text = abc");
            b.Should().Contain("Text = ghi");
        }

        [Test]
        public void Config_RepeatedTruncate_UsesLastValue()
        {
            var s = new Simple { Text = "abcdef" };

            var result = ObjectPrinter.For<Simple>()
                .Truncate(5)
                .Truncate(2)
                .PrintToString(s);

            result.Should().Contain("Text = ab");
            result.Should().NotContain("abcde");
        }
    }
}
