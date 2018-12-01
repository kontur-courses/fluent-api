using System;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Tests;

namespace TestsProject
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        [Test]
        public void ExcludeTypeShouldWork()
        {
            var strClass = new StringTestClass();
            ObjectPrinter
                .For<StringTestClass>()
                .Exclude<string>()
                .PrintToString(strClass)
                .Should()
                .Be("StringTestClass" + Environment.NewLine + Environment.NewLine);

        }

        [Test]
        public void ExcludePropertyShouldWork()
        {
            var strClass = new StringTestClass();
            ObjectPrinter
                .For<StringTestClass>()
                .Exclude(p => p.Str)
                .PrintToString(strClass)
                .Should()
                .Be("StringTestClass" + 
                    Environment.NewLine +
                    Environment.NewLine +
                    "\t\tstr = string" + 
                    Environment.NewLine);

        }

        [Test]
        public void ExcludeFieldShouldWork()
        {
            var strClass = new StringTestClass();
            ObjectPrinter
                .For<StringTestClass>()
                .Exclude(p => p.str)
                .PrintToString(strClass)
                .Should()
                .Be("StringTestClass" +
                    Environment.NewLine +
                    "\t\tStr = String" +
                    Environment.NewLine +
                    Environment.NewLine);

        }

        [Test]
        public void SerealiseTypeUsingLambdaShouldWork()
        {
            var strClass = new StringTestClass();
            ObjectPrinter
                .For<StringTestClass>()
                .Exclude(a => a.str)
                .Serialise<string>()
                .Using(a => "1")
                .PrintToString(strClass)
                .Should()
                .Be("StringTestClass" +
                    Environment.NewLine +
                    "\t\tStr = 1" +
                    Environment.NewLine);

        }

        [Test]
        public void SerealiseFieldUsingLambdaShouldWork()
        {
            var strClass = new StringTestClass();
            ObjectPrinter
                .For<StringTestClass>()
                .Exclude(a => a.Str)
                .Serialise(a => a.str)
                .Using(a => "1")
                .PrintToString(strClass)
                .Should()
                .Be("StringTestClass" +
                    Environment.NewLine +
                    Environment.NewLine +
                    "\t\tstr = 1");

        }

        [Test]
        public void SerealisePropertyUsingLambdaShouldWork()
        {
            var strClass = new StringTestClass();
            ObjectPrinter
                .For<StringTestClass>()
                .Exclude(a => a.str)
                .Serialise(a => a.Str)
                .Using(a => "1")
                .PrintToString(strClass)
                .Should()
                .Be("StringTestClass" +
                    Environment.NewLine +
                    "\t\tStr = 1" +
                    Environment.NewLine);

        }

        [Test]
        public void SerealisePropertyUsingTrimShouldWork()
        {
            var strClass = new StringTestClass();
            ObjectPrinter
                .For<StringTestClass>()
                .Exclude(a => a.str)
                .Serialise(a => a.Str)
                .Trimming(1)
                .PrintToString(strClass)
                .Should()
                .Be("StringTestClass" +
                    Environment.NewLine +
                    "\t\tStr = S" +
                    Environment.NewLine);
        }

        [Test]
        public void SerealiseFieldUsingTrimShouldWork()
        {
            var strClass = new StringTestClass();
            ObjectPrinter
                .For<StringTestClass>()
                .Exclude(a => a.Str)
                .Serialise(a => a.str)
                .Trimming(1)
                .PrintToString(strClass)
                .Should()
                .Be("StringTestClass" +
                    Environment.NewLine +
                    Environment.NewLine +
                    "\t\tstr = s");

        }
    }
}
