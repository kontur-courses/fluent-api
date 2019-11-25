using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
	public class PrintingConfig<TOwner>: IPrintingConfig<TOwner>
	{
		private const int MaxNestingLevel = 10;
		
		private readonly HashSet<Type> _excludedTypes = new HashSet<Type>();
		private readonly HashSet<string> _excludedProperties = new HashSet<string>();
		private readonly Dictionary<Type, Func<object, string>> _typesPrintingBehaviors =
			new Dictionary<Type, Func<object, string>>();
		private readonly Dictionary<string, Func<object, string>> _propertiesPrintingBehaviors =
			new Dictionary<string, Func<object, string>>();

		private readonly HashSet<Type> _finalTypes = new HashSet<Type>
		{
			typeof(int), typeof(double), typeof(float), typeof(string),
			typeof(DateTime), typeof(TimeSpan), typeof(bool)
		};

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
			var propertyName = $"{selectedMember.DeclaringType?.FullName}.{selectedMember.Name}";
			return propertyName;
		}

		internal PrintingConfig<TOwner> Excluding<TPropType>()
		{
			_excludedTypes.Add(typeof(TPropType));
			return this;
		}

		public string PrintToString(TOwner obj)
		{
			return PrintToString(obj, 0, new HashSet<object>());
		}

		private string PrintToString(object obj, int nestingLevel, HashSet<object> visitedObjects,
			PropertyInfo propertyInfo=null)
		{
			if (nestingLevel > MaxNestingLevel)
				return "Max nesting level exceeded" + Environment.NewLine;
			if (visitedObjects.Contains(obj))
				return "Circular reference" + Environment.NewLine;
			if (obj == null)
				return "null" + Environment.NewLine;
			
			var objType = obj.GetType();
			var stringRepresentation = ApplyPrintingBehavior(obj, objType, propertyInfo);
			if (objType.IsClass)
				visitedObjects.Add(obj);
			if (_finalTypes.Contains(objType))
				return stringRepresentation + Environment.NewLine;
			
			var sb = new StringBuilder();
			sb.AppendLine(stringRepresentation);
			if (obj is IEnumerable enumerable)
				PrintCollection(nestingLevel, sb, enumerable, visitedObjects);
			else
				PrintProperties(obj, nestingLevel, sb, visitedObjects);
			return sb.ToString();
		}

		private void PrintProperties(object obj, int nestingLevel, StringBuilder sb, HashSet<object> visitedObjects)
		{
			var objType = obj.GetType();
			var ident = new string('\t', nestingLevel + 1);
			
			foreach (var property in objType.GetProperties())
			{
				if (CheckExcluding(property))
					continue;
				var propertyString = PrintToString(property.GetValue(obj), nestingLevel + 1, visitedObjects, property);
				sb.Append(ident + property.Name + " = " + propertyString);
			}
		}

		private void PrintCollection(int nestingLevel, StringBuilder sb, IEnumerable enumerable, 
									HashSet<object> visitedObjects)
		{
			var ident = new string('\t', nestingLevel + 1);
			if (enumerable is IDictionary dictionary)
				foreach (DictionaryEntry pair in dictionary)
					sb.Append($"{ident}{pair.Key}: {PrintToString(pair.Value, nestingLevel + 1, Clone(visitedObjects))}");
			else
			{
				var index = 0;
				foreach (var item in enumerable)
					sb.Append($"{ident}[{index++}] {PrintToString(item, nestingLevel + 1, Clone(visitedObjects))}");
			}
		}

		private static HashSet<object> Clone(HashSet<object> oldHashSet) => new HashSet<object>(oldHashSet);

		private string ApplyPrintingBehavior(object obj, Type objType, PropertyInfo propertyInfo)
		{
			var result = objType.Name;
			if (objType.IsGenericType)
				result = objType.ToString();
			var propertyName = $"{propertyInfo?.DeclaringType?.FullName}.{propertyInfo?.Name}";
			if (propertyInfo != null && 
			    _propertiesPrintingBehaviors.TryGetValue(propertyName, out var printingBehavior))
				result = printingBehavior(obj);
			else if (_typesPrintingBehaviors.TryGetValue(objType, out printingBehavior))
				result = printingBehavior(obj);
			else if (_finalTypes.Contains(objType))
				return obj.ToString();
			return result;
		}

		private bool CheckExcluding(PropertyInfo propertyInfo)
		{
			return _excludedTypes.Contains(propertyInfo.PropertyType) ||
			       _excludedProperties.Contains($"{propertyInfo.DeclaringType?.FullName}.{propertyInfo.Name}");
		}
	}

	internal interface IPrintingConfig<TOwner>
	{
		Dictionary<string, Func<object, string>> PropertiesPrintingBehaviors { get; }
		Dictionary<Type, Func<object, string>> TypesPrintingBehaviors { get; }
		string GetPropertyName<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector);
	}
}