namespace ObjectPrinting
{
    public class SerializedObject
    {
        public readonly object Object;
        public readonly int NestingLevel;

        public SerializedObject(object obj, int nestingLevel)
        {
            Object = obj;
            NestingLevel = nestingLevel;
        }
    }
}
