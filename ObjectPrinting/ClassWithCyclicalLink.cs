using System.Drawing;

namespace ObjectPrinting
{
    public class ClassWithCyclicalLink
    {
        public ClassWithCyclicalLink CyclicalLinkObject;

        public ClassWithCyclicalLink()
        {
            CyclicalLinkObject = this;
        }
    }
}