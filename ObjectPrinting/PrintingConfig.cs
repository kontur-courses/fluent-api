using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace ObjectPrinting
{
	public class PrintingConfig<TOwner> :
		IBaseConfig<TOwner>
	{
		private readonly Type[] finalTypes = new[]
		{
			typeof(int), typeof(double), typeof(float), typeof(string),
			typeof(DateTime), typeof(TimeSpan), typeof(Guid)
		};

        private readonly HashSet<object> objects;

		private List<Type> excludedTypes;
		private List<PropertyInfo> excludedProperties;

		private Dictionary<Type, Func<object, string>> serializedByType;
		private Dictionary<PropertyInfo, Func<object, string>> serializedByPropertyInfo;

		public PrintingConfig()
		{
            objects = new HashSet<object>();
            excludedTypes = new List<Type>();
			excludedProperties = new List<PropertyInfo>();
			serializedByType = new Dictionary<Type, Func<object, string>>();
			serializedByPropertyInfo = new Dictionary<PropertyInfo, Func<object, string>>();
		}

		public string PrintToString(TOwner obj)
		{
			return PrintToString(obj, 0);
		}

		private string PrintToString(object obj, int nestingLevel)
		{
			//TODO apply configurations
			if (obj == null)
				return "null" + Environment.NewLine;

			if (finalTypes.Contains(obj.GetType()))
				return obj + Environment.NewLine;

            if (objects.Contains(obj))
                return "cycled" + Environment.NewLine;
            objects.Add(obj);

			var identation = new string('\t', nestingLevel + 1);
			var sb = new StringBuilder();
			var type = obj.GetType();
			sb.AppendLine(type.Name);
			foreach (var propertyInfo in type.GetProperties())
			{
				var propType = propertyInfo.PropertyType;
                
				if (excludedProperties.Contains(propertyInfo) || excludedTypes.Contains(propType))
					continue;

				if (serializedByPropertyInfo.ContainsKey(propertyInfo))
				{
					var serializedValue = serializedByPropertyInfo[propertyInfo](propertyInfo.GetValue(obj));
					sb.Append(identation + propertyInfo.Name + " = " + serializedValue);
					continue;
				}

				if (serializedByType.ContainsKey(propType))
				{
					var serializedValue = serializedByType[propType](propertyInfo.GetValue(obj));
					sb.Append(identation + propertyInfo.Name + " = " + serializedValue);
					continue;
				}

				if (propType.BaseType == typeof(Array) || propType.GetInterfaces().Contains(typeof(IList)))
				{
					sb.Append(identation + propertyInfo.Name + ": ");
					sb.Append(SerializeListElements(propertyInfo.GetValue(obj), nestingLevel + 1));
					continue;
				}

				if (propType.GetInterfaces().Contains(typeof(IDictionary)))
				{
					sb.Append(identation + propertyInfo.Name + ": ");
					sb.Append(SerializeDictionaryElements(propertyInfo.GetValue(obj), nestingLevel + 1));
					continue;
				}

				sb.Append(identation + propertyInfo.Name + " = " +
				          PrintToString(propertyInfo.GetValue(obj),
					          nestingLevel + 1));
			}

			return sb.ToString();
		}

		private string SerializeDictionaryElements(object obj, int nesting)
		{
			var sb = new StringBuilder();
			var dictionary = obj as IDictionary;
			var identation = new string('\t', nesting + 1);
			if (dictionary == null || dictionary.Keys.Count == 0)
				return "<empty>" + Environment.NewLine;
			sb.Append(Environment.NewLine);
			var index = 1;
			foreach (var element in dictionary.Keys)
			{
				sb.AppendLine(identation + index++ + " element:");
				sb.Append(identation + "\tKey: " + PrintToString(element, nesting + 2));
				sb.Append(identation + "\tValue: " + PrintToString(dictionary[element], nesting + 2));
			}

			return sb.ToString();
		}

		private string SerializeListElements(object obj, int nesting)
		{
			var sb = new StringBuilder();
			var list = obj as IEnumerable;
			var identation = new string('\t', nesting + 1);
			if (list == null || !list.Cast<object>().Any())
				return "<empty>" + Environment.NewLine;
			sb.Append(Environment.NewLine);
			var index = 1;
			foreach (var element in list)
			{
				sb.Append(identation + index++ + ": " + PrintToString(element, nesting + 1));
			}

			return sb.ToString();
		}

		public IBaseConfig<TOwner> Exclude<TArg>(Expression<Func<TOwner, TArg>> f)
		{
			var configuratedProperty = Helper.GetPropertyInfo(f);
			excludedProperties.Add(configuratedProperty);
			return this;
		}

		public IBaseConfig<TOwner> Exclude<T>()
		{
			excludedTypes.Add(typeof(T));
			return this;
		}

		TypeConfig<TOwner, TArg> IBaseConfig<TOwner>.Printing<TArg>()
		{
			var configuratedType = typeof(TArg);
			return new TypeConfig<TOwner, TArg>(this, configuratedType);
		}

		public PropertyConfig<TOwner, TArg> Printing<TArg>(Expression<Func<TOwner, TArg>> f)
		{
			var configuratedProperty = Helper.GetPropertyInfo(f);
			return new PropertyConfig<TOwner, TArg>(this, configuratedProperty);
		}

		public void AddPropertySerialization<TProperty>(Func<TProperty, string> f, PropertyInfo configuratedProperty)
		{
			serializedByPropertyInfo.Add(configuratedProperty, obj => f((TProperty)obj));
		}

		public void AddTypeSerialization<TProperty>(Func<TProperty, string> f, Type configuratedType)
		{
			serializedByType.Add(configuratedType, obj => f((TProperty)obj));
		}
	}
}