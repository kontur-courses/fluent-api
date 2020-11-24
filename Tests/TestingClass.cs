using System;

namespace Tests
{
    public class TestingPropertiesClass
    {
        public Guid Guid { get; set; }
        public string String { get; set; }
        public double Double { get; set; }
        public int Int32 { get; set; }
    }

    public class TestingFieldsClass
    {
        public Guid Guid;
        public string String;
        public double Double;
        public int Int32;
    }
}