namespace ObjectPrinting
{
    public class ObjectAndNestingLevel
    {
        public object Object;
        public int NestingLevel;

        public ObjectAndNestingLevel(object o, int nestingLevel)
        {
            Object = o;
            NestingLevel = nestingLevel;
        }
    }
}