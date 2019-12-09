using System;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedField.Global
// ReSharper disable UnusedMember.Global

namespace ObjectPrinting.Tests.TestTypes
{
    // class members used implicitly when calling PrintToString() for instance in PrintToStringTests class.
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