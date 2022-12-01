using NUnit.Framework;
using System;
using System.Collections.Generic;
using FluentAssertions;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    public class TypeExtensionsTests
    {
        enum TestedEnum { One=1, Two}

        [TestCase(typeof(char))]
        [TestCase(typeof(int))]
        [TestCase(typeof(double))]
        [TestCase(typeof(decimal))]
        [TestCase(typeof(Guid))]
        [TestCase(typeof(string))]
        [TestCase(typeof(DateTime))]
        [TestCase(typeof(int?))]
        [TestCase(typeof(Nullable<int>))]
        [TestCase(typeof(DateTimeOffset))]
        [TestCase(typeof(TimeSpan))]
        [TestCase(typeof(TestedEnum))]
        public void Should_BeTrue_When_TypeIs(Type type)
        {
            type.IsFinal().Should().BeTrue();
        }

        struct TestedStruct
        {
            int a;
        }

        class TestedClass
        {
            int a;
        }

        [TestCase(typeof(TestedStruct))]
        [TestCase(typeof(TestedClass))]
        [TestCase(typeof(char []))]
        [TestCase(typeof(List<int>))]
        [TestCase(typeof(List<int?>))]
        [TestCase(typeof(List<string>))]
        [TestCase(typeof(List<TestedClass>))]
        [TestCase(typeof(List<TestedStruct>))]
        [TestCase(typeof(IEnumerable<TestedStruct>))]
        public void Should_BeFalse_When_TypeIs(Type type)
        {
            type.IsFinal().Should().BeFalse();
        }
    }
}
