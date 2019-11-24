using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
	public class PrintingConfig<TOwner>: IPrintingConfig<TOwner>
	{
		private readonly HashSet<Type> _excludedTypes = new HashSet<Type>();
		private readonly HashSet<string> _excludedProperties = new HashSet<string>();
		private readonly Dictionary<Type, Func<object, string>> _typesPrintingBehaviors =
			new Dictionary<Type, Func<object, string>>();
		private readonly Dictionary<string, Func<object, string>> _propertiesPrintingBehaviors =
			new Dictionary<string, Func<object, string>>();

		public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
		{
			return new PropertyPrintingConfig<TOwner, TPropType>(this);
		}

		public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
			Expression<Func<TOwner, TPropType>> memberSelector)
		{
			return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
		}

		public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
		{
			var propertyName = GetPropertyName(memberSelector);
			_excludedProperties.Add(propertyName);
			return this;
		}

		Dictionary<string, Func<object, string>> IPrintingConfig<TOwner>.PropertiesPrintingBehaviors =>
			_propertiesPrintingBehaviors;
		Dictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.TypesPrintingBehaviors =>
			_typesPrintingBehaviors;

		string IPrintingConfig<TOwner>.GetPropertyName<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector) =>
			GetPropertyName(memberSelector);

		private static string GetPropertyName<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
		{
			if (memberSelector.Body.NodeType != ExpressionType.MemberAccess)
				throw new ArgumentException("member selector type must be member access");
			var selectedMember = ((MemberExpression) memberSelector.Body).Member;
			var propertyName = $"{selectedMember.ReflectedType?.FullName}.{selectedMember.Name}";
			return propertyName;
		}

		internal PrintingConfig<TOwner> Excluding<TPropType>()
		{
			_excludedTypes.Add(typeof(TPropType));
			return this;
		}

		public string PrintToString(TOwner obj)
		{
			return PrintToString(obj, 0);
		}

		private string PrintToString(object obj, int nestingLevel)
		{
			if (obj == null)
				return "null" + Environment.NewLine;

			var finalTypes = new HashSet<Type>
			{
				typeof(int), typeof(double), typeof(float), typeof(string),
				typeof(DateTime), typeof(TimeSpan)
			};
			if (finalTypes.Contains(obj.GetType()))
				return obj + Environment.NewLine;

			var ident = new string('\t', nestingLevel + 1);
			var sb = new StringBuilder();
			var type = obj.GetType();
			sb.Append(ApplyPrintingBehavior(obj, type) + Environment.NewLine);
			foreach (var propertyInfo in type.GetProperties())
			{
				if (CheckExcluding(propertyInfo))
					continue;
				sb.Append(ident + propertyInfo.Name + " = " +
				          PrintToString(propertyInfo.GetValue(obj),
					          nestingLevel + 1));
			}
			return sb.ToString();
		}

		private string ApplyPrintingBehavior(object obj, Type objType)
		{
			var result = objType.Name;
			if (_propertiesPrintingBehaviors.TryGetValue(objType.FullName, out var printingBehavior))
				result = printingBehavior(obj);
			else if (_typesPrintingBehaviors.TryGetValue(objType, out printingBehavior))
				result = printingBehavior(obj);
			return result;
		}

		private bool CheckExcluding(PropertyInfo propertyInfo)
		{
			return _excludedTypes.Contains(propertyInfo.PropertyType) ||
			       _excludedProperties.Contains(propertyInfo.ReflectedType?.FullName + propertyInfo.Name);
		}
	}

	internal interface IPrintingConfig<TOwner>
	{
		Dictionary<string, Func<object, string>> PropertiesPrintingBehaviors { get; }
		Dictionary<Type, Func<object, string>> TypesPrintingBehaviors { get; }
		string GetPropertyName<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector);
	}
}