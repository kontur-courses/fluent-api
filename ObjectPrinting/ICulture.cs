using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public interface ICulture<TOwner>
    {
        public PrintingConfig<TOwner> SetCulture();
    }

    public class Culture<TOwner> : ICulture<TOwner>
    {
        public PrintingConfig<TOwner> SetCulture()
        {
            throw new NotImplementedException();
        }
    }
}
