using System;
using System.Globalization;
using System.Linq.Expressions;
using ObjectPrinting;

namespace ConsoleApp1
{
    class Program
    {
        public static void Main(string[] args)
        {
            double d = 10.5;
                Console.WriteLine(d.ToString(CultureInfo.InvariantCulture));
            
        }
        
        public static void TestMethod<TPropType>(Expression<Func<Person, TPropType>> memberSelector)
        {
            Console.WriteLine(memberSelector.Body.NodeType);
            Console.WriteLine(typeof(TPropType));
            Console.WriteLine(memberSelector.ToString());
        }
    }
}