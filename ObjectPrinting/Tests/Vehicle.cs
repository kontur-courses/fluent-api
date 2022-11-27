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
        private int YearOfCarProduction { get; }

        public int AgeOfTheCar { get; }

        public Vehicle(string name, int? power, double? weight, int yearOfCarProduction)
        {
            CarNumber = CarNumber++;
            Name = name;
            Power = power;
            Weight = weight;
            this.YearOfCarProduction = yearOfCarProduction;
            AgeOfTheCar = 2022 - this.YearOfCarProduction;
        }
    }
}
