namespace ObjectPrinting.Tests.Modules
{
    internal class NestingClass
    {
        public NestingClass Son { get; set; }

        public static NestingClass CreateNesting(int levelOfNesting)
        {
            var parent = new NestingClass();
            var tmp = parent;
            for (var i = 0; i < levelOfNesting; i++)
            {
                tmp.Son = new NestingClass();
                tmp = tmp.Son;
            }

            return parent;
        }
    }
}