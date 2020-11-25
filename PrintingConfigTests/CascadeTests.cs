using System;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Configuration;
using PrintingConfigTests.TestingModels;

namespace PrintingConfigTests
{
    [TestFixture]
    public class CascadeTests
    {
        private string result;

        [Test]
        public void Default_CascadeSerialize()
        {
            var subject = CreateCascadeSubject();

            result = ObjectPrinter.For<CascadeTestingClass>()
                .Build()
                .PrintToString(subject);

            result.Should()
                .ContainAll(nameof(CascadeTestingClass),
                    $"{nameof(CascadeTestingClass.Int32)} = {subject.Int32}",
                    $"{nameof(CascadeTestingClass.String)} = {subject.String}",
                    $"{nameof(CascadeTestingClass.Int32)} = {subject.Child.Int32}",
                    $"{nameof(CascadeTestingClass.String)} = {subject.Child.String}",
                    $"{nameof(CascadeTestingClass.Child)} = null");
        }

        [Test]
        public void ParentSettingSpecified_ChildInheritSettings()
        {
            var subject = CreateCascadeSubject();

            result = ObjectPrinter.For<CascadeTestingClass>()
                .Choose(x => x.Int32)
                .Exclude()
                .Build()
                .PrintToString(subject);

            result.Should()
                .Contain($"{nameof(CascadeTestingClass.Int32)} = {subject.Child.Int32}")
                .And
                .NotContain(subject.Int32.ToString());
        }

        [Test]
        public void ChildSettingSpecified_ShouldNotAffectParent()
        {
            var subject = CreateCascadeSubject();

            result = ObjectPrinter.For<CascadeTestingClass>()
                .Choose(x => x.Child.Int32)
                .Exclude()
                .Build()
                .PrintToString(subject);

            result.Should()
                .Contain($"{nameof(CascadeTestingClass.Int32)} = {subject.Int32}");
        }

        [Test]
        public void CyclicReference_IgnoreAlreadyPrinted()
        {
            var subject = new CascadeTestingClass {Int32 = 1, String = "a"};
            subject.Child = subject;

            Action test = () => ObjectPrinter.For<CascadeTestingClass>()
                .Build()
                .PrintToString(subject);

            test.Should()
                .Throw<InvalidOperationException>()
                .WithMessage($"Cyclic reference in {typeof(CascadeTestingClass)}");
        }

        [TearDown]
        public void TearDown()
        {
            TestContext.Out.WriteLine(result);
        }

        private static CascadeTestingClass CreateCascadeSubject() => new CascadeTestingClass
        {
            Int32 = 1,
            String = "a",
            Child = new CascadeTestingClass
            {
                Int32 = 2,
                String = "b"
            }
        };
    }
}