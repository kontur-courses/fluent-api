using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class Pet
    {
        public string Name { get; set; } = "Барсик";
    }

    public class PetOwner
    {
        public string Name { get; set; } = "Александр";
        public Pet Pet { get; set; } = new();
    }
    
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        public class Sample
        {
            public string Name { get; set; } = "Алекс";
            public int Age { get; set; } = 19;
            public double Height { get; set; } = 1.75;
            public string Nickname = "Александр";
        }

        public class Node
        {
            public string Name { get; set; } = "";
            public Node? Next { get; set; }
            public Node? Other { get; set; }
        }

        [Test]
        public void PrintsPropertiesAndFields()
        {
            var result = ObjectPrinter.For<Sample>()
                .PrintToString(new Sample());

            Assert.That(result, Does.Contain("Name = Алекс"));
            Assert.That(result, Does.Contain("Age = 19"));
            Assert.That(result, Does.Contain("Nickname = Александр"));
        }

        [Test]
        public void PrintsNull()
        {
            var result = ObjectPrinter.For<Sample>()
                .PrintToString(null!);

            Assert.That(result, Is.EqualTo("null" + Environment.NewLine));
        }

        [Test]
        public void PrintsGuidValue()
        {
            var id = Guid.Parse("12345678-1234-1234-1234-123456789abc");

            var result = ObjectPrinter.For<Guid>().PrintToString(id);

            Assert.That(result,
                Is.EqualTo(id + Environment.NewLine));
        }

        [Test]
        public void ExcludesTypeFromPropertiesAndFields()
        {
            var result = ObjectPrinter.For<Sample>()
                .Excluding(typeof(string))
                .PrintToString(new Sample());

            Assert.That(result, Does.Not.Contain("Name ="));
            Assert.That(result, Does.Not.Contain("Nickname ="));
            Assert.That(result, Does.Contain("Age = 19"));
        }

        [TestCase(nameof(Sample.Name))]
        [TestCase(nameof(Sample.Nickname))]
        public void ExcludesMemberByName(string memberName)
        {
            var result = ObjectPrinter.For<Sample>()
                .Excluding(memberName)
                .PrintToString(new Sample());

            Assert.That(result, Does.Not.Contain(memberName + " ="));
            Assert.That(result, Does.Contain("Age = 19"));
        }

        [Test]
        public void UsesAlternativeSerializationForType()
        {
            var result = ObjectPrinter.For<Sample>()
                .WithAlternativeSerialization<string>(value => $"[{value}]")
                .PrintToString(new Sample());

            Assert.That(result, Does.Contain("Name = [Алекс]"));
            Assert.That(result, Does.Contain("Nickname = [Александр]"));
            Assert.That(result, Does.Contain("Age = 19" + Environment.NewLine));
        }

        [TestCase(nameof(Sample.Name), "Алекс")]
        [TestCase(nameof(Sample.Nickname), "Александр")]
        public void UsesAlternativeSerializationForMember(
            string memberName, string originalValue)
        {
            var result = ObjectPrinter.For<Sample>()
                .WithAlternativeSerialization<string>(
                    memberName, value => $"[{value}]")
                .PrintToString(new Sample());

            Assert.That(result,
                Does.Contain($"{memberName} = [{originalValue}]" + Environment.NewLine));

            Assert.That(result,
                Does.Not.Contain($"{memberName} = {originalValue}" + Environment.NewLine));
        }

        [Test]
        public void MemberSerializationOverridesTypeSerialization()
        {
            var result = ObjectPrinter.For<Sample>()
                .WithAlternativeSerialization<string>(value => $"[{value}]")
                .WithAlternativeSerialization<string>(
                    nameof(Sample.Name), value => $"Имя: {value}")
                .PrintToString(new Sample());

            Assert.That(result, Does.Contain("Name = Имя: Алекс"));
            Assert.That(result, Does.Contain("Nickname = [Александр]"));
        }

        [TestCase("ru-RU", "1,75")]
        [TestCase("en-US", "1.75")]
        public void UsesSelectedCulture(string cultureName, string expected)
        {
            var result = ObjectPrinter.For<Sample>()
                .WithCulture<double>(CultureInfo.GetCultureInfo(cultureName))
                .PrintToString(new Sample());

            Assert.That(result, Does.Contain("Height = " + expected));
        }

        [TestCase("Алекс", 3, "Але")]
        [TestCase("Алекс", 10, "Алекс")]
        [TestCase("Алекс", 0, "")]
        [TestCase("", 3, "")]
        [TestCase(null, 3, "null")]
        public void TrimsStringProperty(
            string? name, int length, string expected)
        {
            var sample = new Sample { Name = name! };

            var result = ObjectPrinter.For<Sample>()
                .Printing(p => p.Name)
                .Trimmed(length)
                .PrintToString(sample);

            Assert.That(result,
                Does.Contain("Name = " + expected + Environment.NewLine));
        }

        [Test]
        public void TrimsStringField()
        {
            var result = ObjectPrinter.For<Sample>()
                .Printing(p => p.Nickname)
                .Trimmed(3)
                .PrintToString(new Sample());

            Assert.That(result,
                Does.Contain("Nickname = Але" + Environment.NewLine));
        }

        [Test]
        public void RejectsNegativeTrimLength()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                ObjectPrinter.For<Sample>()
                    .Printing(p => p.Name)
                    .Trimmed(-1);
            });
        }

        [Test]
        public void DetectsReferenceToSelf()
        {
            var node = new Node { Name = "Корень" };
            node.Next = node;

            var result = ObjectPrinter.For<Node>().PrintToString(node);

            Assert.That(result, Does.Contain("Next = [cycle]"));
        }

        [Test]
        public void DetectsCycleBetweenObjects()
        {
            var first = new Node { Name = "Первый" };
            var second = new Node { Name = "Второй" };
            first.Next = second;
            second.Next = first;

            var result = ObjectPrinter.For<Node>().PrintToString(first);

            Assert.That(result, Does.Contain("Name = Второй"));
            Assert.That(result, Does.Contain("Next = [cycle]"));
        }

        [Test]
        public void SharedObjectInDifferentBranchesIsNotCycle()
        {
            var child = new Node { Name = "Потомок" };
            var root = new Node
            {
                Name = "Корень",
                Next = child,
                Other = child
            };

            var result = ObjectPrinter.For<Node>().PrintToString(root);

            Assert.That(result, Does.Not.Contain("[cycle]"));
            Assert.That(
                result.Split(new[] { "Name = Потомок" },
                    StringSplitOptions.None).Length - 1,
                Is.EqualTo(2));
        }
        
        [Test]
        public void PrintsArray()
        {
            var result = ObjectPrinter.For<int[]>()
                .PrintToString(new[] { 10, 20, 30 });

            Assert.That(result, Is.EqualTo(
                "Int32[]" + Environment.NewLine +
                "\t[0] = 10" + Environment.NewLine +
                "\t[1] = 20" + Environment.NewLine +
                "\t[2] = 30" + Environment.NewLine));
        }

        [Test]
        public void PrintsList()
        {
            var items = new List<string> { "Алекс", "Борис" };

            var result = ObjectPrinter.For<List<string>>()
                .PrintToString(items);

            Assert.That(result, Is.EqualTo(
                items.GetType().Name + Environment.NewLine +
                "\t[0] = Алекс" + Environment.NewLine +
                "\t[1] = Борис" + Environment.NewLine));
        }

        [Test]
        public void PrintsDictionary()
        {
            var items = new Dictionary<string, int>
            {
                ["Алекс"] = 19,
                ["Борис"] = 25
            };

            var result = ObjectPrinter.For<Dictionary<string, int>>()
                .PrintToString(items);

            Assert.That(result, Does.Contain(
                "\tКлюч: Алекс" + Environment.NewLine +
                "\tЗначение: 19" + Environment.NewLine));

            Assert.That(result, Does.Contain(
                "\tКлюч: Борис" + Environment.NewLine +
                "\tЗначение: 25" + Environment.NewLine));
        }

        [Test]
        public void PrintsEmptyArray()
        {
            var result = ObjectPrinter.For<int[]>()
                .PrintToString(Array.Empty<int>());

            Assert.That(result, Is.EqualTo("Int32[]" + Environment.NewLine));
        }

        [Test]
        public void PrintsEmptyList()
        {
            var items = new List<int>();

            var result = ObjectPrinter.For<List<int>>()
                .PrintToString(items);

            Assert.That(result,
                Is.EqualTo(items.GetType().Name + Environment.NewLine));
        }

        [Test]
        public void PrintsEmptyDictionary()
        {
            var items = new Dictionary<string, int>();

            var result = ObjectPrinter.For<Dictionary<string, int>>()
                .PrintToString(items);

            Assert.That(result,
                Is.EqualTo(items.GetType().Name + Environment.NewLine));
        }

        [Test]
        public void PrintsNullElement()
        {
            var items = new object?[] { null, "Алекс" };

            var result = ObjectPrinter.For<object?[]>()
                .PrintToString(items);

            Assert.That(result, Does.Contain("[0] = null"));
            Assert.That(result, Does.Contain("[1] = Алекс"));
        }

        [Test]
        public void PrintsObjectInsideCollection()
        {
            var items = new[] { new Sample() };

            var result = ObjectPrinter.For<Sample[]>()
                .PrintToString(items);

            Assert.That(result, Does.Contain("[0] = Sample"));
            Assert.That(result, Does.Contain("Name = Алекс"));
            Assert.That(result, Does.Contain("Age = 19"));
        }

        [Test]
        public void PrintsNestedCollections()
        {
            var items = new List<int[]>
            {
                new[] { 10, 20 },
                new[] { 30 }
            };

            var result = ObjectPrinter.For<List<int[]>>()
                .PrintToString(items);

            Assert.That(result, Is.EqualTo(
                items.GetType().Name + Environment.NewLine +
                "\t[0] = Int32[]" + Environment.NewLine +
                "\t\t[0] = 10" + Environment.NewLine +
                "\t\t[1] = 20" + Environment.NewLine +
                "\t[1] = Int32[]" + Environment.NewLine +
                "\t\t[0] = 30" + Environment.NewLine));
        }

        [Test]
        public void PrintsCollectionInsideDictionary()
        {
            var items = new Dictionary<string, List<int>>
            {
                ["Числа"] = new List<int> { 10, 20 }
            };

            var result = ObjectPrinter
                .For<Dictionary<string, List<int>>>()
                .PrintToString(items);

            Assert.That(result, Does.Contain("Ключ: Числа"));
            Assert.That(result, Does.Contain("\t\t[0] = 10"));
            Assert.That(result, Does.Contain("\t\t[1] = 20"));
        }

        [Test]
        public void PrintsNullDictionaryValue()
        {
            var items = new Dictionary<string, string?>
            {
                ["Отсутствует"] = null
            };

            var result = ObjectPrinter.For<Dictionary<string, string?>>()
                .PrintToString(items);

            Assert.That(result, Does.Contain(
                "Ключ: Отсутствует" + Environment.NewLine +
                "\tЗначение: null" + Environment.NewLine));
        }

        [Test]
        public void AppliesSerializationToCollectionElements()
        {
            var result = ObjectPrinter.For<int[]>()
                .WithAlternativeSerialization<int>(value => $"число: {value}")
                .PrintToString(new[] { 10, 20 });

            Assert.That(result, Does.Contain("[0] = число: 10"));
            Assert.That(result, Does.Contain("[1] = число: 20"));
        }

        [Test]
        public void AppliesCultureToCollectionElements()
        {
            var result = ObjectPrinter.For<double[]>()
                .WithCulture<double>(CultureInfo.GetCultureInfo("ru-RU"))
                .PrintToString(new[] { 1.75, 2.5 });

            Assert.That(result, Does.Contain("[0] = 1,75"));
            Assert.That(result, Does.Contain("[1] = 2,5"));
        }

        [Test]
        public void AppliesSerializationToDictionaryKeysAndValues()
        {
            var items = new Dictionary<string, int>
            {
                ["Алекс"] = 19
            };

            var result = ObjectPrinter.For<Dictionary<string, int>>()
                .WithAlternativeSerialization<string>(value => $"[{value}]")
                .WithAlternativeSerialization<int>(value => $"число: {value}")
                .PrintToString(items);

            Assert.That(result, Does.Contain("Ключ: [Алекс]"));
            Assert.That(result, Does.Contain("Значение: число: 19"));
        }

        [Test]
        public void DetectsArrayReferencingItself()
        {
            var items = new object[1];
            items[0] = items;

            var result = ObjectPrinter.For<object[]>()
                .PrintToString(items);

            Assert.That(result, Does.Contain("[0] = [cycle]"));
        }

        [Test]
        public void DetectsListReferencingItself()
        {
            var items = new List<object>();
            items.Add(items);

            var result = ObjectPrinter.For<List<object>>()
                .PrintToString(items);

            Assert.That(result, Does.Contain("[0] = [cycle]"));
        }

        [Test]
        public void DetectsDictionaryReferencingItself()
        {
            var items = new Dictionary<string, object>();
            items["Сам"] = items;

            var result = ObjectPrinter.For<Dictionary<string, object>>()
                .PrintToString(items);

            Assert.That(result, Does.Contain("Ключ: Сам"));
            Assert.That(result, Does.Contain("Значение: [cycle]"));
        }

        [Test]
        public void SharedCollectionInDifferentBranchesIsNotCycle()
        {
            var child = new[] { 10 };
            var items = new[] { child, child };

            var result = ObjectPrinter.For<int[][]>()
                .PrintToString(items);

            Assert.That(result, Does.Not.Contain("[cycle]"));

            var occurrences = result.Split(
                new[] { "\t\t[0] = 10" },
                StringSplitOptions.None).Length - 1;

            Assert.That(occurrences, Is.EqualTo(2));
        }
        
        [Test]
        public void MemberSerializationDoesNotAffectOtherTypes()
        {
            var result = ObjectPrinter.For<PetOwner>()
                .WithAlternativeSerialization<string>(
                    nameof(PetOwner.Name), name => $"Человек: {name}")
                .PrintToString(new PetOwner());

            Assert.That(result, Does.Contain("Name = Человек: Александр"));
            Assert.That(result, Does.Contain("Name = Барсик"));
            Assert.That(result, Does.Not.Contain("Человек: Барсик"));
        }

        [Test]
        public void MemberExclusionDoesNotAffectOtherTypes()
        {
            var result = ObjectPrinter.For<PetOwner>()
                .Excluding(nameof(PetOwner.Name))
                .PrintToString(new PetOwner());

            Assert.That(result, Does.Not.Contain("Name = Александр"));
            Assert.That(result, Does.Contain("Name = Барсик"));
        }

        [Test]
        public void TrimmingDoesNotAffectOtherTypes()
        {
            var result = ObjectPrinter.For<PetOwner>()
                .Printing(owner => owner.Name)
                .Trimmed(3)
                .PrintToString(new PetOwner());

            Assert.That(result,
                Does.Contain("Name = Але" + Environment.NewLine));
            Assert.That(result,
                Does.Contain("Name = Барсик" + Environment.NewLine));
        }
    }
}