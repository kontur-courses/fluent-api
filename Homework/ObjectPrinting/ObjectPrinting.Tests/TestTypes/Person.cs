using System;
using System.Diagnostics.CodeAnalysis;

namespace ObjectPrinting.Tests.TestTypes
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Property { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
    }
}