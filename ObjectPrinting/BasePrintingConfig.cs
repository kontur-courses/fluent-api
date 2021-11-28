using System;

namespace ObjectPrinting
{
    public class BasePrintingConfig
    {
        protected static readonly Type[] FinalTypes = {
            typeof(int), 
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime), 
            typeof(TimeSpan)
        };
    }
}