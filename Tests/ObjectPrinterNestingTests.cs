using System;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Configuration;
using Tests.TestingModels;

namespace Tests
{
    [TestFixture]
    public class ObjectPrinterNestingTests
    {
        private string result;

        [Test]
        public void Default_SerializeNestedProperties()
        {
            var subject = CreateNestingSubject();

            result = ObjectPrinter.For<NestedContainingTestingClass>()
                .Build()
                .PrintToString(subject);

            result.Should()
                .ContainAll(nameof(TestingPropertiesClass),
                    $"{nameof(NestedContainingTestingClass.Int)} = {subject.Int}",
                    $"{nameof(NestedContainingTestingClass.String)} = {subject.String}",
                    $"{nameof(NestedContainingTestingClass.Nested)} = {nameof(TestingPropertiesClass)}",
                    $"{nameof(TestingPropertiesClass.Double)} = {subject.Nested.Double}",
                    $"{nameof(TestingPropertiesClass.Int)} = {subject.Nested.Int}",
                    $"{nameof(TestingPropertiesClass.Guid)} = {subject.Nested.Guid}",
                    $"{nameof(TestingPropertiesClass.String)} = {subject.Nested.String}");
        }

        [Test]
        public void ChildPropertySerializerSpecified_UseIt()
        {
            var subject = CreateNestingSubject();

            result = ObjectPrinter.For<NestedContainingTestingClass>()
                .Choose(x => x.Nested.Guid)
                .UseSerializer(g => $"ggg {g.ToString()}")
                .Build()
                .PrintToString(subject);

            result.Should()
                .Contain($"{nameof(TestingPropertiesClass.Guid)} = ggg {subject.Nested.Guid}");
        }

        [Test]
        public void PropertyGroupSerializerSpecified_ApplyToAllLevels()
        {
            var subject = CreateNestingSubject();

            result = ObjectPrinter.For<NestedContainingTestingClass>()
                .Choose<int>()
                .Exclude()
                .Build()
                .PrintToString(subject);

            result.Should().NotContainAny(
                nameof(TestingPropertiesClass.Int),
                nameof(NestedContainingTestingClass.Int),
                subject.Int.ToString(),
                subject.Nested.Int.ToString());
        }

        [Test]
        public void NestedPropertySerializerSpecified_DoesntAffectedParent()
        {
            var subject = CreateNestingSubject();

            result = ObjectPrinter.For<NestedContainingTestingClass>()
                .Choose(x => x.Nested.Int)
                .UseSerializer(_ => "uip")
                .Build()
                .PrintToString(subject);

            result.Should()
                .Contain($"{nameof(NestedContainingTestingClass.Int)} = {subject.Int}");
        }

        [TearDown]
        public void TearDown()
        {
            TestContext.Out.WriteLine(result);
        }

        private static NestedContainingTestingClass CreateNestingSubject() => new NestedContainingTestingClass
        {
            Int = 123,
            String = "abc",
            Nested = new TestingPropertiesClass
            {
                Double = 45.67,
                Int = 890,
                Guid = Guid.NewGuid(),
                String = "xyz"
            }
        };
    }
}