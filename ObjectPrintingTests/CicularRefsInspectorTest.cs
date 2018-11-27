using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using FluentAssertions;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    class CircularRefsInspectorTest
    {
        [Test]
        public void TestInspectObjectsWithNoCycles()
        {
            var person = new Person();
            var inspector = new CircularRefsInspector(
                person, new HashSet<Type>(), new HashSet<PropertyInfo>());

            var result = inspector.GetCircularlyReferencedObjects();

            result.Should().BeEmpty();
        }

        [Test]
        public void TestInspectingSimpleCycle()
        {
            var cycle = new Cycle();
            var inspector = new CircularRefsInspector(cycle, 
                new HashSet<Type>(), new HashSet<PropertyInfo>());

            var result = inspector.GetCircularlyReferencedObjects();

            result.Should().BeEquivalentTo(new HashSet<object>{cycle});
        }

        [Test]
        public void TestInspectingMoreComplexCycle()
        {
            // Not a cycle considering directed graph
            var d = new Node();
            var b = new Node {First = d};
            var c = new Node {First = d};
            var a = new Node {First = b, Second = c};

            var inspector = new CircularRefsInspector(a, 
                new HashSet<Type>(), new HashSet<PropertyInfo>());

            var result = inspector.GetCircularlyReferencedObjects();

            result.Should().BeEquivalentTo(new HashSet<object> {d});
        }
    }
}
