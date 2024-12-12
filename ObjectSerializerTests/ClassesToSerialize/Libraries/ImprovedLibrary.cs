using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectSerializerTests.ClassesToSerialize.Libraries
{
    public class ImprovedLibrary : BaseLibrary
    {
        public readonly Dictionary<string, Dictionary<string, int>> WordOccurenceByAuthor = new();

        public ImprovedLibrary(string name) : base(name)
        {
        }

        public void AddWordOccurenceByAuthor(string author, string word, int count)
        {
            if (!WordOccurenceByAuthor.ContainsKey(author))
            {
                WordOccurenceByAuthor[author] = new Dictionary<string, int>();
            }

            WordOccurenceByAuthor[author][word] = count;
        }
    }
}
