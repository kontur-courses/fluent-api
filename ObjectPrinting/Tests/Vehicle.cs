using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Tests
{
    public class Vehicle
    {
        public static int CarNumber { get; set; }
        public string Name { get; set; }
        public int? Power { get; set; }
        public double? Weight { get; set; }
        public int YearOfCarProduction { get; set; }

        public Vehicle(string name, int? power, double? weight, int yearOfCarProduction)
        {
            CarNumber = CarNumber++;
            Name = name;
            Power = power;
            Weight = weight;
            YearOfCarProduction = yearOfCarProduction;
        }
    }
}
