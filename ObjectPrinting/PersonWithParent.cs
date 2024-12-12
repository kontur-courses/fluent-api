
using ObjectPrinting.Tests;

namespace ObjectPrinting;

public class PersonWithParent : Person
{
	public Person? Parent { get; set; }
}