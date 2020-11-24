using System;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Configuration;

namespace Tests
{
    public class ObjectPrinterTests
    {
        [Test]
        public void CanPrintProperties()
        {
            var subject = new TestingPropertiesClass
            {
                Int32 = 18,
                Double = 123,
                Guid = Guid.NewGuid(),
                String = "Abc xyz"
            };

            ObjectPrinter.For<TestingPropertiesClass>()
                .Choose<int>()
                .Exclude()
                .PrintToString(subject)
                .Should()
                .ContainAll(
                    nameof(TestingPropertiesClass),
                    $"{nameof(TestingPropertiesClass.Guid)} = {subject.Guid}",
                    $"{nameof(TestingPropertiesClass.String)} = {subject.String}",
                    $"{nameof(TestingPropertiesClass.Double)} = {subject.Double}",
                    $"{nameof(TestingPropertiesClass.Int32)} = {subject.Int32}");
        }

        [Test]
        public void CanPrintFields()
        {
            var subject = new TestingFieldsClass
            {
                Int32 = 18,
                Double = 123,
                Guid = Guid.NewGuid(),
                String = "Abc xyz"
            };

            ObjectPrinter.For<TestingFieldsClass>()
                .Choose<int>()
                .Exclude()
                .PrintToString(subject)
                .Should()
                .ContainAll(
                    nameof(TestingFieldsClass),
                    $"{nameof(TestingFieldsClass.Guid)} = {subject.Guid}",
                    $"{nameof(TestingFieldsClass.String)} = {subject.String}",
                    $"{nameof(TestingFieldsClass.Double)} = {subject.Double}",
                    $"{nameof(TestingFieldsClass.Int32)} = {subject.Int32}");
        }

        [Test]
        public void PropertyExcluded_DoNotPrint()
        {
            var subject = new TestingPropertiesClass
            {
                Int32 = 18,
                Double = 123,
                Guid = Guid.NewGuid(),
                String = "Abc xyz"
            };

            ObjectPrinter.For<TestingPropertiesClass>()
                .Choose<int>()
                .Exclude()
                .PrintToString(subject)
                .Should()
                .NotContainAny($"{nameof(TestingPropertiesClass.Int32)}", $"{subject.Int32}");
        }

        [Test]
        public void PropertyGroupExcluded_DoNotPrint()
        {
            var subject = new TestingPropertiesClass
            {
                Int32 = 18,
                Double = 123,
                Guid = Guid.NewGuid(),
                String = "Abc xyz"
            };

            ObjectPrinter.For<TestingPropertiesClass>()
                .Choose(p => p.Guid)
                .Exclude()
                .PrintToString(subject)
                .Should()
                .NotContainAny($"{nameof(TestingPropertiesClass.Guid)}", $"{subject.Guid}");
        }
    }

    public class CascadeTestingClass
    {
        public int Int32 { get; set; }
        public string String { get; set; }
        public CascadeTestingClass Child { get; set; }
    }
}