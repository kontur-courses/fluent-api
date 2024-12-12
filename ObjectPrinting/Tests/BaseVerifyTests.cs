using System.Threading.Tasks;
using NUnit.Framework;
using VerifyNUnit;
using VerifyTests;

namespace ObjectPrinting.Tests;

[TestFixture]
public class BaseVerifyTests
{
	private static readonly VerifySettings Settings = new();

	[OneTimeSetUp]
	public void OneTimeSetUp() =>
		Settings.UseDirectory("snapshots");

	protected static Task Verify(string target) =>
		Verifier.Verify(target, Settings);
}