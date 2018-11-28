﻿using System;

namespace ObjectPrintingTests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }

        public Person Parent { get; set; }
        public Person Child { get; set; }
        public int[] LuckyNumbers { get; set; }
        public int Weight { get; set; }
        public float UselessField { get; set; }
        
    }
}