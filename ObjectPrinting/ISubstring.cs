using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public interface ISubstring<TOwner>
    {
        public ISubstring<TOwner> Substring();
    }

    public class CutString<TOwner> : ISubstring<TOwner>
    {
        public ISubstring<TOwner> Substring()
        {
            throw new NotImplementedException();
        }
    }
}
