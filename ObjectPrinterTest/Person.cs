using System;
using System.Collections.Generic;
using System.Transactions;

namespace ObjectPrinterTest
{
    public class Person
    {
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public Guid Id { get; set; }
//        public override string ToString()
//        {
//            return "haha";
//        }
    }
    
    
    public class ClassWithFieldsAndProperty
    {
        public string Field1;
        public int Field2;
        public List<int> Property { get; set; }
        public ClassWithFieldsAndProperty Field3;
    }
    
    public class TestClass
    {
        public TestClass A { get; set; }
        public string Name { get; set; }
    }

    public class TestClassWithOneProperty
    {
        public List<TestClassWithOneProperty> Prop { get; set; }
    }
}