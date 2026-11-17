namespace ObjectPrinting
{
    internal static class PrintingFormat
    {
        internal const string NullValue = "null";
        internal const string CircularReferenceMarker = "[Circular reference]";
        internal const string MemberAssignmentSeparator = " = ";

        internal const string CollectionEntryPrefix = "[";
        internal const string CollectionEntrySuffix = "]";
        internal const string EmptyCollectionMarker = "[empty]";

        internal const char IndentationCharacter = '\t';
    }
}