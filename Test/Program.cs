using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Test
{
	internal static class Program
	{
		public static void Main(string[] args)
		{
			var s1 = new string("John".ToCharArray());
			var s2 = new string("John".ToCharArray());
			Console.WriteLine(ReferenceEquals(s1, s2));
		}
	}

	class Person
	{
		public Name Name { get; }
	}

	internal class Name
	{
		public string Firstname { get; }
		public string Surname { get; }
	}

	internal static class Ex
	{
		public static void Printing<TOwner, TPropType>(this TOwner owner,
			Expression<Func<TOwner, TPropType>> memberSelector)
		{
		}
	}
}