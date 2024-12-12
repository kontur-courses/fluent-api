using System.Threading.Tasks;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

public class ObjectPrinterNestingClassTests : BaseVerifyTests
{
	[Test]
	public Task Recursive_Verify()
	{
		var person = new PersonWithParent
		{
			Name = "Alex",
			Surname = "Kash",
			Age = 19,
			Parent = new PersonWithParent
			{
				Name = "Bob",
				Surname = "Kash",
				Age = 40,
				Parent = new PersonWithParent()
			}
		};

		return Verify(person.PrintToString(p => p.SetDepth(2)));
	}
}