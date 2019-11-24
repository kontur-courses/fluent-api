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
			Console.WriteLine(60.1.ToString(CultureInfo.CurrentCulture));
		}
	}

	class Person
	{
		public Name Name;
	}

	internal class Name
	{
		public string Firstname;
		public string Surname;
	}

	internal static class Ex
	{
		public static void Printing<TOwner, TPropType>(this TOwner owner,
			Expression<Func<TOwner, TPropType>> memberSelector)
		{
		}
	}
}