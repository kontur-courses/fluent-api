using System;
using System.Diagnostics.CodeAnalysis;

namespace ObjectPrinting.Tests.TestTypes
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")] // public fields used implicitly
    public class TypeWithPublicFields
    {
        public readonly int NumberField;
        public readonly DateTime DateTimeField;

        public TypeWithPublicFields(int numberField, DateTime dateTimeField)
        {
            NumberField = numberField;
            DateTimeField = dateTimeField;
        }

        public string SomeProperty { get; set; }
    }
}