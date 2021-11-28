using System;
using System.Linq;

namespace ObjectPrinting.Solved.PrintingConfiguration
{
    public static class FinalTypesKeeper
    {
        private static readonly Type[] FinalTypes =
        {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(string),

            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid)
        };

        public static bool IsFinalType(Type type)
        {
            return FinalTypes.Contains(type);
        }
    }
}