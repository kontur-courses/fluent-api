using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterCollectionTests : BaseVerifyTests
{
	[Test]
	public Task SimpleCollection_Verify()
	{
		var simpleCollection = new List<int> { 1, 2, 3, 4, 5 };
		return Verify(simpleCollection.PrintToString());
	}

	[Test]
	public Task SimpleArray_Verify()
	{
		var simpleArray = new[] { 1, 2, 3, 4, 5 };
		return Verify(simpleArray.PrintToString());
	}

	[Test]
	public Task SimpleDictionary_Verify()
	{
		var simpleDictionary = new Dictionary<int, int> { { 1, 1 }, { 2, 2 } };
		return Verify(simpleDictionary.PrintToString());
	}

	[Test]
	public Task DifficultCollection_Verify()
	{
		var simpleDictionary = new Dictionary<int, string[]>
		{
			{ 1, ["str1", "str2"]},
			{ 2, ["str3", "str4"] },
		};
		return Verify(simpleDictionary.PrintToString());
	}

	[Test]
	public Task DictionaryWithClass_Verify()
	{
		var simpleDictionary = new Dictionary<int, List<Person>>
		{
			{ 1, [new Person(), new Person()] },
			{ 2, [new Person(), new Person()] },
		};
		return Verify(simpleDictionary.PrintToString());
	}
}