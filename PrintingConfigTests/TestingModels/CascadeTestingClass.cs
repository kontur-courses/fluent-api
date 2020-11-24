namespace PrintingConfigTests.TestingModels
{
    public class CascadeTestingClass
    {
        public int Int32 { get; set; }
        public string String { get; set; }
        public CascadeTestingClass Child { get; set; }
    }
}