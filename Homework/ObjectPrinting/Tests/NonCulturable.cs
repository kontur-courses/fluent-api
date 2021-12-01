using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting.Tests
{
    public class NonCulturable
    {
        public override string ToString()
        {
            return "Culture? Sorry, but I have never heard about it!";
        }
    }
}
