using System;

namespace ObjectPrinting.Tests
{
    internal static class PrintingTestFormat
    {
        internal const string NullValue = "null";
        internal const string CircularReferenceMarker = "[Circular reference]";
        internal const string MemberAssignmentSeparator = " = ";

        internal const string CollectionEntryPrefix = "[";
        internal const string CollectionEntrySuffix = "]";
        internal const string EmptyCollectionMarker = "[empty]";

        internal const char IndentationCharacter = '\t';

        internal static readonly string NewLine = Environment.NewLine;

        internal static string FormatCollectionEntry(string key, string value)
        {
            return CollectionEntryPrefix +
                   key +
                   CollectionEntrySuffix +
                   MemberAssignmentSeparator +
                   value;
        }
    }
}