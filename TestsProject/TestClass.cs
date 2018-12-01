namespace TestsProject
{
    public class StringTestClass
    {
        public string str;
        public string Str { get; set; }

        public StringTestClass()
        {
            this.Str = "String";
            this.str = "string";
        }
    }
}