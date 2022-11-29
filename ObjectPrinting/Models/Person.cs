﻿using System;

namespace ObjectPrinting.Models
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public DateTime Birthday { get; set; }
        public Person Parent { get; set; }
    }
}