using System;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public int? Weight { get; set; }
        public double? Height { get; set; }
        public int Age { get; set; }
        public bool HaveCar { get; set; }
        public Person Parent { get; set; }
        private Vehicle Vehicle { get; set; }
        public Vehicle Car
        {
            get => HaveCar ? Vehicle : null;
            set
            {
                HaveCar = true;
                Vehicle = value;
            }
        }
    }
}